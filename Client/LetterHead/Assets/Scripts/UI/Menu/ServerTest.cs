using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ServerTest : MonoBehaviour
{
    public InputField url;
    public Text output;

    private int success = 0;
    private int fail = 0;
    private bool stop = false;
    private bool isRunning = false;

    private void Start()
    {
    }

    public void OnStartClicked()
    {
        if (!isRunning)
        {
            stop = false;
            RequestAgain();
        }
        else
        {
            stop = true;
            isRunning = false;
        }
    }

    private void RequestAgain()
    {
        Srv.Instance.POST(url.text, null, s =>
        {
            success++;

            if(!stop)
                RequestAgain();

            UpdateOutput();
        }, s =>
        {
            fail++;

            if(!stop)
                RequestAgain();

            UpdateOutput();
        });
    }

    private void UpdateOutput()
    {
        output.text = "Attempts: " + (success + fail) + " Success: " + success + " Fail: " + fail;
    }
}