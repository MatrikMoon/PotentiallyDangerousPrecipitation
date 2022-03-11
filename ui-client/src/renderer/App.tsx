import { useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import SplashScreen from './components/splashScreen/SplashScreen';

const Main = () => {
    const [socket, setSocket] = useState<WebSocket | null>(null);

    useEffect(() => {
        const ws = new WebSocket('ws://localhost:10666');

        ws.onmessage = (event: MessageEvent) => {};

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
