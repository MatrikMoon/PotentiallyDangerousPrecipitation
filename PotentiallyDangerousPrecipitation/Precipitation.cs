using IllusionPlugin;
using PotentiallyDangerousPrecipitation.HarmonyPatches;
using RoR2;
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
        public static bool OnlyForgiveMePlease { get; set; }

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
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                OnlyForgiveMePlease = !OnlyForgiveMePlease;
                Logger.Debug($"OnlyForgiveMePlease: {OnlyForgiveMePlease}");
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
                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, rng));

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
                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                for (var i = 0; i < 50; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, rng));

                Logger.Debug("Placed new Shrine of the Mountain (50x)");
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };

                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                directorCore.TrySpawnObject(new DirectorSpawnRequest(sceneDirector.teleporterSpawnCard, placementRule, rng));

                Logger.Debug("Placed new Teleporter");
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                for (var i = 0; i < 50; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(sceneDirector.teleporterSpawnCard, placementRule, rng));

                Logger.Debug("Placed new Teleporter (50x)");
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                Logger.Info("Activating all teleporters");

                //Find interactor of current player
                var bodies = Resources.FindObjectsOfTypeAll<CharacterBody>().Where(x => x.isPlayerControlled);
                var body = bodies.FirstOrDefault(x => Util.LookUpBodyNetworkUser(x).hasAuthority);

                if (body == null && bodies.Any())
                {
                    body = bodies.ElementAt(0);
                }

                if (body != null)
                {
                    var interactor = body.GetComponent<Interactor>();
                    var teleporterInteractions = Resources.FindObjectsOfTypeAll<TeleporterInteraction>();
                    foreach (var interaction in teleporterInteractions) interaction.OnInteractionBegin(interactor);
                }
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
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                foreach (var controller in voteControllers)
                {
                    if (controller.timerIsActive)
                    {
                        controller.InvokeMethod("FinishVote");
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                /*SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create(null);
                simpleDialogBox.AddCancelButton(CommonLanguageTokens.cancel, Array.Empty<object>());
                simpleDialogBox.AddActionButton(() =>
                {
                    //new Form1().Show();
                }, CommonLanguageTokens.ok, Array.Empty<object>());
                simpleDialogBox.headerLabel.text = "BAHAHA YOU PRESSED F";
                simpleDialogBox.descriptionLabel.text = "BAHAHA YOU PRESSED F, AND THIS IS A DESCRIPTION";*/

                Chat.AddMessage("</noparse><color=#e5eefc>HELLOAAAA</color><noparse>");
            }
        }
    }
}
