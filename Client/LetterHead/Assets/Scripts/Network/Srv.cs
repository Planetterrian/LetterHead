using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class Srv : REST
{
    public static Srv Instance;

    public string sessionId;
    public string baseUrl;

    void Awake()
    {
        Instance = this;
    }

    public override WWW GET(string url, Action<string> onComplete, Action<string> onError = null)
    {
        return base.GET(baseUrl + url, onComplete, onError);  
    }

    public override WWW POST(string url, Dictionary<string, string> post, Action<string> onComplete, Action<string> onError = null)
    {
        return base.POST(baseUrl + url, post, onComplete, onError);
    }

    protected override Dictionary<string, string> GetAdditionalHeaders()
    {
        if (!string.IsNullOrEmpty(sessionId))
        {
            return new Dictionary<string, string>() {
                    { "SessionId", sessionId }};
        }

        return null;
    }

    protected override void OnResults(string results, Action<string> onComplete, Action<string> onError)
    {
        Debug.Log(results);

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

        base.OnResults(results, onComplete, onError);
    }
}