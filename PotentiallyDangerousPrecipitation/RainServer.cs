using PotentiallyDangerousPrecipitation.Extensions;
using PotentiallyDangerousPrecipitation.Utilities;
using Protos.Models;
using Protos.Models.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Action = Protos.Models.Action;
using Player = Protos.Models.Player;

namespace PotentiallyDangerousPrecipitation
{
    internal class RainServer : WsServer
    {
        public event Func<Player, Task> PlayerConnected;
        public event Func<Player, Task> PlayerDisconnected;
        public event Func<Player, Task> PlayerInfoUpdated;
        public event Func<Toggle, Task> ToggleUpdated;
        public event Func<Toggle, Task> ArtifactUpdated;

        private State State { get; set; }
        private Dictionary<string, Action<Action>> Actions { get; set; } = new Dictionary<string, Action<Action>>();

        public void AddToggle(string id, string name, bool defaultValue = false)
        {
            State.Toggles.Add(new Toggle()
            {
                Id = id,
                Name = name,
                Value = defaultValue
            });
        }

        public void AddAction(string id, string name, Action<Action> action, bool useTextBox = false, string textBoxLabel = "", bool useCheckBox = false, string checkBoxLabel = "")
        {
            State.Actions.Add(new Action()
            {
                Id = id,
                Name = name,
                TextBox = useTextBox,
                TextBoxLabel = textBoxLabel,
                CheckBox = useCheckBox,
                CheckBoxLabel = checkBoxLabel
            });
            Actions[id] = action;
        }

        public RainServer(int port) : base(port)
        {
            PacketReceived += WsServer_PacketReceived;

            State = new State();
        }

        public bool GetToggle(string id)
        {
            return State.Toggles.FirstOrDefault(x => x.Id == id)?.Value ?? false;
        }
        
        public void DoAction(Action action)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => Actions[action.Id](action));
        }

        #region EVENTS/ACTIONS
        public Task AddArtifact(string id, string name, bool enabled = false)
        {
            var artifact = new Toggle()
            {
                Id = id,
                Name = name,
                Value = enabled
            };

            lock (State.Artifacts)
            {
                State.Artifacts.Add(artifact);
            }

            var @event = new Event
            {
                artifact_added_event = new Event.ArtifactAddedEvent
                {
                    Artifact = artifact
                }
            };

            return Broadcast(new Packet
            {
                Event = @event
            });
        }

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

        public async Task UpdateArtifact(Toggle artifact)
        {
            lock (State)
            {
                var indexToReplace = State.Artifacts.FindIndex(x => x.Name == artifact.Name);
                State.Artifacts[indexToReplace] = artifact;
            }

            var @event = new Event
            {
                artifact_updated_event = new Event.ArtifactUpdatedEvent
                {
                    Artifact = artifact
                }
            };
            await Broadcast(new Packet
            {
                Event = @event
            });

            if (ArtifactUpdated != null) await ArtifactUpdated.Invoke(artifact);
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
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    var command = packet.Command;
                    switch (command.commandCase)
                    {
                        case Command.commandOneofCase.do_action_command:
                            {
                                DoAction(command.do_action_command.Action);
                                break;
                            }
                    }
                });
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
                    case Event.eventOneofCase.artifact_updated_event:
                        await UpdateArtifact(@event.artifact_updated_event.Artifact);
                        break;
                }
            }
        }
    }
}
