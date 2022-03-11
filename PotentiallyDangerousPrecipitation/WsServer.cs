using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;

namespace PotentiallyDangerousPrecipitation
{
    internal class WsServer
    {
        public async void Start()
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("https://127.0.0.1:10167/");
            httpListener.Start();
            while (true)
            {
                var httpListenerContext = await httpListener.GetContextAsync();
                if (httpListenerContext.Request.IsWebSocketRequest)
                {
                    ProcessRequest(httpListenerContext);
                }
                else
                {
                    httpListenerContext.Response.StatusCode = 400;
                    httpListenerContext.Response.Close();
                }
            }
        }

        private async void ProcessRequest(HttpListenerContext httpListenerContext)
        {
            var webSocketContext = await httpListenerContext.AcceptWebSocketAsync(subProtocol: null);
            var webSocket = webSocketContext.WebSocket;
            byte[] receiveBuffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                else
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(receiveBuffer, 0, receiveResult.Count), WebSocketMessageType.Text, receiveResult.EndOfMessage, CancellationToken.None);
                }
            }
        }
    }
}
