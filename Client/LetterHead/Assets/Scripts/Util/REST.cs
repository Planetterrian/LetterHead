using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

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

        var bodyJsonString = JsonConvert.SerializeObject(post);

        var www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(bodyJsonString);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.chunkedTransfer = false;

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
        yield return www.SendWebRequest();
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