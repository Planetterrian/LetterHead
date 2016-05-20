using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;

namespace LetterHeadServer.Controllers
{
    public class RealTimeController : ApiController
    {
        public HttpResponseMessage Get()
        {
            HttpContext.Current.AcceptWebSocketRequest(ProcessWSChat);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessWSChat(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            while (true)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                if (socket.State == WebSocketState.Open)
                {
                    ProcessMessage(buffer.Array);
                 /*   string userMessage = Encoding.UTF8.GetString(
                        buffer.Array, 0, result.Count);
                    userMessage = "You sent: " + userMessage + " at " +
                        DateTime.Now.ToLongTimeString();
                    buffer = new ArraySegment<byte>(
                        Encoding.UTF8.GetBytes(userMessage));
                    await socket.SendAsync(
                        buffer, WebSocketMessageType.Text, true, CancellationToken.None);*/
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessMessage(byte[] array)
        {
            var stream = new MemoryStream(array);
            BinaryReader read = new BinaryReader(stream);

            var command = read.ReadString();
            var dataLen = read.ReadInt32();

            var data = read.ReadBytes(dataLen);

            Console.WriteLine(command + " - " + data);
        }
    }

}
