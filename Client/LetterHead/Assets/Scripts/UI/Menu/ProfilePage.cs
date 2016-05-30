using System.Collections.Generic;
using System.Linq;
using System.Text;
using LetterHeadShared.DTO;
using Newtonsoft.Json;
using TMPro;
using UI.Pagination;
using UnityEngine;
using UnityEngine.UI;

class ProfilePage : Page
{
    public Dropdown avatarDropdown;
    public AvatarBox facebookAvatarBox;
    public Toggle useFacebookImageToggle;

    public TMP_InputField usernameInput;

    public TextMeshProUGUI stat_gamesPlayed;
    public TextMeshProUGUI stat_wins;
    public TextMeshProUGUI stat_losses;
    public TextMeshProUGUI stat_winPct;
    public TextMeshProUGUI stat_bestScore;
    public TextMeshProUGUI stat_averageScore;
    public TextMeshProUGUI stat_mostWords;

    private bool initDone;

    void Init()
    {
        if (initDone)
            return;

        initDone = true;

        avatarDropdown.ClearOptions();

        foreach (var sprite in AvatarManager.Instance.avatars_sprites)
        {
            avatarDropdown.options.Add(new Dropdown.OptionData()
            {
                image = sprite,
                text = sprite.name
            });
        }

        avatarDropdown.value = -1;
    }

    public void OnNameChanged()
    {
        Srv.Instance.POST("User/SetUsername",
            new Dictionary<string, string>() { { "username", usernameInput.text } }, s =>
            {
                // Silently accept it
                ClientManager.Instance.RefreshMyInfo(false);
            });
    }

    public void OnAvatarChanged()
    {
        if(useFacebookImageToggle.isOn || avatarDropdown.value == -1)
            return;

        Srv.Instance.POST("User/SetAvatar",
            new Dictionary<string, string>() {{"sprite", avatarDropdown.options[avatarDropdown.value].text}}, s =>
            {
                // Silently accept it
                ClientManager.Instance.RefreshMyInfo(false);
            });
    }

    public void OnUseFacebookImageChanged()
    {
        avatarDropdown.gameObject.SetActive(!useFacebookImageToggle.isOn);
        facebookAvatarBox.gameObject.SetActive(useFacebookImageToggle.isOn);

        if (useFacebookImageToggle.isOn)
        {
            Srv.Instance.POST("User/UseFacebookImage", null, s =>
            {
                ClientManager.Instance.RefreshMyInfo(false);
            });
        }
        else
        {
            avatarDropdown.value = -1;
            avatarDropdown.value = Random.Range(0, avatarDropdown.options.Count);
        }
    }

    public void Refresh()
    {
        Init();

        usernameInput.text = ClientManager.Instance.myUserInfo.Username;

        useFacebookImageToggle.isOn = ClientManager.Instance.myUserInfo.FacebookPictureUrl == ClientManager.Instance.myUserInfo.AvatarUrl;

        useFacebookImageToggle.gameObject.SetActive(!string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.FacebookPictureUrl));

        if (!string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.FacebookPictureUrl))
        {
            facebookAvatarBox.SetAvatarImage(ClientManager.Instance.myUserInfo.FacebookPictureUrl);
        }

        if (ClientManager.Instance.myUserInfo.AvatarUrl.StartsWith("sprite:"))
        {
            avatarDropdown.gameObject.SetActive(true);
            facebookAvatarBox.gameObject.SetActive(false);

            var spriteName = ClientManager.Instance.myUserInfo.AvatarUrl.Substring(7);
            for (int index = 0; index < avatarDropdown.options.Count; index++)
            {
                var optionData = avatarDropdown.options[index];
                if (optionData.text == spriteName)
                {
                    avatarDropdown.value = index;
                    avatarDropdown.RefreshShownValue();
                    break;
                }
            }
        }
        else
        {
            avatarDropdown.gameObject.SetActive(false);
            facebookAvatarBox.gameObject.SetActive(true);
        }

        Srv.Instance.POST("User/Stats",
            new Dictionary<string, string>() {{"userId", ClientManager.Instance.myUserInfo.Id.ToString()}}, s =>
            {
                var stats = JsonConvert.DeserializeObject<UserStats>(s);

                stat_gamesPlayed.text = stats.gamesPlayed.ToString();
                stat_wins.text = stats.gamesWon.ToString();
                stat_losses.text = stats.gamesLost.ToString();

                if (stats.gamesPlayed == 0)
                    stat_winPct.text = "0%";
                else
                    stat_winPct.text = Mathf.RoundToInt(((float) stats.gamesWon/(float) stats.gamesPlayed)*100).ToString() + "%";

                stat_bestScore.text = stats.bestScore.ToString();
                stat_averageScore.text = stats.averageScore.ToString();
                stat_mostWords.text = stats.mostWords.ToString();
            });
    }
}
