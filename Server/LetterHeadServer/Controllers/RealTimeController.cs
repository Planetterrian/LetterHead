using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Hangfire;
using LetterHeadServer.Classes;
using LetterHeadServer.Models;
using Microsoft.Ajax.Utilities;

namespace LetterHeadServer.Controllers
{
    public class RealTimeController : ApiController
    {
        private User currentUser;
        private Match match;
        private ApplicationDbContext db;
        private WebSocket socket;

        public HttpResponseMessage Get(string sessionId, int matchId)
        {
            db = new ApplicationDbContext();
            currentUser = UserManager.GetUserBySession(db, sessionId);

            if (currentUser == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid session");
            }

            match = Match.GetById(db, matchId);
            if (match == null)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid Match");
            }

            if (!match.UserAuthorized(currentUser))
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Can't access that match");
            }
            
            HttpContext.Current.AcceptWebSocketRequest(context => ProcessSocket(context, db, currentUser, match));
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessSocket(AspNetWebSocketContext context, ApplicationDbContext db, User user, Match match)
        {
            socket = context.WebSocket;
            while (true)
            {
                ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(
                    buffer, CancellationToken.None);
                if (socket.State == WebSocketState.Open)
                {
                    await ProcessMessage(buffer.Array);
                }
                else
                {
                    break;
                }
            }
        }

        private async Task SendMessage(string command, string json)
        {
            var buffer = new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(json));

            await socket.SendAsync(
                buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task ProcessMessage(byte[] array)
        {
            var stream = new MemoryStream(array);
            BinaryReader read = new BinaryReader(stream);

            var command = read.ReadString();

            MethodInfo theMethod = GetType().GetMethod("_" + command);
            await (Task)theMethod.Invoke(this, new object[] { stream });
            read.Close();
            stream.Close();

            //Console.WriteLine(command + " - " + data);
        }

        private async Task _RequestStart(BinaryReader reader)
        {
            if (match.CurrentUserTurn != currentUser)
            {
                Err("It's your opponents turn");
                return;
            }

            if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                Err("That game has already ended");
                return;
            }

            var round = match.CurrentRoundForUser(currentUser);
            if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
            {
                round.CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Active;
                round.StartedOn = DateTime.Now;

                BackgroundJob.Schedule(() => new MatchController().ManuallyEndRound(match.Id, round.Id), round.EndTime());
            }

            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Running;
            db.SaveChanges();

            var json =
                System.Web.Helpers.Json.Encode(new {TimeRemaining = (round.EndTime() - DateTime.Now).TotalSeconds});

            await SendMessage("StartRound", json);
        }


        private void Err(string errMSg)
        {
            
        }
    }

}
