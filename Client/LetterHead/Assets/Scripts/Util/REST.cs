using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class REST : MonoBehaviour
{
    public virtual WWW GET(string url, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        WWW www = new WWW(url);
        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    public virtual WWW POST(string url, Dictionary<string, string> post, System.Action<string> onComplete, System.Action<string> onError = null)
    {
        WWWForm form = new WWWForm();

        foreach (KeyValuePair<string, string> post_arg in post)
        {
            form.AddField(post_arg.Key, post_arg.Value);
        }

        WWW www = new WWW(url, form);

        StartCoroutine(WaitForRequest(www, onComplete, onError));
        return www;
    }

    private IEnumerator WaitForRequest(WWW www, Action<string> onComplete, Action<string> onError)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            var results = www.text;
            OnResults(results, onComplete, onError);
        }
        else
        {
            if (onError != null)
                onError(www.error);

            Debug.Log(www.error);
        }
    }

    protected virtual void OnResults(string results, Action<string> onComplete, Action<string> onError)
    {
        if (onComplete != null)
            onComplete(results);
    }
}