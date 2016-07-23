using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using Newtonsoft.Json;
using VoxelBusters.NativePlugins;
using VoxelBusters.NativePlugins.Internal;

public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<string, AchievementDescription> achievements = new Dictionary<string, AchievementDescription>();
    private float lastAchievementCheck = -100;


    public bool allowEditorAuth = false;

    void Start()
    {
        if(!Application.isEditor || allowEditorAuth)
            Authenticate();
    }

    public void CheckServerAchievements()
    {
        if(Time.time - lastAchievementCheck < 10)
            return;

        if(ClientManager.Instance.UserId() < 1)
            return;

        lastAchievementCheck = Time.time;

        if (!NPBinding.GameServices.LocalUser.IsAuthenticated)
        {
            return;
        }

        Srv.Instance.POST("User/AchiecvementInfo", null, s =>
        {
            var info = JsonConvert.DeserializeObject<ServerAchievementInfo>(s);

            // Played
            var _achievementGID = "play10";
            int _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesPlayed / (float)_noOfSteps);

            _achievementGID = "play50";
            _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesPlayed / (float)_noOfSteps);

            _achievementGID = "play100";
            _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesPlayed / (float)_noOfSteps);

            // Won
            _achievementGID = "win10";
            _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesWon / (float)_noOfSteps);

            _achievementGID = "win50";
            _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesWon / (float)_noOfSteps);

            _achievementGID = "win100";
            _noOfSteps = NPBinding.GameServices.GetNoOfStepsForCompletingAchievement(_achievementGID);
            SetProgress(_achievementGID, info.GamesWon / (float)_noOfSteps);

            if (info.HasWonAny)
            {
                Set("win");
            }

            if (info.HasWonDaily)
            {
                Set("win_daily");
            }

            if (info.HasThreeGameStreak)
            {
                Set("win3row");
            }

        });
    }

    private void Authenticate()
    {
        NPBinding.GameServices.LocalUser.Authenticate((bool _success, string _error) => 
        {
            if (_success)
            {
                Debug.Log("Sign-In Successfully");
                Debug.Log("Local User Details : " + NPBinding.GameServices.LocalUser.ToString());

                LoadAchievementDescriptions();
            }
            else
            {
                Debug.Log("Sign-In Failed");
            }
        });
    }

    private float GetAchievementProgress(string id)
    {
        return PlayerPrefs.GetFloat("ach_" + id, 0);
    }

    public void Add(string _achievementID, float qty = 1)
    {
        SetProgress(_achievementID, GetAchievementProgress(_achievementID) + qty);
    }


    public void ReportScore(int score, string _leaderboardID)
    {
        if (!NPBinding.GameServices.LocalUser.IsAuthenticated)
        {
            return;
        }

        NPBinding.GameServices.ReportScoreWithGlobalID(_leaderboardID, score, (bool _status, string error) => {
            if (_status)
                Debug.Log(string.Format("Successfully reported score={0} to leaderboard with ID={1}.", score, _leaderboardID));
            else
                Debug.Log(string.Format("Failed to report score to leaderboard with ID={0} Err {1}.", _leaderboardID, error));
        });
    }


    private void LoadAchievementDescriptions()
    {
        achievements.Clear();

        NPBinding.GameServices.LoadAchievementDescriptions((AchievementDescription[] _descriptions, string _error) => 
        {
            Debug.Log("Achivements loaded");

            if (_descriptions != null)
            {
                int _descriptionCount = _descriptions.Length;

                for (int _iter = 0; _iter < _descriptionCount; _iter++)
                {
                    var ach = _descriptions[_iter];
                    achievements[ach.Identifier] = ach;
                }
            }

            CheckServerAchievements();
        });
    }

    private AchievementDescription GetAchievementDescription(string id)
    {
        string _achievementID = GameServicesUtils.GetAchievementID(id);
        return achievements[_achievementID];
    }

    void SetProgress(string _achievementID, float pct)
    {
        if (!NPBinding.GameServices.LocalUser.IsAuthenticated)
        {
            return;
        }

        if (pct > 1)
            pct = 1;

        if (GetAchievementProgress(_achievementID) == pct)
            return;

        PlayerPrefs.SetFloat("ach_" + _achievementID, pct);

        pct *= 100f;

        NPBinding.GameServices.ReportProgressWithGlobalID(_achievementID, pct, (bool _status, string _error) => {

            if (_status)
                Debug.Log(string.Format("Successfully reported points={0} to achievement with ID={1}.", pct, _achievementID));
            else
                Debug.Log(string.Format("Failed to report progress of achievement with ID={0}.", _achievementID));
        });
    }

    public void Set(string _achievementID, int qty = 1)
    { 
        SetProgress(_achievementID, qty);
    }

    public void ShowAchievements()
    {
        if (!NPBinding.GameServices.LocalUser.IsAuthenticated)
        {
            Authenticate();
            return;
        }

        NPBinding.GameServices.ShowAchievementsUI((string _error) =>
        {
            Debug.Log("Closed achievements UI.");
        });
    }
}
