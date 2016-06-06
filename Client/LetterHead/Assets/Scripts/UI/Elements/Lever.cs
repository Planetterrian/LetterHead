using System;
using System.Collections.Generic;
using System.Linq;
using uTools;
using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour
{
    public Image leverBase;
    public uTweenAlpha leverBaseGlow;

    public Image leverHandle;
    public uTweenAlpha leverHandleGlow;


    public float speed;

    public enum State
    {
        On, Middle, Off
    }

    private RectTransform rect;

    private float destRotation;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        leverHandle = GetComponent<Image>();
    }

    void Update()
    {
        if (Mathf.Abs(rect.localEulerAngles.z - destRotation) > 0.0001f)
        {
            rect.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rect.localEulerAngles.z, destRotation, Time.deltaTime * speed));
        }
    }

    public void SetState(State state)
    {
        switch (state)
        {
            case State.On:
                destRotation = 65;

                leverBaseGlow.gameObject.SetActive(true);
                leverHandleGlow.gameObject.SetActive(true);
                break;
            case State.Middle:
                destRotation = 0;

                leverBaseGlow.gameObject.SetActive(false);
                leverHandleGlow.gameObject.SetActive(false);
                break;
            case State.Off:
                destRotation = -65;
                leverBaseGlow.gameObject.SetActive(false);
                leverHandleGlow.gameObject.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException("state", state, null);
        }
    }

}