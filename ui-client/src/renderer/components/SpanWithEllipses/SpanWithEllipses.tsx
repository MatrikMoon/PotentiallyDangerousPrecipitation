import './SpanWithEllipses.scss';

type Props = {
    text: string;
};

const SpanWithEllipses = ({ text }: Props) => <span className='loading'>{text}</span>;

export default SpanWithEllipses;
