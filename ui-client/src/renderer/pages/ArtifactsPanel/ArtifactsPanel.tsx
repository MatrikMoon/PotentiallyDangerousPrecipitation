import './ArtifactsPanel.scss';
import { Toggle } from '../../proto/models';
import { FormControlLabel, FormGroup, Switch } from '@mui/material';
import { Event, Packet } from 'renderer/proto/packets';
import { RainClientWrapper } from 'renderer/RainClientWrapper';

type Props = {
    artifacts: Toggle[];
    rainClient: RainClientWrapper | undefined;
};

const ArtifactsPanel = (props: Props) => {
    return (
        <div className='artifactsPanel'>
            <FormGroup>
                {props.artifacts.map((x) => (
                    <FormControlLabel
                        key={x.id}
                        control={
                            <Switch
                                checked={x.value || false}
                                onChange={() => {
                                    if (props.rainClient) {
                                        const packet = new Packet();
                                        packet.event = new Event();
                                        packet.event.artifact_updated_event = new Event.ArtifactUpdatedEvent();
                                        packet.event.artifact_updated_event.artifact = x;
                                        packet.event.artifact_updated_event.artifact.value =
                                            !packet.event.artifact_updated_event.artifact.value;
                                        props.rainClient.send(packet.serializeBinary());
                                    }
                                }}
                            />
                        }
                        label={x.name}
                    />
                ))}
            </FormGroup>
        </div>
    );
};

export default ArtifactsPanel;
