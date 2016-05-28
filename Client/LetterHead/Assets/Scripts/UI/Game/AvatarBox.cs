using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class AvatarBox : MonoBehaviour
{
    public RawImage avatarImage;
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI score;

    private string pendingUrl;

    // Use this for initialization
    void Start ()
    {
        if (!string.IsNullOrEmpty(pendingUrl))
        {
            StartCoroutine(LoadAvatarImage(pendingUrl));
            pendingUrl = "";
        }
    }

    public void SetName(string username)
    {
        nameLabel.text = username;
    }

    public void SetAvatarImage(string url)
    {
        if(string.IsNullOrEmpty(url))
            return;


        if (url.StartsWith("sprite:"))
        {
            // Using a built in sprite
            avatarImage.texture = AvatarManager.Instance.GetAvatarImage(url.Substring(7));
        }
        else
        {
            if (!gameObject.activeInHierarchy)
            {
                pendingUrl = url;
                return;
            }
            StartCoroutine(LoadAvatarImage(url));
        }
    }

    private IEnumerator LoadAvatarImage(string url)
    {
        WWW www = new WWW(url);

        yield return www;

        avatarImage.texture = www.texture;
    }
}
