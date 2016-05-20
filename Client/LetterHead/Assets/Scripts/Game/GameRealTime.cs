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

    void Start()
    {
    }

    void OnDisable()
    {
        if(socket != null && socket.IsConnected)
            socket.Close(CloseStatusCode.Normal);
    }

    public void Connect()
    {
        System.Uri uri = new Uri(Srv.Instance.Url());
        string uriWithoutScheme = uri.Host + ":" + uri.Port + uri.PathAndQuery;

        socket = WebSocketManager.Instance.MakeNew("ws://" + uriWithoutScheme + "api/RealTime?sessionId=" 
            + ClientManager.Instance.sessionId + "&matchId=" + GameManager.Instance.MatchId);

        socket.OnMessage += (sender, args) =>
        {
            ProcessMessage(args.RawData);
            //Debug.Log(args.Data);
        };

        socket.OnError += (sender, args) =>
        {
            Debug.LogError(args.Message);
        };

        socket.OnOpen += (sender, args) =>
        {
            Debug.Log("Connected to real time socket");
            GameGui.Instance.OnRealTimeConnected();
        };

        socket.OnClose += (sender, args) =>
        {
            Debug.Log("Disconnected fromn real time socket: " + args.Code + " " + args.Reason);
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

        theMethod.Invoke(this, new object[] { read });

        read.Close();
        stream.Close();

    }


    public void SendMsg(string command, string data)
    {
        var steam = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(steam);
        bw.Write(command);

        bw.Write(data);

        socket.SendAsync(steam.ToByteArray(), b => { });
        bw.Close();
        steam.Close();
    }

    public void SendMsg(string command, byte[] data = null)
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

        Debug.Log("Start time = " + time);
    }

    public bool IsConnected()
    {
        if (socket == null)
            return false;

        return socket.IsConnected;
    }
}