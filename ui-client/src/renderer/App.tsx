import { ipcMain } from 'electron';
import { useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { FButton } from './components/FButton';
import SplashScreen from './components/splashScreen/SplashScreen';
import { Packet, Connect } from './proto/packets';

const Main = () => {
    const [socket, setSocket] = useState<WebSocket | null>(null);
    const [ipaInstalled, setIpaInstalled] = useState(false);

    useEffect(() => {
        const ws = new WebSocket('ws://127.0.0.1:10666');

        ws.onmessage = async (event: MessageEvent) => {
            const packet = Packet.deserializeBinary(await event.data.arrayBuffer());
            switch (packet.packet) {
                case 'connect_response': {
                    const connectResponse = packet.connect_response;
                    console.log(connectResponse.response.message);
                }
            }
        };

        ws.onopen = () => {
            const packet = new Packet();
            packet.connect = new Connect();
            packet.connect.password = 'rain';
            ws.send(packet.serializeBinary());
        };

        (window as any).api.receive('ipa-installed', (data: any) => {
            setIpaInstalled(true);
        });

        setSocket(ws);
    }, []);

    return (
        <div>
            {!ipaInstalled && (
                <>
                    <SplashScreen />
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
