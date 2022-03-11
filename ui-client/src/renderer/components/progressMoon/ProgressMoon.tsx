import { Component } from 'react';
import './ProgressMoon.scss';

class ProgressMoon extends Component {
    render() {
        return (
            <div className='logo'>
                <img src={require('./moonmoonlayer.png')} className='moonlogo' alt='logo' />
                <img src={require('./moonstarlayer.png')} className='starslogo' alt='logo' />
            </div>
        );
    }
}

export default ProgressMoon;
