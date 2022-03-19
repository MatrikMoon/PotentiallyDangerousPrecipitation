import { useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import SplashScreen from './components/splashScreen/SplashScreen';
import { Packet, Connect } from './proto/packets';

const Main = () => {
    const [socket, setSocket] = useState<WebSocket | null>(null);

    useEffect(() => {
        const ws = new WebSocket('ws://127.0.0.1:10666');

        ws.onmessage = (event: MessageEvent) => {
            const packet = Packet.deserializeBinary(event.data);
            switch (packet.packet) {
                case 'connect_response': {
                    const connectResponse = packet.connect_response;
                    console.log(connectResponse.response.message);
                }
            }
        };

        ws.onopen = () => {
            const connect = new Connect();
            connect.password = 'rain';
            console.log('Sending:');
            console.log(connect.serializeBinary());
            ws.send(connect.serializeBinary());
        };

        setSocket(ws);
    }, []);

    return (
        <div>
            <SplashScreen />
            {/* <FButton selectedColor='#10c036' selectedSecondaryColor='#a115e2' text={'START QUIZ'} /> */}
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
