import { Action, Player, Toggle } from "./proto/models";
import { Connect, Packet, Response } from "./proto/packets";
import { CustomEventEmitter } from "./utils/custom-event-emitter";
import { Semaphore } from "./utils/semaphore";

type wsMessage = (this: WebSocket, ev: MessageEvent<any>) => any;
// type wsOpen = (this: WebSocket, ev: Event) => any;
// type wsClose = (this: WebSocket, ev: CloseEvent) => any;
// type wsError = (this: WebSocket, ev: Event) => any;

const semaphore = new Semaphore(1); // Allow only 1 instance at a time

type RainClientEvents = {
    setConnectedToGame: boolean;
    setToggles: Toggle[];
    setActions: Action[];
    setArtifacts: Toggle[];
    setConnectedPlayers: Player[];
};

export class RainClientWrapper extends CustomEventEmitter<RainClientEvents> {
    private socket: WebSocket | undefined;
    private toggles: Toggle[] = [];
    // private actions: Action[] = [];
    private artifacts: Toggle[] = [];
    private players: Player[] = [];

    constructor() {
        super();

        this.onMessage = this.onMessage.bind(this);
        this.onOpen = this.onOpen.bind(this);
        this.onClose = this.onClose.bind(this);
        this.onError = this.onError.bind(this);
    }

    public connect() {
        this.socket = new WebSocket('ws://127.0.0.1:10666');

        const ourOnMessage = this.onMessage;
        const wrappedOnWsMessage: wsMessage = async function (this: WebSocket, ev: MessageEvent<any>) {
            await semaphore.use(async () => {
                await ourOnMessage.call(this, ev);
            });
        };

        this.socket.onmessage = wrappedOnWsMessage;
        this.socket.onopen = this.onOpen;
        this.socket.onclose = this.onClose;
        this.socket.onerror = this.onError;
    }

    public send(data: string | ArrayBufferLike | Blob | ArrayBufferView) {
        this.socket?.send(data);
    }

    private async onMessage(event: MessageEvent) {
        console.log('BEGIN');
        const packet = Packet.deserializeBinary(await event.data.arrayBuffer());
        switch (packet.packet) {
            case 'connect_response': {
                const connectResponse = packet.connect_response;
                console.log(connectResponse.response.message);

                if (connectResponse.response.type == Response.ResponseType.Success) {
                    this.emit('setConnectedToGame', true);
                }

                this.toggles = [];
                // this.actions = [];
                this.artifacts = [];
                this.players = [];

                this.emit('setToggles', connectResponse.state.toggles);
                this.emit('setActions', connectResponse.state.actions);
                this.emit('setArtifacts', connectResponse.state.artifacts);
                this.emit('setConnectedPlayers', connectResponse.state.players);
                break;
            }
            case 'event': {
                const _event = packet.event;
                switch (_event.event) {
                    case 'player_added_event': {
                        const player_added_event = _event.player_added_event;

                        this.players = [...this.players, player_added_event.player];
                        this.emit('setConnectedPlayers', this.players);
                        break;
                    }
                    case 'artifact_added_event': {
                        const artifact_added_event = _event.artifact_added_event;
                        console.log([...this.artifacts.map(x => x.name)], artifact_added_event.artifact.name);
                        this.artifacts = [...this.artifacts, artifact_added_event.artifact];
                        this.emit('setArtifacts', this.artifacts);
                        break;
                    }
                    case 'player_left_event': {
                        const player_left_event = _event.player_left_event;
                        this.players = this.players.filter((x) => x.user.id !== player_left_event.player.user.id);
                        this.emit('setConnectedPlayers', this.players);
                        break;
                    }
                    case 'player_updated_event': {
                        const player_updated_event = _event.player_updated_event;
                        const newPlayers = [...this.players];
                        const index = newPlayers.findIndex(
                            (x) => x.user.id === player_updated_event.player.user.id
                        );
                        newPlayers[index] = player_updated_event.player;
                        this.players = newPlayers;
                        this.emit('setConnectedPlayers', this.players);
                        break;
                    }
                    case 'toggle_updated_event': {
                        const toggle_updated_event = _event.toggle_updated_event;
                        const newToggles = [...this.toggles];
                        const index = newToggles.findIndex((x) => x.id === toggle_updated_event.toggle.id);
                        newToggles[index] = toggle_updated_event.toggle;
                        this.toggles = newToggles;
                        this.emit('setToggles', this.toggles);
                        break;
                    }
                    case 'artifact_updated_event': {
                        const artifact_updated_event = _event.artifact_updated_event;
                        const newArtifacts = [...this.artifacts];
                        const index = newArtifacts.findIndex((x) => x.id === artifact_updated_event.artifact.id);
                        newArtifacts[index] = artifact_updated_event.artifact;
                        this.artifacts = newArtifacts;
                        this.emit('setArtifacts', this.artifacts);
                        break;
                    }
                }
            }
        }
        console.log('ERND');
    }

    private async onOpen() {
        const packet = new Packet();
        packet.connect = new Connect();
        packet.connect.password = 'rain';
        this.socket!.send(packet.serializeBinary());
    }

    private async onClose() {
        this.emit('setConnectedToGame', false);
        this.connect();
    }

    private async onError(error: Event) {
        console.error('Socket encountered error: ', error, 'Closing socket');
        this.socket!.close();
    }
}