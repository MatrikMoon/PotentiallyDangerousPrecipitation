import React, { Component, CSSProperties } from 'react';
import './FButton.scss';

type ButtonState = {
    hovered: boolean;
};

type ButtonProps = {
    //Mechanics
    text?: string;
    onClick?: () => void;
    selected?: boolean;

    //Colors
    backgroundColor?: string;
    textColor?: string;
    selectedColor?: string;
    selectedSecondaryColor?: string;
    selectedTextColor?: string;
};

export class FButton extends Component<ButtonProps, ButtonState> {
    constructor(props: ButtonProps) {
        super(props);

        this.state = {
            hovered: false,
        };

        this.onMouseEnter = this.onMouseEnter.bind(this);
        this.onMouseLeave = this.onMouseLeave.bind(this);
    }

    private onMouseEnter(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ hovered: true });
    }

    private onMouseLeave(event: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ hovered: false });
    }

    render() {
        let style = {
            '--backgroundColor': this.props.backgroundColor ?? '#eee',
            '--text-color': this.props.textColor ?? '#969696',
            '--selected-text-color': this.props.selectedTextColor ?? '#eee',
            '--selected-color': this.props.selectedColor ?? '#ff6114',
            '--selected-secondary-color': this.props.selectedSecondaryColor ?? '#fc0254',
        } as CSSProperties;

        return (
            <button
                className={`btn${this.state.hovered || this.props.selected ? ' selected' : ''}`}
                onClick={this.props.onClick}
                onMouseEnter={this.onMouseEnter}
                onMouseLeave={this.onMouseLeave}
                style={style}
            >
                {this.props.text && (
                    //Two spans. One has a shadow, and the shadowspan has .7 opacity to make the shadow mesh with the background better
                    //One has only text, and full opacity, so that the text doesn't also fade into the background with the shadow's opacity
                    <>
                        <span>{this.props.text}</span>
                        <span className='shadowSpan'>{this.props.text}</span>
                    </>
                )}
                {this.props.children}
            </button>
        );
    }
}
