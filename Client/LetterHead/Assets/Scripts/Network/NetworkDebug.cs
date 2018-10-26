using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkDebug : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        Debug.Log("Test 1");
        var post = new Dictionary<string, string>();
        post.Add("x", "");
        var url = "http://letterhead.azurewebsites.net/Chat/GetAllMessages";
        var www = UnityWebRequest.Post(url, post);
        yield return www.SendWebRequest();
        Debug.Log(www.isHttpError + " " + www.isNetworkError);
        Debug.Log(www.error);
        Debug.Log("DONE TEST");
        yield return new WaitForSeconds(1);


        Debug.Log("Test 2");
        post = new Dictionary<string, string>();
        post.Add("x", "");
        url = "https://letterhead.azurewebsites.net/Chat/GetAllMessages";
        www = UnityWebRequest.Post(url, post);
        yield return www.SendWebRequest();
        Debug.Log(www.isHttpError + " " + www.isNetworkError);
        Debug.Log(www.error);
        Debug.Log("DONE TEST");
        yield return new WaitForSeconds(1);


        Debug.Log("Test 4");
        url = "http://www.google.com";
        www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        Debug.Log(www.isHttpError + " " + www.isNetworkError);
        Debug.Log(www.error);
        Debug.Log("DONE TEST");
        yield return new WaitForSeconds(1);


        Debug.Log("Test 5");
        url = "https://letterhead.azurewebsites.net/Chat/GetAllMessages";
        using (WWW www2 = new WWW(url))
        {
            yield return www;
            Debug.Log(www2.error);
            Debug.Log("DONE TEST");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
