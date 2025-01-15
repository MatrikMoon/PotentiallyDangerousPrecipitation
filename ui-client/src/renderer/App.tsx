import { useEffect, useState } from 'react';
import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.scss';
import { FButton } from './components/FButton/FButton';
import SpanWithEllipses from './components/SpanWithEllipses/SpanWithEllipses';
import SplashScreen from './components/SplashScreen/SplashScreen';
import TabControl from './pages/TabControl/TabControl';
import { RainClientWrapper } from './RainClientWrapper';
import ProgressMoon from './components/ProgressMoon/ProgressMoon';
import { Action, Player, Toggle } from './proto/models';
import { createTheme, ThemeProvider } from '@mui/material';

// Ensure MUI items are set up for a dark theme
const theme = createTheme({
    palette: {
        text: {
            primary: '#fff',
            secondary: '#fff',
        },
    },
});

const Main = () => {
    const [rainClient, setRainClient] = useState<RainClientWrapper | undefined>();
    const [connectedToGame, setConnectedToGame] = useState(false);
    const [ipaKnownInstalled, setIpaKnownInstalled] = useState(true); // Assume the game is installed/modded until told otherwise, so we don't prematurely show the install button

    // Toggles
    const [connectedPlayers, setConnectedPlayers] = useState<Player[]>([]);
    const [toggles, setToggles] = useState<Toggle[]>([]);
    const [actions, setActions] = useState<Action[]>([]);
    const [artifacts, setArtifacts] = useState<Toggle[]>([]);

    useEffect(() => {
        const rainClient = new RainClientWrapper();

        rainClient.on('setActions', setActions);
        rainClient.on('setArtifacts', setArtifacts);
        rainClient.on('setConnectedPlayers', setConnectedPlayers);
        rainClient.on('setConnectedToGame', setConnectedToGame);
        rainClient.on('setToggles', setToggles);

        rainClient.connect();

        setRainClient(rainClient);

        (window as any).api.receive('ipa-installed-status', (data: any) => {
            setIpaKnownInstalled(data);
        });
    }, []);

    return (
        <ThemeProvider theme={theme}>
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
            {connectedToGame && <TabControl rainClient={rainClient} toggles={toggles} actions={actions} artifacts={artifacts} />}
            <ProgressMoon />
        </ThemeProvider>
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
