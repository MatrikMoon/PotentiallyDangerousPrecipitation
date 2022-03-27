using PotentiallyDangerousPrecipitation.Extensions;
using Protos.Models;
using Protos.Models.Packets;
using RoR2;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PotentiallyDangerousPrecipitation
{
    internal class RainServer : WsServer
    {
        public event Func<Player, Task> PlayerConnected;
        public event Func<Player, Task> PlayerDisconnected;
        public event Func<Player, Task> PlayerInfoUpdated;
        public event Func<Toggle, Task> ToggleUpdated;

        private static State State { get; set; }

        public RainServer(int port) : base(port)
        {
            PacketReceived += WsServer_PacketReceived;

            State = new State();

            //Default toggles
            State.Toggles.AddRange(
                new System.Collections.Generic.List<Toggle>
                {
                    new Toggle()
                    {
                        Id = "perfect_loot_chance",
                        Name = "Perfect Loot Chance",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "perfect_fuel_cell_chance",
                        Name = "Perfect Fuel Cell Chance",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "perfect_proc_item_chance",
                        Name = "Perfect Proc Item Chance",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "perfect_legendary_chance",
                        Name = "Perfect Legendary Chance",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "high_stacks",
                        Name = "High Stacks",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "infinite_recycling",
                        Name = "Infinite Recycling",
                        Value = false
                    },
                    new Toggle()
                    {
                        Id = "only_forgive_me_please",
                        Name = "Only Forgive Me Please",
                        Value = false
                    }
                });
        }

        public bool GetToggle(string id)
        {
            return State.Toggles.FirstOrDefault(x => x.Id == id)?.Value ?? false;
        }

        #region EVENTS/ACTIONS
        public async Task AddPlayer(Player player)
        {
            lock (State)
            {
                State.Players.Add(player);
            }

            var @event = new Event
            {
                player_added_event = new Event.PlayerAddedEvent
                {
                    Player = player
                }
            };
            await Broadcast(new Packet
            {
                Event = @event
            });

            if (PlayerConnected != null) await PlayerConnected.Invoke(player);
        }

        public async Task UpdatePlayer(Player player)
        {
            lock (State)
            {
                var playerToReplace = State.Players.FirstOrDefault(x => x.User.UserEquals(player.User));
                State.Players.Remove(playerToReplace);
                State.Players.Add(player);
            }

            var @event = new Event
            {
                player_updated_event = new Event.PlayerUpdatedEvent
                {
                    Player = player
                }
            };
            await Broadcast(new Packet
            {
                Event = @event
            });

            if (PlayerInfoUpdated != null) await PlayerInfoUpdated.Invoke(player);
        }

        public async Task RemovePlayer(Player player)
        {
            lock (State)
            {
                var playerToRemove = State.Players.FirstOrDefault(x => x.User.UserEquals(player.User));
                State.Players.Remove(playerToRemove);
            }

            var @event = new Event
            {
                player_left_event = new Event.PlayerLeftEvent
                {
                    Player = player
                }
            };
            await Broadcast(new Packet
            {
                Event = @event
            });

            if (PlayerDisconnected != null) await PlayerDisconnected.Invoke(player);
        }

        public async Task UpdateToggle(Toggle toggle)
        {
            lock (State)
            {
                var indexToReplace = State.Toggles.FindIndex(x => x.Name == toggle.Name);
                State.Toggles[indexToReplace] = toggle;
            }

            var @event = new Event
            {
                toggle_updated_event = new Event.ToggleUpdatedEvent
                {
                    Toggle = toggle
                }
            };
            await Broadcast(new Packet
            {
                Event = @event
            });

            if (ToggleUpdated != null) await ToggleUpdated.Invoke(toggle);
        }
        #endregion

        private async Task WsServer_PacketReceived(ConnectedUser user, Packet packet)
        {
            if (packet.packetCase == Packet.packetOneofCase.Connect)
            {
                var response = new Packet()
                {
                    ConnectResponse = new ConnectResponse()
                    {
                        Self = new User()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = "Moon"
                        },
                        Response = new Response()
                        {
                            Message = "Successfully connected to Rain server",
                            Type = Response.ResponseType.Success
                        },
                        State = State
                    }
                };
                await Send(user, response);
            }
            else if (packet.packetCase == Packet.packetOneofCase.Command)
            {
                var command = packet.Command;
                switch (command.commandCase)
                {
                    case Command.commandOneofCase.give_lunar_coins_command:
                        {
                            var giveLunarCoinsCommand = command.give_lunar_coins_command;
                            if (giveLunarCoinsCommand.ToEveryone)
                            {
                                Logger.Info("Giving all players lunar coins");
                                var bodies = Resources.FindObjectsOfTypeAll<CharacterBody>().Where(x => x.isPlayerControlled);
                                foreach (var body in bodies) Util.LookUpBodyNetworkUser(body).AwardLunarCoins((uint)giveLunarCoinsCommand.Amount);
                            }
                            else
                            {
                                Logger.Info("Giving self lunar coins");
                                NetworkUser.readOnlyInstancesList.Where(x => x.hasAuthority).First().AwardLunarCoins((uint)giveLunarCoinsCommand.Amount);
                            }
                            break;
                        }
                    case Command.commandOneofCase.spawn_mountain_shrine_command:
                        {
                            var spawnMountainShrineCommand = command.spawn_mountain_shrine_command;
                            var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                            var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.Random
                            };
                            var categories = ClassicStageInfo.instance.interactableCategories;
                            var card = categories.categories.SelectMany(x => x.cards).First(x => x.spawnCard.name == "iscShrineBoss");
                            var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                            for (var i = 0; i < spawnMountainShrineCommand.Amount; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(card.spawnCard, placementRule, rng));

                            Logger.Debug($"Placed new Shrine of the Mountain ({spawnMountainShrineCommand.Amount}x)");
                            break;
                        }
                    case Command.commandOneofCase.spawn_teleporter_command:
                        {
                            var spawnTeleporterCommand = command.spawn_teleporter_command;
                            var sceneDirector = Resources.FindObjectsOfTypeAll<SceneDirector>().FirstOrDefault();
                            var directorCore = Resources.FindObjectsOfTypeAll<DirectorCore>().FirstOrDefault();

                            DirectorPlacementRule placementRule = new DirectorPlacementRule
                            {
                                placementMode = DirectorPlacementRule.PlacementMode.Random
                            };
                            var rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                            for (var i = 0; i < spawnTeleporterCommand.Amount; i++) directorCore.TrySpawnObject(new DirectorSpawnRequest(sceneDirector.teleporterSpawnCard, placementRule, rng));

                            Logger.Debug($"Placed new Teleporter ({spawnTeleporterCommand.Amount}x)");
                            break;
                        }
                    case Command.commandOneofCase.start_teleporter_event_command:
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
                            break;
                        }
                    case Command.commandOneofCase.unlock_all_achievements_command:
                        {
                            foreach (var achievement in AchievementManager.allAchievementDefs)
                            {
                                foreach (var localUser in LocalUserManager.readOnlyLocalUsersList)
                                {
                                    AchievementManager.GetUserAchievementManager(localUser).GrantAchievement(achievement);
                                }
                            }
                            break;
                        }
                    case Command.commandOneofCase.skip_active_vote_command:
                        {
                            var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                            foreach (var controller in voteControllers)
                            {
                                if (controller.timerIsActive)
                                {
                                    controller.InvokeMethod("FinishVote");
                                }
                            }
                            break;
                        }
                    case Command.commandOneofCase.reset_active_vote_command:
                        {
                            var voteControllers = Resources.FindObjectsOfTypeAll<VoteController>();
                            foreach (var controller in voteControllers)
                            {
                                controller.InvokeMethod("InitializeVoters");
                            }
                            break;
                        }
                }
            }
            else if (packet.packetCase == Packet.packetOneofCase.Event)
            {
                var @event = packet.Event;
                switch (@event.eventCase)
                {
                    case Event.eventOneofCase.player_added_event:
                        await AddPlayer(@event.player_added_event.Player);
                        break;
                    case Event.eventOneofCase.player_updated_event:
                        await UpdatePlayer(@event.player_updated_event.Player);
                        break;
                    case Event.eventOneofCase.player_left_event:
                        await RemovePlayer(@event.player_left_event.Player);
                        break;
                    case Event.eventOneofCase.toggle_updated_event:
                        await UpdateToggle(@event.toggle_updated_event.Toggle);
                        break;
                }
            }
        }
    }
}
