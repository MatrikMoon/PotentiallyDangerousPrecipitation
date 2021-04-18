using HarmonyLib;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                            .GetTypes()
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

    [HarmonyPatch(typeof(GenericPickupController))]
    public class GenericPickupControllerPatch
    {
        [HarmonyPatch("GrantItem")]
        [HarmonyPatch(new[] { typeof(CharacterBody), typeof(Inventory) })]
        static bool Prefix(GenericPickupController __instance, CharacterBody body, Inventory inventory)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.GenericPickupController::GrantItem(RoR2.CharacterBody,RoR2.Inventory)' called on client");
                return false;
            }

            PickupDef pickupDef = PickupCatalog.GetPickupDef(__instance.pickupIndex);

            var count = Precipitation.HighStacks ? 10 : 1;
            inventory.GiveItem((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None, count);

            typeof(GenericPickupController).InvokeMethod("SendPickupMessage", inventory.GetComponent<CharacterMaster>(), __instance.pickupIndex);

            UnityEngine.Object.Destroy(__instance.gameObject);

            return false;
        }

        [HarmonyPatch(MethodType.Setter)]
        [HarmonyPatch("NetworkRecycled")]
        [HarmonyPatch(new[] { typeof(bool) })]
        static bool Prefix(GenericPickupController __instance, bool value)
        {
            if (Precipitation.InfiniteRecycling) Logger.Debug("Prevented recycler from setting!");
            return !Precipitation.InfiniteRecycling;
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
            if (Precipitation.PerfectLootChance)
            {
                __result = 100;
                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch]
    public class AntiLogRedirectPatch
    {
        static MethodBase TargetMethod()
        {
            //Patch the thing that steals logs from me
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName.Contains("Assembly-CSharp"));
            var redirectorType = assembly.GetType("RoR2.UnitySystemConsoleRedirector");
            return redirectorType.GetMethod("Redirect", ReflectionUtil._allBindingFlags);
        }

        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(BasicPickupDropTable))]
    public class BasicPickupDropTablePatch
    {
        [HarmonyPatch("GenerateDrop")]
        [HarmonyPatch(new[] { typeof(Xoroshiro128Plus) })]
        static bool Prefix(Xoroshiro128Plus rng, ref PickupIndex __result)
        {
            var original = typeof(SacrificeArtifactManager).GetField<PickupDropTable>("dropTable");
            if (Precipitation.PerfectFuelCellChance)
            {
                __result = rng.NextElementUniform(new List<PickupIndex>() {
                    PickupCatalog.FindPickupIndex("ItemIndex.EquipmentMagazine"),
                    PickupCatalog.FindPickupIndex("ItemIndex.AutoCastEquipment"),
                });
                return false;
            }
            else if (Precipitation.PerfectProcItemChance)
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
            else if (Precipitation.PerfectLegendaryChance)
            {
                var selector = original.GetField<WeightedSelection<List<PickupIndex>>>("selector");
                __result = rng.NextElementUniform(selector.choices[2].value);
                return false;
            }
            return true;
        }
    }
}
