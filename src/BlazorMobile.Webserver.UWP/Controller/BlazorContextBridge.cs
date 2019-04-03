using BlazorMobile.Common.Interop;
using BlazorMobile.Webserver.Common.Interop;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlazorMobile.Webserver.UWP.Controller
{
    public static class BlazorContextBridge
    {
        private static List<WebSocket> WebSockets = new List<WebSocket>();

        public static async Task OnMessageReceived(HttpContext context, WebSocket webSocket)
        {
            WebSockets.Add(webSocket);

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!received.CloseStatus.HasValue)
            {
                //TODO: Considering to send data from client side as binary Streamed JSON for performance in the future !
                //Value type reference as byte[] and/or string are not good for performance
                string methodProxyJson = System.Text.Encoding.UTF8.GetString(buffer, 0, received.Count);

                Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                {
                    MethodProxy taksInput = null;
                    MethodProxy taksOutput = null;

                    try
                    {
                        taksInput = ContextBridge.GetMethodProxyFromJSON(methodProxyJson);
                        taksOutput = ContextBridge.Receive(taksInput);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: [Native] - BlazorContextBridge.Receive: " + ex.Message);
                    }

                    try
                    {
                        string jsonReturnValue = ContextBridge.GetJSONReturnValue(taksOutput);
                        SendMessageToClient(jsonReturnValue);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: [Native] - BlazorContextBridge.Send: " + ex.Message);
                    }
                });
            }
            await webSocket.CloseAsync(received.CloseStatus.Value, received.CloseStatusDescription, CancellationToken.None);
            WebSockets.Remove(webSocket);
        }

        public static void SendMessageToClient(string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            var arraySegment = new ArraySegment<byte>(bytes);

            foreach (var ws in WebSockets)
            {
                ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
