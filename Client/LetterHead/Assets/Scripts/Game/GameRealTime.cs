using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WebSocketSharp;

public class GameRealTime : Singleton<GameRealTime>
{
    private WebSocket socket;

    void Start()
    {
    }

    public void Connect()
    {
        System.Uri uri = new Uri(Srv.Instance.Url());
        string uriWithoutScheme = uri.Host + ":" + uri.Port + uri.PathAndQuery;

        socket = WebSocketManager.Instance.MakeNew("ws://" + uriWithoutScheme + "api/RealTime?sessionId=" 
            + ClientManager.Instance.sessionId + "&matchId=" + GameManager.Instance.MatchId);

        socket.OnMessage += (sender, args) =>
        {
            Debug.Log(args.Data);
        };

        socket.OnError += (sender, args) =>
        {
            Debug.LogError(args.Message);
        };

        socket.OnOpen += (sender, args) =>
        {
            Debug.Log("Connected to real time socket");
            SendMessage("Test", "Yup this is a test!!!");
        };

        socket.OnClose += (sender, args) =>
        {
            Debug.Log("Disconnected fromn real time socket: " + args.Code + " " + args.Reason);
        };

        socket.Connect();
    }

    public void SendMessage(string command, string data)
    {
        SendMessage(command, Encoding.UTF8.GetBytes(data));
    }

    public void SendMessage(string command, byte[] data)
    {
        var steam = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(steam);
        bw.Write(command);
        bw.Write(data.Length);
        bw.Write(data);

        socket.SendAsync(steam.ToByteArray(), OnSendComplete);
        bw.Close();
        steam.Close();
    }

    private void OnSendComplete(bool status)
    {
        
    }
}