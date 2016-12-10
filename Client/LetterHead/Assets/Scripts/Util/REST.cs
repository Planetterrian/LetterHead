using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.Networking;

public class REST : MonoBehaviour
{
    public virtual UnityWebRequest GET(string url, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        var www = UnityWebRequest.Get(url);
       
        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    public virtual UnityWebRequest POST(string url, Dictionary<string, string> post, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        if (post == null)
        {
            post = new Dictionary<string, string>();
            post.Add("x", "");
        }

        var www = UnityWebRequest.Post(url, post);

        var additionalHeaders = GetAdditionalHeaders();
        if (additionalHeaders != null)
        {
            foreach (var additionalHeader in additionalHeaders)
            {
                www.SetRequestHeader(additionalHeader.Key, additionalHeader.Value);
            }
        }


        //WWW www = new WWW(url, form.data, headers);

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