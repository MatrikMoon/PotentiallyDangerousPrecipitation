import './MainPage.scss';
import { proto } from '../../proto/models';
import { FormControlLabel, FormGroup, Switch } from '@mui/material';
import { Event, Packet } from 'renderer/proto/packets';
import ProgressMoon from '../ProgressMoon/ProgressMoon';

type Props = {
    toggles: proto.models.Toggle[];
    socket: WebSocket | null;
};

const MainPage = (props: Props) => {
    return (
        <div className='mainPage'>
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
            <ProgressMoon />
        </div>
    );
};

export default MainPage;
