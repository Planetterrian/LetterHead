using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class REST : MonoBehaviour
{
    public virtual UnityWebRequest GET(string url, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        Debug.Log(url);
        var www = UnityWebRequest.Get(url);
       
        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    public virtual UnityWebRequest POST(string url, Dictionary<string, string> post, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        var queryString = "";

        if (post != null)
        {

            foreach (var kv in post)
            {
                queryString += kv.Key + "=" + kv.Value + "&";
            }

            queryString = "?" + queryString.TrimEnd('&');
        }

        url = url + queryString;

        Debug.Log(url);
        var www = UnityWebRequest.Get(url);

        var additionalHeaders = GetAdditionalHeaders();
        if (additionalHeaders != null)
        {
            foreach (var additionalHeader in additionalHeaders)
            {
                www.SetRequestHeader(additionalHeader.Key, additionalHeader.Value);
            }
        }

        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    protected virtual Dictionary<string, string> GetAdditionalHeaders()
    {
        return null;
    }

    private IEnumerator WaitForRequest(UnityWebRequest www, Action<string> onComplete, Action<string> onError)
    {
        yield return www.Send();
        // check for errors
        if (www.error == null)
        {
            var results = www.downloadHandler.data.Length == 0 ? "" : www.downloadHandler.text;
            OnResults(results, onComplete, onError, www.url);
        }
        else
        {
            if (onError != null)
                onError(www.error);

            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

    protected virtual void OnResults(string results, Action<string> onComplete, Action<string> onError, string url)
    {
        if (onComplete != null)
            onComplete(results);
    }
}