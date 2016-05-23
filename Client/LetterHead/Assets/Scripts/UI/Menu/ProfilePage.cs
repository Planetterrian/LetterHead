using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UI.Pagination;
using UnityEngine.UI;

class ProfilePage : Page
{
    public Dropdown avatarDropdown;
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
            new Dictionary<string, string>() { { "sprite", usernameInput.text } }, s =>
            {
                // Silently accept it
            });
    }

    public void OnAvatarChanged()
    {
        Srv.Instance.POST("User/SetAvatar",
            new Dictionary<string, string>() {{"sprite", avatarDropdown.options[avatarDropdown.value].text}}, s =>
            {
                // Silently accept it
            });
    }

    public void Refresh()
    {
        Init();

        usernameInput.text = ClientManager.Instance.myUserInfo.Username;

        if (ClientManager.Instance.myUserInfo.AvatarUrl.StartsWith("sprite:"))
        {
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
    }
}
