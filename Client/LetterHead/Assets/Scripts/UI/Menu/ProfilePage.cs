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
    public AvatarBox avatarBox;
    public Toggle useFacebookImageToggle;

    public TMP_InputField usernameInput;

    public AvatarSelectWindow avatarSelectWindow;

    public TextMeshProUGUI stat_gamesPlayed;
    public TextMeshProUGUI stat_wins;
    public TextMeshProUGUI stat_losses;
    public TextMeshProUGUI stat_winPct;
    public TextMeshProUGUI stat_bestScore;
    public TextMeshProUGUI stat_averageScore;
    public TextMeshProUGUI stat_mostWords;
    public TextMeshProUGUI stat_ties;

    private bool initDone;
    private bool dontAllowAvatarChange;

    void Init()
    {
        if (initDone)
            return;

        initDone = true;
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

    public void OnAvatarChanged(string avatarName)
    {
        ClientManager.Instance.myUserInfo.AvatarUrl = "sprite:" + avatarName;

        Srv.Instance.POST("User/SetAvatar",
            new Dictionary<string, string>() {{"sprite", avatarName}}, s =>
            {
                // Silently accept it
                ClientManager.Instance.RefreshMyInfo(false);
            });

        dontAllowAvatarChange = true;
        useFacebookImageToggle.isOn = false;
        dontAllowAvatarChange = false;

        Refresh();
    }

    public void OnUseFacebookImageChanged()
    {
        if(dontAllowAvatarChange)
            return;

        if (useFacebookImageToggle.isOn)
        {
            Srv.Instance.POST("User/UseFacebookImage", null, s =>
            {
                ClientManager.Instance.RefreshMyInfo(false);
            });

            ClientManager.Instance.myUserInfo.AvatarUrl = ClientManager.Instance.myUserInfo.FacebookPictureUrl;
            Refresh();
        }
        else
        {
            OnAvatarChanged("Picture1");
        }
    }

    public void Refresh()
    {
        dontAllowAvatarChange = true;
        TimerManager.AddEvent(0.2f, () => dontAllowAvatarChange = false);
        Init();

        usernameInput.text = ClientManager.Instance.myUserInfo.Username;

        useFacebookImageToggle.isOn = ClientManager.Instance.myUserInfo.FacebookPictureUrl == ClientManager.Instance.myUserInfo.AvatarUrl;

        useFacebookImageToggle.gameObject.SetActive(!string.IsNullOrEmpty(ClientManager.Instance.myUserInfo.FacebookPictureUrl));

        avatarBox.SetAvatarImage(ClientManager.Instance.myUserInfo.AvatarUrl);

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
                stat_ties.text = stats.gamesTied.ToString();
            });
    }

    public void ShowAvatarSelectWindow()
    {
        if (ClientManager.Instance.myUserInfo.AvatarUrl.StartsWith("sprite:"))
        {
            // Using a built in sprite
            avatarSelectWindow.selectedAvatar = ClientManager.Instance.myUserInfo.AvatarUrl.Substring(7);
        }
        else
        {
            avatarSelectWindow.selectedAvatar = "";
        }
        
        avatarSelectWindow.OnSelected = OnAvatarChanged;
        avatarSelectWindow.ShowModal();
    }
}
