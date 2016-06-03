using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Chomper : MonoBehaviour
{
    public Image spring;
    public Image top;
    public Image bottom;

    public float idleSpeed = 2;
    public float bitepeed = 5;
    public float moveInSpeed = 2;
    public float moveOutSpeed = 2;

    private RectTransform rectTransform;
    private float mouthAngle;
    private float targetMouthAngle;
    private Transform target;
    private Mode currentMode;

    public AudioSource warningSound;
    public AudioSource chompSound;

    private float startingXpos;

    private enum Mode
    {
        Idle, Open, OpenPause, Close, Hide
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
    }

    public void Begin(Transform target)
    {
        gameObject.SetActive(true);

        this.target = target;
        rectTransform.anchoredPosition = new Vector2(1.1f, 0);
        transform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);
        currentMode = Mode.Idle;
        startingXpos = transform.position.x;
        warningSound.enabled = true;
    }

    public void OnClicked()
    {
        PowerupManager.Instance.OnChompedClicked();
    }

    void Update()
    {
        var speed = 0f;

        switch (currentMode)
        {
            case Mode.Idle:
                speed = idleSpeed;
                break;
            case Mode.Open:
                speed = bitepeed;
                break;
            case Mode.Close:
                speed = bitepeed * 2;
                break;
        }

        mouthAngle = Mathf.LerpAngle(mouthAngle, targetMouthAngle, Time.deltaTime*speed);

        top.transform.localEulerAngles = new Vector3(top.transform.localEulerAngles.x, top.transform.localEulerAngles.y, -mouthAngle);
        bottom.transform.localEulerAngles = new Vector3(top.transform.localEulerAngles.x, top.transform.localEulerAngles.y, mouthAngle);

        if (Mathf.Abs(mouthAngle - targetMouthAngle) < 1f)
        {
            switch (currentMode)
            {
                case Mode.Idle:
                    targetMouthAngle = UnityEngine.Random.Range(0f, 5f);
                    break;
                case Mode.Open:
                    currentMode = Mode.OpenPause;
                    TimerManager.AddEvent(1.2f, () =>
                    {
                        targetMouthAngle = 0f;
                        currentMode = Mode.Close;
                    });
                    break;
                case Mode.Close:
                    OnBiteComplete();
                    break;
            }
        }

        if (target && currentMode == Mode.Idle)
        {
            var vecToTarget = (target.transform.position - transform.position);
            if (vecToTarget.magnitude < 1)
            {
                currentMode = Mode.Open;
                targetMouthAngle = 30f;
            }
            else
            {
                transform.Translate(vecToTarget.normalized * Time.deltaTime * moveInSpeed);
            }
        }

        if (currentMode == Mode.Hide)
        {
            transform.Translate(new Vector3(1, 0, 0) * Time.deltaTime * moveOutSpeed);
            if (transform.position.x >= startingXpos)
            {
                currentMode = Mode.Idle;
                gameObject.SetActive(false);
            }
        }
    }

    public void Hide()
    {
        currentMode = Mode.Hide;
    }

    private void OnBiteComplete()
    {
        TimerManager.AddEvent(0.5f, () => currentMode = Mode.Hide);
        PowerupManager.Instance.ChomperBite();
        warningSound.enabled = false;
        chompSound.Play();
    }
}