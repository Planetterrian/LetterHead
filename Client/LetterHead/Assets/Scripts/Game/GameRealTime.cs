using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WebSocketSharp;

public class GameRealTime : Singleton<GameRealTime>
{
    private WebSocket socket;
    private bool isShuttingDown;

    private int currentMatchId;

    void Start()
    {
        var x = Loom.Current;
    }

    void OnDisable()
    {
        isShuttingDown = true;

        if(socket != null && socket.IsConnected)
            socket.CloseAsync(CloseStatusCode.Normal);
    }

    public void Connect()
    {
        // We're still connected
        if (currentMatchId == GameManager.Instance.MatchId && socket.IsConnected)
        {
            SendMsg("RefreshRound");
            return;
        }

        currentMatchId = GameManager.Instance.MatchId;
        System.Uri uri = new Uri(Srv.Instance.Url());
        string uriWithoutScheme = uri.Host + ":" + uri.Port + uri.PathAndQuery;

        socket = WebSocketManager.Instance.MakeNew("ws://" + uriWithoutScheme + "api/RealTime?sessionId=" 
            + ClientManager.Instance.SessionId + "&matchId=" + GameManager.Instance.MatchId);

        socket.OnMessage += (sender, args) =>
        {
            ProcessMessage(args.RawData);
            //Debug.Log(args.Data);
        };

        socket.OnError += (sender, args) =>
        {
            Debug.LogError(args.Message);

            if (!isShuttingDown)
            {
                DialogWindowTM.Instance.Show("Disconnected", "You have been disconnected from the server", () =>
                {
                    PersistManager.Instance.LoadMenu();
                });
            }
        };

        socket.OnOpen += (sender, args) =>
        {
            Debug.Log("Connected to real time socket");
            GameGui.Instance.OnRealTimeConnected();
            GameManager.Instance.OnRealTimeConnected();
        };

        socket.OnClose += (sender, args) =>
        {
            Debug.Log("Disconnected fromn real time socket: " + args.Code + " " + args.Reason);

            if (!isShuttingDown)
            {
                DialogWindowTM.Instance.Show("Disconnected", "You have been disconnected from the server", () =>
                {
                    PersistManager.Instance.LoadMenu();
                });
            }
        };

        socket.Connect();
    }

    private void ProcessMessage(byte[] rawData)
    {
        var stream = new MemoryStream(rawData);
        BinaryReader read = new BinaryReader(stream);

        var command = read.ReadString();

        MethodInfo theMethod = GetType().GetMethod("_" + command, BindingFlags.NonPublic | BindingFlags.Instance);

        Debug.Log(command);

        Loom.QueueOnMainThread(() =>
        {
            theMethod.Invoke(this, new object[] {read});

            read.Close();
            stream.Close();
        });
    }


    private void SendMsg(string command, string data)
    {
        var steam = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(steam);
        bw.Write(command);

        bw.Write(data);

        socket.SendAsync(steam.ToByteArray(), b => { });
        bw.Close();
        steam.Close();
    }

    private void SendMsg(string command, byte[] data = null)
    {
        var steam = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(steam);
        bw.Write(command);

        if (data != null)
            bw.Write(data);

        socket.SendAsync(steam.ToByteArray(), b => { });
        bw.Close();
        steam.Close();
    }

    void _Err(BinaryReader message)
    {
        var msg = message.ReadString();
        Debug.LogError(msg);
    }


    void _StartRound(BinaryReader message)
    {
        var time = message.ReadSingle();
        
        GameManager.Instance.StartGame(time);

        Debug.Log("Start time = " + time);
    }

    public bool IsConnected()
    {
        if (socket == null)
            return false;

        return socket.IsConnected;
    }

    public void RequestStart()
    {
        SendMsg("RequestStart");
    }

    public void AddWord(string word, int usedLetterIds)
    {
        using (var steam = new MemoryStream())
        {
            using (BinaryWriter bw = new BinaryWriter(steam))
            {
                bw.Write(word);
                bw.Write(usedLetterIds);
                SendMsg("AddWord", steam.ToByteArray());
            }
        }

    }
}