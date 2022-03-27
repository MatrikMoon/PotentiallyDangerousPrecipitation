type wsMessage = (this: WebSocket, ev: MessageEvent<any>) => any;
type wsOpen = (this: WebSocket, ev: Event) => any;
type wsClose = (this: WebSocket, ev: CloseEvent) => any;
type wsError = (this: WebSocket, ev: Event) => any;

export const connectWs = (
    onWsMessage: wsMessage,
    onWsOpen: wsOpen,
    onWsClose: wsClose,
    onWsError: wsError,
    setSocket: React.Dispatch<React.SetStateAction<WebSocket | null>>
) => {
    const ws = new WebSocket('ws://127.0.0.1:10666');

    ws.onmessage = onWsMessage;
    ws.onopen = onWsOpen;
    ws.onclose = onWsClose;
    ws.onerror = onWsError;

    setSocket(ws);
};
