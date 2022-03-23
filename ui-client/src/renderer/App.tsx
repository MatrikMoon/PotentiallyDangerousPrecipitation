import { useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { FButton } from './components/FButton/FButton';
import SpanWithEllipses from './components/SpanWithEllipses/spanWithEllipses';
import SplashScreen from './components/SplashScreen/SplashScreen';
import { Packet, Connect, Response } from './proto/packets';

const Main = () => {
    const [socket, setSocket] = useState<WebSocket | null>(null);
    const [connectedToGame, setConnectedToGame] = useState(false);
    const [ipaKnownInstalled, setIpaKnownInstalled] = useState(true); //Assume the game is installed/modded until told otherwise, so we don't prematurely show the install button

    const connectWs = () => {
        const ws = new WebSocket('ws://127.0.0.1:10666');

        ws.onmessage = async (event: MessageEvent) => {
            const packet = Packet.deserializeBinary(await event.data.arrayBuffer());
            switch (packet.packet) {
                case 'connect_response': {
                    const connectResponse = packet.connect_response;
                    console.log(connectResponse.response.message);

                    if (connectResponse.response.type == Response.ResponseType.Success) {
                        setConnectedToGame(true);
                    }
                }
            }
        };

        ws.onopen = () => {
            const packet = new Packet();
            packet.connect = new Connect();
            packet.connect.password = 'rain';
            ws.send(packet.serializeBinary());
        };

        ws.onclose = () => {
            setConnectedToGame(false);
            connectWs();
        };

        ws.onerror = (error: Event) => {
            console.error('Socket encountered error: ', error, 'Closing socket');
            ws.close();
        };

        setSocket(ws);
    };

    useEffect(() => {
        connectWs();
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
