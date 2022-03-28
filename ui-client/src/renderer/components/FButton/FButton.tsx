import React, { Component, CSSProperties } from 'react';
import './FButton.scss';

type ButtonState = {
    hovered: boolean;
    mouseDown: boolean;
};

type ButtonProps = {
    //Mechanics
    text?: string;
    onClick?: () => void;
    hovered?: boolean;
    selected?: boolean;

    //Colors
    backgroundColor?: string;
    textColor?: string;
    hoveredColor?: string;
    hoveredSecondaryColor?: string;
    hoveredTextColor?: string;
    selectedColor?: string;
    selectedSecondaryColor?: string;
    selectedTextColor?: string;
};

export class FButton extends Component<ButtonProps, ButtonState> {
    constructor(props: ButtonProps) {
        super(props);

        this.state = {
            hovered: false,
            mouseDown: false,
        };

        this.onMouseEnter = this.onMouseEnter.bind(this);
        this.onMouseLeave = this.onMouseLeave.bind(this);
        this.onMouseUp = this.onMouseUp.bind(this);
        this.onMouseDown = this.onMouseDown.bind(this);
    }

    private onMouseEnter(_: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ hovered: true });
    }

    private onMouseLeave(_: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ hovered: false });
    }

    private onMouseUp(_: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ mouseDown: false });
    }

    private onMouseDown(_: React.MouseEvent<HTMLButtonElement, MouseEvent>) {
        this.setState({ mouseDown: true });
    }

    render() {
        let style = {
            '--backgroundColor': this.props.backgroundColor ?? '#eee',
            '--text-color': this.props.textColor ?? '#969696',
            '--hovered-text-color': this.props.hoveredTextColor ?? '#eee',
            '--hovered-color': this.props.hoveredColor ?? '#ff6114',
            '--hovered-secondary-color': this.props.hoveredSecondaryColor ?? '#fc0254',
            '--selected-text-color': this.props.selectedTextColor ?? '#eee',
            '--selected-color': this.props.selectedColor ?? '#2ecc71',
            '--selected-secondary-color': this.props.selectedSecondaryColor ?? '#06863c',
        } as CSSProperties;

        let buttonState = '';
        if (this.state.hovered || this.props.hovered) {
            buttonState = ' hovered';
        }
        if (this.state.mouseDown || this.props.selected) {
            buttonState = ' selected';
        }

        return (
            <button
                className={`btn${buttonState}`}
                onClick={this.props.onClick}
                onMouseEnter={this.onMouseEnter}
                onMouseLeave={this.onMouseLeave}
                onMouseUp={this.onMouseUp}
                onMouseDown={this.onMouseDown}
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
