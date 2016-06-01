using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
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
using LetterHeadShared;
using Microsoft.Ajax.Utilities;
using MatchRound = LetterHeadShared.DTO.MatchRound;

namespace LetterHeadServer.Controllers
{
    public class RealTimeController : ApiController
    {
        private User currentUser;
        private Match match;
        private RealTimeMatch rtm;
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

            //rtm = RealTimeMatchManager.GetMatch(matchId);
            //rtm.OnNewMessage += OnMatchMessage;

            HttpContext.Current.AcceptWebSocketRequest(ProcessSocket);
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
        }

        private void OnMatchMessage(RealTimeMatch.Message message)
        {
            
        }

        private async Task ProcessSocket(AspNetWebSocketContext context)
        {
            socket = context.WebSocket;
            while (true)
            {
                byte[] receiveBuffer = new byte[1024];

                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);

                int count = result.Count;

                while (result.EndOfMessage == false)
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, count, 1024 - count), CancellationToken.None);
                    count += result.Count;
                }

                if (socket.State == WebSocketState.Open)
                {
                    await ProcessMessage(receiveBuffer);
                }
                else
                {
                    break;
                }
            }
        }



        private async Task SendMessage(string command, int data)
        {
            await SendMessage(command, BitConverter.GetBytes(data));
        }

        private async Task SendMessage(string command, float data)
        {
            await SendMessage(command, BitConverter.GetBytes(data));
        }

        private async Task SendMessage(string command, string data)
        {
            var steam = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(steam);
            bw.Write(command);
            bw.Write(data);

            var buffer = new ArraySegment<byte>(steam.ToArray());

            await socket.SendAsync(
                buffer, WebSocketMessageType.Text, true, CancellationToken.None);

            bw.Close();
            steam.Close();
        }

        private async Task SendMessage(string command, byte[] data = null)
        {
            var steam = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(steam);
            bw.Write(command);

            if(data != null)
                bw.Write(data);

            var buffer = new ArraySegment<byte>(steam.ToArray());

            try
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);

            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                throw;
            }

            bw.Close();
            steam.Close();
        }

        
        private async Task ProcessMessage(byte[] array)
        {
            var stream = new MemoryStream(array);
            BinaryReader read = new BinaryReader(stream);

            var command = read.ReadString();

            MethodInfo theMethod = GetType().GetMethod("_" + command, BindingFlags.NonPublic | BindingFlags.Instance);
            if(theMethod == null)
                throw new ArgumentException("Invalid command: " + command);

            await (Task)theMethod.Invoke(this, new object[] { read });
            read.Close();
            stream.Close();

            //Console.WriteLine(command + " - " + data);
        }

        private async Task _AddWord(BinaryReader reader)
        {
            if (match.CurrentUserTurn != currentUser)
            {
                await Err("It's your opponents turn");
                return;
            }

            if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                await Err("That game has already ended");
                return;
            }

            var round = match.CurrentRoundForUser(currentUser);
            if (round.CurrentState != LetterHeadShared.DTO.MatchRound.RoundState.Active)
            {
                await Err("Round not active");
                return;
            }

            var word = reader.ReadString();
            var uniqueLetterCount = reader.ReadInt32();

            round.Words.Add(word);
            round.UsedLetterIds = uniqueLetterCount;
            db.SaveChanges();
        }

        private async Task _RefreshRound(BinaryReader reader)
        {
            ((IObjectContextAdapter)db).ObjectContext.Refresh(RefreshMode.StoreWins, match);
        }

        private async Task _RequestStart(BinaryReader reader)
        {
            if (match.CurrentUserTurn != currentUser)
            {
                await Err("It's your opponents turn");
                return;
            }

            if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                await Err("That game has already ended");
                return;
            }

            var round = match.CurrentRoundForUser(currentUser);

            if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.WaitingForCategory)
            {
                await Err("Waiting for categories");
                return;
            }

            if (round.CurrentState == LetterHeadShared.DTO.MatchRound.RoundState.NotStarted)
            {
                round.CurrentState = LetterHeadShared.DTO.MatchRound.RoundState.Active;
                round.StartedOn = DateTime.Now;

                round.EndRoundJobId = BackgroundJob.Schedule(() => new MatchController().ManuallyEndRound(match.Id, round.Id), round.EndTime());
            }

            match.CurrentState = LetterHeadShared.DTO.Match.MatchState.Running;
            db.SaveChanges();


            await SendMessage("StartRound", round.TimeRemaining());
        }


        private async Task _UseDoOver(BinaryReader reader)
        {
            if (match.CurrentUserTurn != currentUser)
            {
                await Err("It's your opponents turn");
                return;
            }

            if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                await Err("That game has already ended");
                return;
            }

            var round = match.CurrentRoundForUser(currentUser);
            if (round.CurrentState != MatchRound.RoundState.Active)
            {
                await Err("Round not active");
                return;
            }

            if (round.DoOverUsed)
            {
                await Err("Do over already used");
                return;
            }

            if(currentUser.PowerupCount(Powerup.Type.DoOver) < 1)
            {
                await Err("No boost available");
                return;
            }

            round.Letters = BoardHelper.GenerateBoard();
            round.StartedOn = DateTime.Now;
            round.Words = new List<string>();
            round.UsedLetterIds = 0;
            round.Score = 0;
            round.CategoryName = "";
            round.DoOverUsed = true;

            BackgroundJob.Delete(round.EndRoundJobId);
            round.EndRoundJobId = BackgroundJob.Schedule(() => new MatchController().ManuallyEndRound(match.Id, round.Id), round.EndTime());

            currentUser.ConsumePowerup(Powerup.Type.DoOver);

            db.SaveChanges();

            var steam = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(steam);
            bw.Write(round.Letters);
            bw.Write(round.TimeRemaining());


            await SendMessage("DoOver", steam.ToArray());
            bw.Close();
            steam.Close();
        }

        private async Task _UseShield(BinaryReader reader)
        {
            if (match.CurrentUserTurn != currentUser)
            {
                await Err("It's your opponents turn");
                return;
            }

            if (match.CurrentState == LetterHeadShared.DTO.Match.MatchState.Ended)
            {
                await Err("That game has already ended");
                return;
            }

            var round = match.CurrentRoundForUser(currentUser);
            if (round.CurrentState != MatchRound.RoundState.Active)
            {
                await Err("Round not active");
                return;
            }

            if (round.ShieldUsed)
            {
                await Err("Shield already used");
                return;
            }


            if (currentUser.PowerupCount(Powerup.Type.Shield) < 1)
            {
                await Err("No boost available");
                return;
            }

            round.ShieldUsed = true;
            currentUser.ConsumePowerup(Powerup.Type.Shield);
            db.SaveChanges();

            await SendMessage("ShieldUsed");
        }


        private async Task Err(string errMSg)
        {
            await SendMessage("Err", errMSg);
        }
    }

}
