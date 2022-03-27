using HarmonyLib;
using PotentiallyDangerousPrecipitation.Extensions;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

/**
 * Patching method taken appreciatively from Beat Saber Multiplayer on 10/15/2020 by Moon
 * Other methods crafted for Cross Platform purposes
 */

namespace PotentiallyDangerousPrecipitation.HarmonyPatches
{
    public static class Patches
    {
        public static void Patch()
        {
            var instance = new Harmony(typeof(Patches).FullName);

            Logger.Debug("Patching...");
            foreach (var type in Assembly.GetExecutingAssembly()
                            .GetLoadableTypes()
                            .Where(x => x.IsClass && x.Namespace == typeof(Patches).Namespace))
            {
                List<MethodInfo> patchedMethods = instance.CreateClassProcessor(type).Patch();
                if (patchedMethods != null && patchedMethods.Count > 0)
                {
                    foreach (var method in patchedMethods)
                    {
                        Logger.Debug($"Patched {method.DeclaringType}.{method.Name}!");
                    }
                }
            }
            Logger.Info("Applied patches!");
        }
    }

    [HarmonyPatch(typeof(ItemDef))]
    public class ItemDefPatch
    {
        [HarmonyPatch("AttemptGrant")]
        static bool Prefix(ref PickupDef.GrantContext context)
        {
            var highStacks = Precipitation.RainServer.GetToggle("high_stacks");

            Inventory inventory = context.body.inventory;
            PickupDef pickupDef = PickupCatalog.GetPickupDef(context.controller.pickupIndex);
            inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, highStacks ? 10 : 1);
            context.shouldDestroy = true;
            context.shouldNotify = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(GenericPickupController))]
    public class GenericPickupControllerPatch
    {
        private class PickupMessage : MessageBase
        {
            public void Reset()
            {
                masterGameObject = null;
                pickupIndex = PickupIndex.none;
                pickupQuantity = 0u;
            }

            public PickupMessage()
            {
            }

            public override void Serialize(NetworkWriter writer)
            {
                writer.Write(masterGameObject);
                GeneratedNetworkCode._WritePickupIndex_None(writer, pickupIndex);
                writer.WritePackedUInt32(pickupQuantity);
            }

            public override void Deserialize(NetworkReader reader)
            {
                masterGameObject = reader.ReadGameObject();
                pickupIndex = GeneratedNetworkCode._ReadPickupIndex_None(reader);
                pickupQuantity = reader.ReadPackedUInt32();
            }

            public GameObject masterGameObject;

            public PickupIndex pickupIndex;

            public uint pickupQuantity;
        }

        [HarmonyPatch("SendPickupMessage")]
        [HarmonyPatch(new[] { typeof(CharacterMaster), typeof(PickupIndex) })]
        static bool Prefix(CharacterMaster master, PickupIndex pickupIndex)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::SendPickupMessage(RoR2.CharacterMaster,RoR2.PickupIndex)' called on client");
                return false;
            }

            var highStacks = Precipitation.RainServer.GetToggle("high_stacks");

            uint pickupQuantity = highStacks ? 10u : 1u;
            if (master.inventory && !highStacks)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
                ItemIndex itemIndex = (pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None;
                if (itemIndex != ItemIndex.None)
                {
                    pickupQuantity = (uint)master.inventory.GetItemCount(itemIndex);
                }
            }

            var msg = new PickupMessage
            {
                masterGameObject = master.gameObject,
                pickupIndex = pickupIndex,
                pickupQuantity = pickupQuantity
            };
            NetworkServer.SendByChannelToAll(57, msg, RoR2.Networking.QosChannelIndex.chat.intVal);

            return false;
        }

        [HarmonyPatch(MethodType.Setter)]
        [HarmonyPatch("NetworkRecycled")]
        [HarmonyPatch(new[] { typeof(bool) })]
        static bool Prefix(GenericPickupController __instance, bool value)
        {
            var toggle = Precipitation.RainServer.GetToggle("infinite_recycling");
            if (toggle) Logger.Debug("Prevented recycler from setting!");
            return !toggle;
        }
    }

    [HarmonyPatch(typeof(Util))]
    public class UtilPatch
    {
        [HarmonyPatch("EscapeRichTextForTextMeshPro")]
        [HarmonyPatch(new[] { typeof(string) })]
        static bool Prefix(string rtString, ref string __result)
        {
            __result = "<noparse>" + rtString + "</noparse>";
            return false;
        }

        [HarmonyPatch("GetExpAdjustedDropChancePercent")]
        [HarmonyPatch(new[] { typeof(float), typeof(GameObject)})]
        static bool Prefix(ref float __result)
        {
            var toggle = Precipitation.RainServer.GetToggle("perfect_loot_chance");
            if (toggle)
            {
                __result = 100;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch]
    public class AntiLogRedirectPatch
    {
        static MethodBase TargetMethod()
        {
            //Patch the thing that steals logs from me
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName.Contains("RoR2"));
            var redirectorType = assembly.GetType("RoR2.UnitySystemConsoleRedirector");
            return redirectorType.GetMethod("Redirect", ReflectionUtil._allBindingFlags);
        }

        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(PickupDropTable))]
    public class PickupDropTablePatch
    {
        [HarmonyPatch("GenerateDrop")]
        [HarmonyPatch(new[] { typeof(Xoroshiro128Plus) })]
        static bool Prefix(Xoroshiro128Plus rng, ref PickupIndex __result)
        {
            var perfectFuelCellChance = Precipitation.RainServer.GetToggle("perfect_fuel_cell_chance");
            var perfectProcItemChance = Precipitation.RainServer.GetToggle("perfect_proc_item_chance");
            var perfectLegendaryChance = Precipitation.RainServer.GetToggle("perfect_legendary_chance");
            var onlyForgiveMePlease = Precipitation.RainServer.GetToggle("only_forgive_me_please");

            if (perfectFuelCellChance)
            {
                __result = rng.NextElementUniform(new List<PickupIndex>() {
                    PickupCatalog.FindPickupIndex("ItemIndex.EquipmentMagazine"),
                    PickupCatalog.FindPickupIndex("ItemIndex.AutoCastEquipment"),
                });
                return false;
            }
            else if (perfectProcItemChance)
            {
                __result = rng.NextElementUniform(new List<PickupIndex>() {
                    PickupCatalog.FindPickupIndex("ItemIndex.ChainLightning"),
                    PickupCatalog.FindPickupIndex("ItemIndex.Missile"),
                    PickupCatalog.FindPickupIndex("ItemIndex.Dagger"),
                    PickupCatalog.FindPickupIndex("ItemIndex.ShockNearby"),
                    PickupCatalog.FindPickupIndex("ItemIndex.BounceNearby"),
                    PickupCatalog.FindPickupIndex("ItemIndex.Icicle"),
                    PickupCatalog.FindPickupIndex("ItemIndex.LaserTurbine"),
                    PickupCatalog.FindPickupIndex("ItemIndex.NovaOnHeal"),
                    PickupCatalog.FindPickupIndex("ItemIndex.Thorns"),
                    PickupCatalog.FindPickupIndex("ItemIndex.BurnNearby")
                });
                return false;
            }
            else if (perfectLegendaryChance)
            {
                __result = rng.NextElementUniform(Run.instance.availableTier3DropList);
                return false;
            }
            else if (onlyForgiveMePlease)
            {
                __result = rng.NextElementUniform(new List<PickupIndex>() {
                    PickupCatalog.FindPickupIndex("EquipmentIndex.SoulCorruptor"),
                    PickupCatalog.FindPickupIndex("EquipmentIndex.QuestVolatileBattery"),
                    PickupCatalog.FindPickupIndex("EquipmentIndex.CrippleWard"),
                    PickupCatalog.FindPickupIndex("EquipmentIndex.DeathProjectile")
                });
                return false;
            }
            return true;
        }
    }
}
