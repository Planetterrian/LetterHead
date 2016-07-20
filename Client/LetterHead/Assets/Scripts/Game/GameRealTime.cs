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
    private bool pingRecieved;
    private bool manualReconnect;

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
        };

        socket.OnError += (sender, args) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.LogError(args.Message);

                if (!isShuttingDown)
                {
                    PersistManager.Instance.LoadMenu();
/*
                    DialogWindowTM.Instance.Show("Disconnected", "You have been disconnected from the server.", () =>
                    {
                    });*/
                }
            });
        };

        socket.OnOpen += (sender, args) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                if (!isShuttingDown)
                {
                    Debug.Log("Connected to real time socket. Sending Ping");
                    pingRecieved = false;
                    TimerManager.AddEvent(1.3f, CheckPing);
                    SendMsg("ClientPing");
                }
            });
        };

        socket.OnClose += (sender, args) =>
        {
            Loom.QueueOnMainThread(() =>
            {
                Debug.Log("Disconnected fromn real time socket: " + args.Code + " " + args.Reason);

                if (!isShuttingDown)
                {
                    if (manualReconnect)
                    {
                        manualReconnect = false;
                        Connect();
                        return;
                    }

                    DialogWindowTM.Instance.Show("Disconnected", "You have been disconnected from the server.", () =>
                    {
                        PersistManager.Instance.LoadMenu();
                    });
                }
            });
        };

        socket.Connect();
    }

    private void CheckPing()
    {
        if (pingRecieved || isShuttingDown)
            return;

        manualReconnect = true;
        socket.Close();
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

    private void _ServerPing(BinaryReader message)
    {
        if(!GameGui.Instance)
            return;

        pingRecieved = true;

        GameGui.Instance.OnRealTimeConnected();
        GameManager.Instance.OnRealTimeConnected();
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


    void _DoOver(BinaryReader message)
    {
        var letters = message.ReadString();
        var time = message.ReadSingle();

        GameManager.Instance.CurrentRound().DoOverUsed = true;
        GameManager.Instance.CurrentRound().Words = new List<string>();
        GameScene.Instance.ResetGame();
        BoardManager.Instance.SetBoardLetters(letters, false);
        GameManager.Instance.StartGame(time);
        ClientManager.Instance.RefreshMyInfo(false, b => PowerupManager.Instance.OnRoundStateChanged());

        AchievementManager.Instance.Set("booster_doover");
    }

    void _ShieldUsed(BinaryReader message)
    {
        GameManager.Instance.CurrentRound().ShieldUsed = true;
        GameGui.Instance.OnShieldUsed();
        PowerupManager.Instance.OnRoundStateChanged();
        PowerupManager.Instance.CancelSteal();
        ClientManager.Instance.RefreshMyInfo(false, b => PowerupManager.Instance.OnRoundStateChanged());
        SoundManager.Instance.PlayClip("Shield");

        AchievementManager.Instance.Set("booster_shield");
    }

    void _StealTimeStart(BinaryReader message)
    {
        PowerupManager.Instance.OnStealTimeActivated();
    }

    void _StealLetterStart(BinaryReader message)
    {
        PowerupManager.Instance.OnStealLetterActivated(message.ReadString());
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

    public void StealSuccess()
    {
        SendMsg("StealSuccess");
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

    public void OnDoOverUsed()
    {
        SendMsg("UseDoOver");
    }

    public void RequestUseShield()
    {
        SendMsg("UseShield");
    }
}