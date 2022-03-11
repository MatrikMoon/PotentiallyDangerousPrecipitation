import React, { Component, CSSProperties } from 'react';
import './FSearchBar.scss';

type SearchBarProps = {
    //Mechanics
    text: string;
    onComplete?: (text: string) => void;
    onTextChanged?: (text: string) => void;

    //Options and parameters
    autoCompletable?: boolean;
    autoCompleteSet?: Array<string>;

    //Colors
    backgroundColor?: string;
    shadowColor?: string;
};

export class FSearchBar extends Component<SearchBarProps> {
    private listRef: React.RefObject<any>;
    private inputRef: React.RefObject<any>;

    constructor(props: SearchBarProps) {
        super(props);

        this.onTextChanged = this.onTextChanged.bind(this);
        this.onKeyDown = this.onKeyDown.bind(this);
        this.onAutoCompleteItemClick = this.onAutoCompleteItemClick.bind(this);

        this.listRef = React.createRef();
        this.inputRef = React.createRef();

        //Gets document onkeydown so we can control the autocomplete
        document.onkeydown = this.onKeyDown;
    }

    componentWillUnmount() {
        document.onkeydown = null;
    }

    //Crazy, black-box keydown logic
    //Traits:
    //	- Enter Press when scrolling through autocomplete list
    //		places the text in, and focuses, the input
    //	- Up/Down arrows will scroll through the list, or refocus
    //		the input when appropriate
    private onKeyDown(event: KeyboardEvent) {
        if (event.key === 'Enter') {
            if (document.activeElement === this.inputRef.current) {
                this.props.onComplete?.call(this, this.props.text);
            } else {
                this.autocompleteItemSelected(document.activeElement);
            }
        } else if (
            this.props.autoCompletable &&
            this.props.autoCompleteSet?.length &&
            this.props.autoCompleteSet?.length > 0
        ) {
            if (event.key === 'ArrowUp' && document.activeElement !== this.inputRef.current) {
                if (document.activeElement === this.listRef.current.firstChild) this.inputRef.current.focus();
                else {
                    (document.activeElement?.previousSibling as any).focus();
                }
            } else if (event.key === 'ArrowDown' && document.activeElement !== this.listRef.current.lastChild) {
                if (document.activeElement === this.inputRef.current) {
                    this.listRef.current.firstElementChild.focus();
                } else {
                    (document.activeElement?.nextSibling as any).focus();
                }
            }
        }
    }

    private onTextChanged(event: React.ChangeEvent<HTMLInputElement>) {
        this.props.onTextChanged?.call(this, event.target.value);
    }

    private onAutoCompleteItemClick(event: React.MouseEvent<HTMLLIElement, MouseEvent>) {
        this.autocompleteItemSelected(event.target);
    }

    private autocompleteItemSelected(target: any) {
        this.props.onTextChanged?.call(this, target.innerText);
        this.inputRef.current.focus();
    }

    render() {
        let style = {
            '--background-color': this.props.backgroundColor ?? '#444b58',
            '--shadow-color': this.props.shadowColor ?? '#00000099',
        } as CSSProperties;

        const listItems = this.props.autoCompleteSet?.filter((x) =>
            x.toLowerCase().includes(this.props.text.toLowerCase() as string)
        );

        return (
            <div className='bar'>
                <input
                    ref={this.inputRef}
                    className={
                        this.props.text && this.props.autoCompletable && listItems && listItems.length > 0
                            ? 'list-open'
                            : ''
                    }
                    value={this.props.text}
                    onChange={this.onTextChanged}
                    spellCheck={false}
                    style={style}
                />
                {this.props.text && this.props.autoCompletable && this.props.autoCompleteSet && (
                    <ul ref={this.listRef}>
                        {listItems?.map((x) => {
                            return (
                                <li key={x} onClick={this.onAutoCompleteItemClick} data-id={x} tabIndex={1}>
                                    {x}
                                </li>
                            );
                        })}
                    </ul>
                )}
            </div>
        );
    }
}
