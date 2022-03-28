import './TabControl.scss';
import { proto } from '../../proto/models';
import { Box, Tab, Tabs } from '@mui/material';
import { useState } from 'react';
import LegacyPanel from '../LegacyPanel/LegacyPanel';

interface TabPanelProps {
    children?: React.ReactNode;
    index: number;
    value: number;
}

function TabPanel(props: TabPanelProps) {
    const { children, value, index, ...other } = props;

    return (
        <div role='tabpanel' hidden={value !== index} id={`vertical-tabpanel-${index}`} {...other}>
            {value === index && <Box sx={{ p: 3 }}>{children}</Box>}
        </div>
    );
}

type Props = {
    toggles: proto.models.Toggle[];
    socket: WebSocket | null;
};

const TabControl = (props: Props) => {
    const [value, setValue] = useState(0);

    const handleChange = (_: React.SyntheticEvent, newValue: number) => {
        setValue(newValue);
    };

    return (
        <div className='tabWrapper'>
            <Box sx={{ flexGrow: 1, display: 'flex' }} height='100vh' width='100vw'>
                <Tabs
                    textColor='inherit'
                    orientation='vertical'
                    variant='scrollable'
                    value={value}
                    onChange={handleChange}
                    sx={{ borderRight: 1, borderColor: 'divider' }}
                >
                    <Tab label='Legacy Panel' />
                    <Tab label='Experimental' disabled />
                </Tabs>
                <TabPanel value={value} index={0}>
                    <LegacyPanel toggles={props.toggles} socket={props.socket} />
                </TabPanel>
            </Box>
        </div>
    );
};

export default TabControl;
