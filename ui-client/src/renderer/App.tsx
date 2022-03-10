import { MemoryRouter as Router, Routes, Route } from 'react-router-dom';
import './App.css';
import Button from '@mui/material/Button';

const Hello = () => {
    return (
        <div>
            <div className='Hello'>
                <button
                    type='button'
                    onClick={() =>
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        (window as any).electron.ipcRenderer.myPing()
                    }
                >
                    A
                </button>
                <Button variant='contained'>Hello World</Button>
            </div>
        </div>
    );
};

export default function App() {
    return (
        <Router>
            <Routes>
                <Route path='/' element={<Hello />} />
            </Routes>
        </Router>
    );
}
