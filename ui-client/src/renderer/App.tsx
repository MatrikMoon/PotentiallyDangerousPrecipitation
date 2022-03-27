import { useCallback, useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { FButton } from './components/FButton/FButton';
import SpanWithEllipses from './components/SpanWithEllipses/spanWithEllipses';
import SplashScreen from './components/SplashScreen/SplashScreen';
import { Packet, Connect, Response } from './proto/packets';
import { proto } from './proto/models';
import MainPage from './components/MainPage/MainPage';
import { connectWs as connectToRainServer } from './RainClientWrapper';

const Main = () => {
    const [socket, setSocket] = useState<WebSocket | null>(null);
    const [connectedToGame, setConnectedToGame] = useState(false);
    const [ipaKnownInstalled, setIpaKnownInstalled] = useState(true); //Assume the game is installed/modded until told otherwise, so we don't prematurely show the install button

    //Toggles
    const [connectedPlayers, setConnectedPlayers] = useState<proto.models.Player[]>([]);
    const [toggles, setToggles] = useState<proto.models.Toggle[]>([]);

    const onWsMessage = useCallback(
        async (event: MessageEvent) => {
            const packet = Packet.deserializeBinary(await event.data.arrayBuffer());
            switch (packet.packet) {
                case 'connect_response': {
                    const connectResponse = packet.connect_response;
                    console.log(connectResponse.response.message);

                    if (connectResponse.response.type == Response.ResponseType.Success) {
                        setConnectedToGame(true);
                    }

                    setToggles(connectResponse.state.toggles);
                    setConnectedPlayers(connectResponse.state.players);
                    break;
                }
                case 'event': {
                    const _event = packet.event;
                    switch (_event.event) {
                        case 'player_added_event': {
                            const player_added_event = _event.player_added_event;
                            setConnectedPlayers([...connectedPlayers, player_added_event.player]);
                            break;
                        }
                        case 'player_left_event': {
                            const player_left_event = _event.player_left_event;
                            setConnectedPlayers(
                                connectedPlayers.filter((x) => x.user.id !== player_left_event.player.user.id)
                            );
                            break;
                        }
                        case 'player_updated_event': {
                            const player_updated_event = _event.player_updated_event;
                            const newPlayers = [...connectedPlayers];
                            const index = newPlayers.findIndex(
                                (x) => x.user.id === player_updated_event.player.user.id
                            );
                            newPlayers[index] = player_updated_event.player;
                            setConnectedPlayers(newPlayers);
                            break;
                        }
                        case 'toggle_updated_event': {
                            const toggle_updated_event = _event.toggle_updated_event;
                            const newToggles = [...toggles];
                            const index = newToggles.findIndex((x) => x.id === toggle_updated_event.toggle.id);
                            newToggles[index] = toggle_updated_event.toggle;
                            setToggles(newToggles);
                            break;
                        }
                    }
                }
            }
        },
        [socket, connectedToGame, connectedPlayers, toggles]
    );

    const onWsOpen = useCallback(() => {
        const packet = new Packet();
        packet.connect = new Connect();
        packet.connect.password = 'rain';
        socket!.send(packet.serializeBinary());
    }, [socket]);

    const onWsClose = () => {
        setConnectedToGame(false);
        connectToRainServer(onWsMessage, onWsOpen, onWsClose, onWsError, setSocket);
    };

    const onWsError = useCallback(
        (error: Event) => {
            console.error('Socket encountered error: ', error, 'Closing socket');
            socket!.close();
        },
        [socket]
    );

    //When the callbacks update, reassign them to the socket
    useEffect(() => {
        if (socket) {
            socket.onmessage = onWsMessage;
            socket.onopen = onWsOpen;
            socket.onclose = onWsClose;
            socket.onerror = onWsError;
        }
    }, [socket, onWsMessage, onWsOpen, onWsClose, onWsError]);

    useEffect(() => {
        connectToRainServer(onWsMessage, onWsOpen, onWsClose, onWsError, setSocket);
        (window as any).api.receive('ipa-installed-status', (data: any) => {
            setIpaKnownInstalled(data);
        });
    }, []);

    return (
        <div>
            {!connectedToGame && (
                <>
                    <SplashScreen />
                    {!ipaKnownInstalled && (
                        <div className='installButton'>
                            <FButton
                                onClick={() => {
                                    (window as any).api.send('install-ipa');
                                }}
                                selectedColor='#10c036'
                                selectedSecondaryColor='#a115e2'
                                text={'Install the Mod'}
                            />
                        </div>
                    )}
                    {ipaKnownInstalled && (
                        <div className='loadingText'>
                            <SpanWithEllipses text='Attempting to connect to game' />
                        </div>
                    )}
                </>
            )}
            {connectedToGame && <MainPage socket={socket} toggles={toggles} />}
        </div>
    );
};

export default function App() {
    return (
        <Router>
            <Routes>
                <Route path='/' element={<Main />} />
            </Routes>
        </Router>
    );
}
