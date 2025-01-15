using IllusionPlugin;
using PotentiallyDangerousPrecipitation.Extensions;
using PotentiallyDangerousPrecipitation.HarmonyPatches;
using PotentiallyDangerousPrecipitation.Utilities;
using RoR2;
using RoR2.ContentManagement;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Protos.Models;
using UnityEngine.Assertions.Must;

namespace PotentiallyDangerousPrecipitation
{
    class Precipitation : IPlugin
    {
        public string Name => "PotentiallyDangerousPrecipitation";
        public string Version => "0.0.0";

        public static RainServer RainServer { get; set; } = new RainServer(10666);

        private UnityMainThreadDispatcher _threadDispatcher;

        public void OnApplicationQuit()
        {

        }

        public void OnApplicationStart()
        {
            // View unity logs as well as my own
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;

            // Hook the item grant functionality
            Patches.Patch();

            ContentManager.onContentPacksAssigned += ContentManager_onContentPacksAssigned;

            // Set up toggles and actions
            // Default toggles
            RainServer.AddToggle("perfect_loot_chance", "Perfect Loot Chance");
            RainServer.AddToggle("perfect_loot_chance_for_me", "Perfect Loot Chance for Me");
            RainServer.AddToggle("perfect_fuel_cell_chance", "Perfect Fuel Cell Chance");
            RainServer.AddToggle("perfect_proc_item_chance", "Perfect Proc Item Chance");
            RainServer.AddToggle("perfect_legendary_chance", "Perfect Legendary Chance");
            RainServer.AddToggle("high_stacks", "High Stacks");
            RainServer.AddToggle("infinite_recycling", "Infinite Recycling");
            RainServer.AddToggle("only_forgive_me_please", "Only Forgive Me Please");

            RainServer.AddAction("give_lunar_coins_command", "Give Lunar Coins", (a) =>
            {
                if (a.Boolean)
                {
                    Logger.Debug($"Giving all players {a.Integer} lunar coins");
                    var bodies = Resources.FindObjectsOfTypeAll<CharacterBody>().Where(x => x.isPlayerControlled);
                    foreach (var body in bodies) Util.LookUpBodyNetworkUser(body).AwardLunarCoins((uint)a.Integer);
                }
                else
                {
                    Logger.Debug($"Giving self {a.Integer} lunar coins");
                    NetworkUser.readOnlyInstancesList.Where(x => x.hasAuthority).First().AwardLunarCoins((uint)a.Integer);
                }
            },
            useCheckBox: true, checkBoxLabel: "All Players",
            useTextBox: true, textBoxLabel: "Amount");

            RainServer.AddAction("spawn_mountain_shrine_command", "Spawn Shrine of the Mountain", (a) =>
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

                for (var i = 0; i < a.Integer; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, rng));

                Logger.Debug($"Placed new Shrine of the Mountain ({a.Integer}x)");
            }, useTextBox: true, textBoxLabel: "Amount");

            RainServer.AddAction("spawn_teleporter_command", "Spawn Teleporter", (a) =>
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                for (var i = 0; i < a.Integer; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(sceneDirector.teleporterSpawnCard, placementRule, rng));

                Logger.Debug($"Placed new Teleporter ({a.Integer}x)");
            }, useTextBox: true, textBoxLabel: "Amount");

            RainServer.AddAction("start_teleporter_event_command", "Start Teleporter Event", (a) =>
            {
                Logger.Debug("Activating all teleporters");

                // Find interactor of current player
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
            });

            RainServer.AddAction("unlock_all_achievements_command", "Unlock all Achievements", (a) =>
            {
                foreach (var achievement in AchievementManager.allAchievementDefs)
                {
                    foreach (var localUser in LocalUserManager.readOnlyLocalUsersList)
                    {
                        AchievementManager.GetUserAchievementManager(localUser).GrantAchievement(achievement);
                    }
                }
            });

            RainServer.AddAction("skip_active_vote_command", "Skip Active Vote", (a) =>
            {
                var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                foreach (var controller in voteControllers)
                {
                    if (controller.timerIsActive)
                    {
                        controller.InvokeMethod("FinishVote");
                    }
                }
            });

            RainServer.AddAction("reset_active_vote_command", "Reset Active Vote", (a) =>
            {
                var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                foreach (var controller in voteControllers)
                {
                    controller.InvokeMethod("InitializeVoters");
                }
            });

            RainServer.AddAction("promote_host_command", "Promote to Host", (a) =>
            {
                Console.instance.SubmitCmd(null, $"steam_lobby_assign_owner {a.String}", true);
            });

            RainServer.AddAction("evolve_lemurians_command", "Evolve Lemurians", (a) =>
            {
                var devotionInventoryController = Resources.FindObjectsOfTypeAll<DevotionInventoryController>().FirstOrDefault();
                if (devotionInventoryController != null)
                {
                    devotionInventoryController.ActivateDevotedEvolution();
                }
            });

            RainServer.ArtifactUpdated += RainServer_ArtifactUpdated;

            // Start the UI server
            Task.Run(RainServer.Start);
        }

        private Task RainServer_ArtifactUpdated(Toggle artifact)
        {
            var artifactManager = Resources.FindObjectsOfTypeAll<RunArtifactManager>().FirstOrDefault();
            if (artifactManager != null)
            {
                var artifactDef = ArtifactCatalog.FindArtifactDef(artifact.Name);
                if (artifact.Value != artifactManager.IsArtifactEnabled(artifactDef))
                {
                    artifactManager.SetArtifactEnabledServer(artifactDef, artifact.Value);
                }
            }
            return Task.CompletedTask;
        }

        private void ContentManager_onContentPacksAssigned(HG.ReadOnlyArray<ReadOnlyContentPack> readOnlyContentPacks)
        {
            // Add list of existing artifacts
            foreach (var contentPack in readOnlyContentPacks)
            {
                foreach (var artifact in contentPack.artifactDefs)
                {
                    System.Console.WriteLine("ONCONTENTPACKSASSIGNED - " + artifact.cachedName);
                    Task.Run(async () => await RainServer.AddArtifact(artifact.nameToken, artifact.cachedName));
                }
            }
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
            _threadDispatcher = _threadDispatcher ?? new GameObject("Thread Dispatcher").AddComponent<UnityMainThreadDispatcher>();
        }

        public void OnUpdate()
        {
            /*if (Input.GetKeyDown(KeyCode.F))
            {
                SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create(null);
                simpleDialogBox.AddCancelButton(CommonLanguageTokens.cancel, Array.Empty<object>());
                simpleDialogBox.AddActionButton(() =>
                {
                    //new Form1().Show();
                }, CommonLanguageTokens.ok, Array.Empty<object>());
                simpleDialogBox.headerLabel.text = "BAHAHA YOU PRESSED F";
                simpleDialogBox.descriptionLabel.text = "BAHAHA YOU PRESSED F, AND THIS IS A DESCRIPTION";

                Chat.AddMessage("</noparse><color=#e5eefc>HELLOAAAA</color><noparse>");
            }*/
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var bodies = Resources.FindObjectsOfTypeAll<CharacterBody>().Where(x => x.isPlayerControlled);
                foreach (var body in bodies) Util.LookUpBodyNetworkUser(body).AwardLunarCoins(1000000000);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Logger.Debug("Activating all teleporters");

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
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                for (var i = 0; i < 1; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(sceneDirector.teleporterSpawnCard, placementRule, rng));

                Logger.Debug($"Placed new Teleporter ({1}x)");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
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

                for (var i = 0; i < 10; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, rng));

                Logger.Debug($"Placed new Shrine of the Mountain ({10}x)");
            }
        }
    }
}
