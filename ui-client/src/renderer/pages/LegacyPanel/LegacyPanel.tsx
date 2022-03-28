import './LegacyPanel.scss';
import { proto } from '../../proto/models';
import { Checkbox, FormControlLabel, FormGroup, Switch } from '@mui/material';
import { Event, Packet } from 'renderer/proto/packets';
import { FButton } from '../../components/FButton/FButton';

type Props = {
    toggles: proto.models.Toggle[];
    socket: WebSocket | null;
};

const LegacyPanel = (props: Props) => {
    return (
        <div className='legacyPanel'>
            <FormGroup>
                {props.toggles.map((x) => (
                    <FormControlLabel
                        key={x.id}
                        control={
                            <Switch
                                checked={x.value || false}
                                onChange={() => {
                                    if (props.socket) {
                                        const packet = new Packet();
                                        packet.event = new Event();
                                        packet.event.toggle_updated_event = new Event.ToggleUpdatedEvent();
                                        packet.event.toggle_updated_event.toggle = x;
                                        packet.event.toggle_updated_event.toggle.value =
                                            !packet.event.toggle_updated_event.toggle.value;
                                        props.socket.send(packet.serializeBinary());
                                    }
                                }}
                            />
                        }
                        label={x.name}
                    />
                ))}
            </FormGroup>
            <FormGroup>
                <div className='buttonColumn'>
                    <FButton text='Give 20m Lunar Coins (Self)' textColor='#606060' />
                    <FButton text='Give 20m Lunar Coins (Everyone)' textColor='#606060' />
                    <FButton text='Spawn Shrine of the Mountain' textColor='#606060' />
                    <FButton text='Spawn Shrine of the Mountain (50x)' textColor='#606060' />
                    <FButton text='Spawn Teleporter' textColor='#606060' />
                    <FButton text='Spawn Teleporter (50x)' textColor='#606060' />
                    <FButton text='Start Teleporter Event' textColor='#606060' />
                    <FButton text='Unlock All Achievements (Permanent)' textColor='#606060' />
                    <FButton text='Skip Active Vote' textColor='#606060' />
                    <FButton text='Reset Active Vote' textColor='#606060' />
                </div>
            </FormGroup>
        </div>
    );
};

export default LegacyPanel;
