using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Events;

public class KeyboardInput : MonoBehaviour
{
    [Serializable]
    public class KeyPressEvent : UnityEvent<string> { }
    public KeyPressEvent OnKeyPressed;
    public UnityEvent OnBackpsace;
    public UnityEvent OnLongBackspace;
    public UnityEvent OnEnter;

    private float backspaceDownTime;
    public float longPressDelay = 0.5f;

    void Update()
    {
        for (int i = 97; i < 123; i++)
        {
            if(Input.GetKeyDown((KeyCode)i))
            {
                OnKeyPressed.Invoke(((KeyCode)i).ToString().ToLower());
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            backspaceDownTime = Time.time;
            OnBackpsace.Invoke();
        }

        if (Input.GetKey(KeyCode.Backspace))
        {
            if (backspaceDownTime > 0 && Time.time - backspaceDownTime >= longPressDelay)
            {
                backspaceDownTime = 0;
                OnLongBackspace.Invoke();
            }
        }


        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            OnEnter.Invoke();
        }
    }
}
