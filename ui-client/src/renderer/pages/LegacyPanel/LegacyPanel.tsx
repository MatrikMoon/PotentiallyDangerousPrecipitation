import './LegacyPanel.scss';
import { Toggle, Action } from '../../proto/models';
import { Checkbox, createTheme, FormControlLabel, FormGroup, Switch, TextField, ThemeProvider } from '@mui/material';
import { Command, CommandDoActionCommand, Event, EventToggleUpdatedEvent, Packet } from 'renderer/proto/packets';
import { FButton } from '../../components/FButton/FButton';
import { useState } from 'react';

type Props = {
    toggles: Toggle[];
    actions: Action[];
    socket: WebSocket | null;
};

const theme = createTheme({
    palette: {
        background: {
            paper: '#fff',
        },
        text: {
            primary: '#173A5E',
            secondary: '#46505A',
        },
        action: {
            active: '#001E3C',
        },
    },
});

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
                                        packet.event.toggle_updated_event = new EventToggleUpdatedEvent();
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
                    {props.actions.map((x) => {
                        return (
                            <div className='buttonFieldPair'>
                                <FButton
                                    text={x.name}
                                    textColor='#606060'
                                    onClick={() => {
                                        const packet = new Packet();
                                        packet.command = new Command();
                                        packet.command.do_action_command = new CommandDoActionCommand();
                                        packet.command.do_action_command.action = x;
                                        props.socket!.send(packet.serializeBinary());
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
