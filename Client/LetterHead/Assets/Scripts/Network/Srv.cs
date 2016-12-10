using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Experimental.Networking;

public class Srv : REST
{
    public static Srv Instance;

    public string baseUrl;
    public string editorUrl;

    public bool ignoreEditorUrl;

    void Awake()
    {
        Instance = this;
    }


    public string Url()
    {
        if (Application.isEditor && !ignoreEditorUrl)
        {
            if (System.Environment.UserName == "Pete")
            {
                return editorUrl;
            }
        }

        return baseUrl;
    }

    public override UnityWebRequest GET(string url, Action<string> onComplete, Action<string> onError = null)
    {
        return base.GET(Url() + url, onComplete, onError);  
    }

    public override UnityWebRequest POST(string url, Dictionary<string, string> post, Action<string> onComplete, Action<string> onError = null)
    {
        return base.POST(Url() + url, post, onComplete, onError);
    }

    protected override Dictionary<string, string> GetAdditionalHeaders()
    {
        if (ClientManager.Instance && !string.IsNullOrEmpty(ClientManager.Instance.SessionId))
        {
            return new Dictionary<string, string>() {
                    { "SessionId", ClientManager.Instance.SessionId }};
        }

        return null;
    }

    protected override void OnResults(string results, Action<string> onComplete, Action<string> onError, string url)
    {
        if(Application.isEditor)
            Debug.Log(url + ": " + results);

        try
        {
            var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(results);
            if (json.ContainsKey("Error"))
            {
                if (onError != null)
                    onError(json["Error"]);
                return;
            }
        }
        catch (Exception)
        {
            // ignored
        }

        base.OnResults(results, onComplete, onError, url);
    }

}