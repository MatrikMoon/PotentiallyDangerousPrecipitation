import './LegacyPanel.scss';
import { Toggle, Action } from '../../proto/models';
import { Checkbox, FormControlLabel, FormGroup, Switch, TextField } from '@mui/material';
import { Command, Event, Packet } from 'renderer/proto/packets';
import { FButton } from '../../components/FButton/FButton';
import { RainClientWrapper } from 'renderer/RainClientWrapper';

type Props = {
    toggles: Toggle[];
    actions: Action[];
    rainClient: RainClientWrapper | undefined;
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
                                    if (props.rainClient) {
                                        const packet = new Packet();
                                        packet.event = new Event();
                                        packet.event.toggle_updated_event = new Event.ToggleUpdatedEvent();
                                        packet.event.toggle_updated_event.toggle = x;
                                        packet.event.toggle_updated_event.toggle.value =
                                            !packet.event.toggle_updated_event.toggle.value;
                                        props.rainClient.send(packet.serializeBinary());
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
                    {props.actions.map((x) => {
                        return (
                            <div className='buttonFieldPair' key={x.id}>
                                <FButton
                                    text={x.name}
                                    textColor='#606060'
                                    onClick={() => {
                                        const packet = new Packet();
                                        packet.command = new Command();
                                        packet.command.do_action_command = new Command.DoActionCommand();
                                        packet.command.do_action_command.action = x;
                                        props.rainClient!.send(packet.serializeBinary());
                                    }}
                                />
                                {x.check_box && (
                                    <FormControlLabel
                                        control={
                                            <Checkbox
                                                onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                                                    x.boolean = event?.target.checked;
                                                }}
                                            />
                                        }
                                        label={x.check_box_label}
                                    />
                                )}
                                {x.text_box && (
                                    <TextField
                                        id={x.id}
                                        label={x.text_box_label}
                                        onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
                                            x.string = event.target.value;
                                            x.integer = Number(event.target.value);
                                        }}
                                    />
                                )}
                            </div>
                        );
                    })}
                </div>
            </FormGroup>
        </div>
    );
};

export default LegacyPanel;
