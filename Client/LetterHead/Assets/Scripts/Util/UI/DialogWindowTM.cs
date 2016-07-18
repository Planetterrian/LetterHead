using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindowTM : Singleton<DialogWindowTM>
{
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI title;
    public Button okayButton;
    public Button cancelButton;
    private Action okayCallback;
    private Action cancelCallback;

    public TextMeshProUGUI cancelButtonText;
    public TextMeshProUGUI okButtonText;

    public Transform butLeft;
    public Transform butRight;
    public Transform butBottom;


    private float okayStartingPos;

    private float startingHeight;

    protected override void Awake()
    {
        base.Awake();

        okayStartingPos = okayButton.transform.localPosition.x;
        startingHeight = GetComponent<RectTransform>().GetHeight();
    }

    void Start()
    {
        Hide();
    }

    public void Error(string errText)
    {
        Show("Error", errText, () => { });
    }

    public void Show(string titleText, string text, Action okCallback, Action cancelCallback = null, string okayText = "OK", string cancelText = "Cancel")
    {
        gameObject.SetActive(true);

        okayCallback = okCallback;
        okayButton.interactable = okCallback != null;
        mainText.text = text;
        title.text = titleText;
        this.cancelCallback = cancelCallback;

        cancelButtonText.text = cancelText;
        okButtonText.text = okayText;

        //GetComponent<Window>().Show();

        if (okayText == "")
        {
            // Info box
            cancelButton.gameObject.SetActive(false);
            okayButton.gameObject.SetActive(false);

            butLeft.gameObject.SetActive(false);
            butRight.gameObject.SetActive(false);
            butBottom.gameObject.SetActive(false);
        }
        else if (cancelCallback == null)
        {
            okayButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);

            butLeft.gameObject.SetActive(false);
            butRight.gameObject.SetActive(false);
            butBottom.gameObject.SetActive(true);

            okayButton.transform.SetParent(butBottom, false);
        }
        else
        {
            okayButton.gameObject.SetActive(true);

            butLeft.gameObject.SetActive(true);
            butRight.gameObject.SetActive(true);
            butBottom.gameObject.SetActive(false);

            okayButton.transform.SetParent(butRight, false);
            cancelButton.gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OKClicked()
    {
        Hide();
        okayCallback();
    }

    public void CancelClicked()
    {
        Hide();
        cancelCallback();
    }

}
