using UnityEngine;
using System.Collections;
using WebSocketSharp;

public class WebSocketManager : Singleton<WebSocketManager>
{

	// Use this for initialization
	void Start () {
	}

    public WebSocket MakeNew(string url)
    {
        var ws = new WebSocket(url);
        ws.Log.Output = (data, s) =>
        {
            Debug.Log(data.Message);
        };

        ws.Log.Level = LogLevel.Info;


        //Debug.Log("Making new socket to URL " + url);
        return ws;
    }

}
