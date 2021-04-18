using IllusionPlugin;
using PotentiallyDangerousPrecipitation.HarmonyPatches;
using RoR2;
using RoR2.UI;
using System;
using System.Linq;
using UnityEngine;

namespace PotentiallyDangerousPrecipitation
{
    class Precipitation : IPlugin
    {
        public string Name => "PotentiallyDangerousPrecipitation";
        public  string Version => "0.0.0";

        public static bool PerfectLootChance { get; set; }
        public static bool PerfectFuelCellChance { get; set; }
        public static bool PerfectProcItemChance { get; set; }
        public static bool PerfectLegendaryChance { get; set; }
        public static bool HighStacks { get; set; }
        public static bool InfiniteRecycling { get; set; }

        public void OnApplicationQuit()
        {

        }

        public void OnApplicationStart()
        {
            //View unity logs as well as my own
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;

            //Hook the item grant functionality
            Patches.Patch();
        }

        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {

            switch (type)
            {
                case LogType.Assert:
                case LogType.Warning:
                    Logger.Warning($"{condition}{stackTrace}");
                    break;
                case LogType.Log:
                    Logger.Warning($"{condition}{stackTrace}");
                    break;
                case LogType.Error:
                case LogType.Exception:
                    Logger.Warning($"{condition}{stackTrace}");
                    break;
            }
        }

        public void OnFixedUpdate()
        {
            
        }

        public void OnLevelWasInitialized(int level)
        {

        }

        public void OnLevelWasLoaded(int level)
        {

        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.CapsLock))
            {
                PerfectLootChance = !PerfectLootChance;
                Logger.Debug($"PerfectLootChance: {PerfectLootChance}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                PerfectFuelCellChance = !PerfectFuelCellChance;
                Logger.Debug($"PerfectFuelCellChance/ShellChance: {PerfectFuelCellChance}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PerfectProcItemChance = !PerfectProcItemChance;
                Logger.Debug($"PerfectProcItemChance: {PerfectProcItemChance}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PerfectLegendaryChance = !PerfectLegendaryChance;
                Logger.Debug($"PerfectLegendaryChance: {PerfectLegendaryChance}");
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                InfiniteRecycling = !InfiniteRecycling;
                Logger.Debug($"InfiniteRecycling: {InfiniteRecycling}");
            }
            if (Input.GetKeyDown(KeyCode.Numlock))
            {
                HighStacks = !HighStacks;
                Logger.Debug($"HighStacks: {HighStacks}");
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                var categories = ClassicStageInfo.instance.interactableCategories;
                var card = categories.categories.SelectMany(x => x.cards).First(x => x.spawnCard.name == "iscShrineBoss");

                directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, sceneDirector.GetField<Xoroshiro128Plus>("rng")));

                Logger.Debug("Placed new Shrine of the Mountain");
            }
            if (Input.GetKeyDown(KeyCode.N))
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                var categories = ClassicStageInfo.instance.interactableCategories;
                var card = categories.categories.SelectMany(x => x.cards).First(x => x.spawnCard.name == "iscShrineBoss");

                for (var i = 0; i < 50; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, sceneDirector.GetField<Xoroshiro128Plus>("rng")));

                Logger.Debug("Placed new Shrine of the Mountain (50x)");
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                Logger.Info("Giving all players lunar coins");
                var bodies = Resources.FindObjectsOfTypeAll<CharacterBody>().Where(x => x.isPlayerControlled);
                foreach (var body in bodies) Util.LookUpBodyNetworkUser(body).AwardLunarCoins(2000000000);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Logger.Info("Giving self lunar coins");
                NetworkUser.readOnlyInstancesList.Where(x => x.hasAuthority).First().AwardLunarCoins(2000000000);
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                foreach (var achievement in AchievementManager.allAchievementDefs)
                {
                    foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
                    {
                        AchievementManager.GetUserAchievementManager(user).GrantAchievement(achievement);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                foreach (var controller in voteControllers)
                {
                    controller.InvokeMethod("InitializeVoters");
                }
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create(null);
                simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok, Array.Empty<object>());
                simpleDialogBox.AddCommandButton("command!", CommonLanguageTokens.ok, Array.Empty<object>());
                simpleDialogBox.headerLabel.text = "BAHAHA YOU PRESSED F";
                simpleDialogBox.descriptionLabel.text = "BAHAHA YOU PRESSED F, AND THIS IS A DESCRIPTION";
            }
        }
    }
}
