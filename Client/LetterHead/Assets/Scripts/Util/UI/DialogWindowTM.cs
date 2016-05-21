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

    public void Show(string titleText, string text, Action okCallback, Action cancelCallback = null, string okayText = "OK", string cancelText = "CANCEL")
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

        if (cancelCallback == null)
        {
            okayButton.transform.localPosition = new Vector3(0, okayButton.transform.localPosition.y, okayButton.transform.localPosition.z);
            cancelButton.gameObject.SetActive(false);
        }
        else
        {
            okayButton.transform.localPosition = new Vector3(okayStartingPos, okayButton.transform.localPosition.y, okayButton.transform.localPosition.z);
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
