using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using TMPro;
using uTools;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.NativePlugins;

public class EndRoundWindow : WindowController
{
    public GridLayoutGroup longWordGrid;
    public GameObject longWordPrefab;

    public Toggle stealTimeToggle;
    public Toggle stealLetterToggle;

    public Button okButton;
    public Button shopButton;
    public Button leaderboardButton;

    public GameObject endTurnBottom;
    public GameObject endMatchBottom;

    public uTweenTransform crownTween;
    public AvatarBox leftAvatarBox;
    public AvatarBox rightAvatarBox;

    private void Start()
    {
    }

    void OnWindowShown()
    {
        okButton.interactable = true;
        shopButton.interactable = true;

        leaderboardButton.gameObject.SetActive(false);

        if (GameManager.Instance.PlayerCount() == 1)
        {
            endTurnBottom.SetActive(false);
            endMatchBottom.SetActive(false);

            if (GameManager.Instance.MatchDetails.IsDaily)
            {
                if (GameManager.Instance.MatchDetails.CurrentState == Match.MatchState.Ended)
                {
                    leaderboardButton.gameObject.SetActive(true);
                    AchievementManager.Instance.ReportScore(GameManager.Instance.MatchDetails.UserScore(0), "daily");
                }
            }
        }
        else
        {
            if (GameManager.Instance.MatchDetails.CurrentState == Match.MatchState.Ended)
            {
                endMatchBottom.SetActive(true);
                endTurnBottom.SetActive(false);

                leftAvatarBox.SetAvatarImage(GameManager.Instance.MatchDetails.Users[0].AvatarUrl);
                leftAvatarBox.SetName(GameManager.Instance.MatchDetails.Users[0].Username);
                leftAvatarBox.score.text = GameManager.Instance.MatchDetails.UserScore(0).ToString("N0");

                rightAvatarBox.SetAvatarImage(GameManager.Instance.MatchDetails.Users[1].AvatarUrl);
                rightAvatarBox.SetName(GameManager.Instance.MatchDetails.Users[1].Username);
                rightAvatarBox.score.text = GameManager.Instance.MatchDetails.UserScore(1).ToString("N0");

                var leftWon = GameManager.Instance.MatchDetails.UserScore(0) > GameManager.Instance.MatchDetails.UserScore(1);

                if(leftWon && ClientManager.Instance.UserId() == GameManager.Instance.MatchDetails.Users[0].Id || !leftWon && ClientManager.Instance.UserId() == GameManager.Instance.MatchDetails.Users[1].Id)
                    SoundManager.Instance.PlayClip("MatchWin");
                else
                    SoundManager.Instance.PlayClip("MatchLoss");

                crownTween.gameObject.SetActive(true);
                crownTween.from = new Vector3(leftWon ? leftAvatarBox.transform.localPosition.x : rightAvatarBox.transform.localPosition.x, crownTween.from.y, crownTween.from.z);
                crownTween.to = new Vector3(leftWon ? leftAvatarBox.transform.localPosition.x : rightAvatarBox.transform.localPosition.x, crownTween.to.y, crownTween.to.z);
                crownTween.ResetToInitialState();
                crownTween.Play();
            }
            else
            {
                endMatchBottom.SetActive(false);
                endTurnBottom.SetActive(true);

                stealTimeToggle.interactable = PowerupManager.Instance.CanUsePowerup(Powerup.Type.StealTime);
                stealLetterToggle.interactable = PowerupManager.Instance.CanUsePowerup(Powerup.Type.StealLetter);
            }
        }

        longWordGrid.transform.DeleteChildren();

        StartCoroutine(GetLongWords(5, ScoringManager.Instance.Words(), wordList =>
        {
            wordList = wordList.OrderByDescending(w => w.Length).ToList();

            var ct = 0;
            foreach (var word in wordList)
            {
                var wordGo = GameObject.Instantiate(longWordPrefab);
                wordGo.transform.SetParent(longWordGrid.transform);
                wordGo.transform.ResetToOrigin();
                wordGo.GetComponent<TextMeshProUGUI>().text = word.ToUpper();

                ct++;
                if(ct == 9)
                    break;
            }
        }));
    }

