using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lever : MonoBehaviour
{
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
                break;
            case State.Middle:
                destRotation = 0;
                break;
            case State.Bottom:
                destRotation = 65;
                break;
            default:
                throw new ArgumentOutOfRangeException("state", state, null);
        }
    }

}