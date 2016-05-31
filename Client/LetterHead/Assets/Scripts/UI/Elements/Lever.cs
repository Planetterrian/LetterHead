using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Lever : MonoBehaviour
{
    public Image leverBase;
    public Image leverHandle;

    public Sprite handleNormal;
    public Sprite handleGlow;

    public Sprite baseNormal;
    public Sprite baseGlow;

    public float speed;

    public enum State
    {
        Top, Middle, Bottom
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
            case State.Top:
                destRotation = -65;
                leverHandle.sprite = handleGlow;
                leverBase.sprite = baseGlow;
                break;
            case State.Middle:
                destRotation = 0;
                leverHandle.sprite = handleNormal;
                leverBase.sprite = baseNormal;
                break;
            case State.Bottom:
                destRotation = 65;
                leverHandle.sprite = handleNormal;
                leverBase.sprite = baseNormal;
                break;
            default:
                throw new ArgumentOutOfRangeException("state", state, null);
        }
    }

}