    public void OkClicked()
    {
        okButton.interactable = false;
        shopButton.interactable = false;

        if (GameManager.Instance.PlayerCount() == 1)
        {
            if (GameManager.Instance.MatchDetails.CurrentState == Match.MatchState.Ended)
            {
                PersistManager.Instance.LoadMenu();
            }
            else
            {
                Hide();
            }
        }
        else
        {
            if (stealTimeToggle.isOn)
            {
                RequestStealTime();
            }
            else if (stealLetterToggle.isOn)
            {
                RequestStealLetter();
            }
            else
            {
                PersistManager.Instance.LoadMenu();
            }
        }
    }

    public void ShopClicked()
    {
        if (stealTimeToggle.isOn)
        {
            RequestStealTime();
        }
        else if (stealLetterToggle.isOn)
        {
            RequestStealLetter();
        }
        else
        {
            PersistManager.Instance.LoadMenu(5);

        }
    }

    private void RequestStealTime()
    {
        PowerupManager.Instance.DoStealTime((b) =>
        {
            if (b)
            {
                PersistManager.Instance.LoadMenu();
            }
            else
            {
                okButton.interactable = true;
                shopButton.interactable = true;
            }
        });
    }

    private void RequestStealLetter()
    {
        PowerupManager.Instance.DoStealLetter((b) =>
        {
            if (b)
            {
                PersistManager.Instance.LoadMenu();
            }
            else
            {
                okButton.interactable = true;
                shopButton.interactable = true;
            }
        });
    }

    internal IEnumerator GetLongWords(int minLength, List<string> existingWords, Action<List<string>> callback)
    {
        var yoDawn = WordManager.Instance.DawgObj;

        var letters = new List<char>();

        var tiles = BoardManager.Instance.Tiles(true, true);
        for (int index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];
            letters.Add(tile.letterDefinition.letter.ToUpper()[0]);
        }

        var prefixes = new HashSet<string>();

        for (int i = 0; i < letters.Count; i++)
        {
            var firstLetter = letters[i];

            for (int x = 0; x < letters.Count; x++)
            {
                if (x == i)
                    continue;

                var secondLetter = letters[x];

                var prefix = firstLetter.ToString() + secondLetter;
                if (!prefixes.Contains(prefix))
                    prefixes.Add(prefix);
            }
        }

        yield return new WaitForEndOfFrame();

        var longWords = new List<string>();
        var prefixCount = prefixes.Count;

        for (int i = 0; i < prefixCount; i++)
        {
            var prefix = prefixes.ElementAt(i);

            var words = yoDawn.MatchPrefix(prefix).ToList();
            for (int index = 0; index < words.Count; index++)
            {
                var entry = words[index];
                var word = entry.Key;

                // Ignore words that are shorter than our existing longest
                if (word.Length <= minLength)
                    continue;


                var availableLetters = new List<char>(letters);
                bool canSpell = true;
                for (int index1 = 0; index1 < word.Length; index1++)
                {
                    var wordLetter = word[index1];
                    var indexOfLetter = availableLetters.IndexOf(wordLetter);
                    if (indexOfLetter == -1)
                    {
                        canSpell = false;
                        break;
                    }

                    availableLetters.RemoveAt(indexOfLetter);
                }

                if (canSpell && !existingWords.Contains(word))
                {
                    yield return new WaitForEndOfFrame();
                    longWords.Add(word);
                    if (longWords.Count > 50)
                    {
                        callback(longWords);
                        yield break;
                    }
                }
            }
        }

        callback(longWords);
    }

    public void LeaderboardClicked()
    {
        NPBinding.GameServices.ShowLeaderboardUIWithGlobalID("daily", eLeaderboardTimeScope.WEEK, error => { });
    }
}