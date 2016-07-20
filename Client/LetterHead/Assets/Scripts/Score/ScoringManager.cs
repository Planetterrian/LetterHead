using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using UnityEngine;

public class ScoringManager : Singleton<ScoringManager>, IGameHandler
{
    private List<string> submittedWords = new List<string>();

    private Category currentCategory;
    public CategoryManager categoryManager = new CategoryManager();
    public int usedLetterIds;
    public int currentRoundScore;
    public Color usedTileColor;

    private void Start()
    {
        GameScene.Instance.AddGameManger(this);
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);
    }

    public void OnReset()
    {
        Debug.Log("Score Reset");
        submittedWords.Clear();
        currentCategory = null;
        usedLetterIds = 0;
        currentRoundScore = 0;
        OnWordsChanged();
    }

    public void OnWordSubmit()
    {
        var word = Speller.Instance.CurrentWord();

        if (WordManager.Instance.IsWordValid(word) && CanAcceptWord(word) && !WordManager.Instance.IsBadWord(word))
        {
            OnValidWordSubmitted(word);
        }
        else
        {
            SoundManager.Instance.PlayClip("Reject Word");
            Speller.Instance.OnInvalidWord();
        }
    }

    private bool CanAcceptWord(string word)
    {
        if (!Application.isEditor)
        {
            if (submittedWords.Contains(word))
                return false;
        }

        return true;
    }

    private void OnValidWordSubmitted(string word, bool prepopulateWord = false)
    {
        if (!prepopulateWord)
        {
            foreach (var tile in Speller.Instance.tiles)
            {
                usedLetterIds |= 1 << tile.referencedTileID;
            }

            GameRealTime.Instance.AddWord(word, usedLetterIds);
            GameManager.Instance.CurrentRound().Words.Add(word);

            if (word.Length >= 8)
            {
                SoundManager.Instance.PlayClip("Big Word");
            }
            else
            {
                SoundManager.Instance.PlayClip("Accept Word");
            }

            if (word.Length >= 8)
                AchievementManager.Instance.Set("word8");

            if (word.Length >= 10)
                AchievementManager.Instance.Set("word10");

            var wordCount8 = submittedWords.Count(w => w.Length >= 8);
            if(wordCount8 >= 5)
            {
                AchievementManager.Instance.Set("long_words5");
            }
        }

        BoardManager.Instance.ColorSelectedTiles(usedTileColor);

        if (PersistManager.Instance.ClearWord)
            BoardManager.Instance.ClearCurrentWord();

        WordBox.Instance.AddWord(word);
        submittedWords.Add(word);

        OnWordsChanged();
    }

    public void OnWordsChanged()
    {
        UpdateCurrentRoundScore();
        WordCountBox.Instance.Refresh();
        GameGui.Instance.categoryBox.RefreshMyRounds();
    }

    private void OnMatchDetailsLoaded()
    {
        foreach (var word in GameManager.Instance.CurrentRound().Words)
        {
            OnValidWordSubmitted(word, true);
        }

        usedLetterIds = GameManager.Instance.CurrentRound().UsedLetterIds;
        BoardManager.Instance.ColorTiles(usedLetterIds, usedTileColor);
    }

    public List<string> Words()
    {
        return submittedWords;
    }

    public int UniqueLetterCount()
    {
        var ct = NumberOfSetBits(usedLetterIds);

        return ct;
    }

    private int NumberOfSetBits(int i)
    {
        i = i - ((i >> 1) & 0x55555555);
        i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
        return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }

    public void OnCategorySelected(Category category)
    {
        currentCategory = category;
        UpdateCurrentRoundScore();
    }

    private void UpdateCurrentRoundScore()
    {
        if (currentCategory == null)
        {
            GameManager.Instance.MyCurrentRound().Score = 0;
        }
        else
        {
            GameManager.Instance.MyCurrentRound().Score = GetCategoryScore(currentCategory);
        }
    }

    public int GetCategoryScore(Category category)
    {
        return category.GetScore(Words(), UniqueLetterCount(), ExistingCategoryScores());
    }

    private List<int> ExistingCategoryScores()
    {
        var scores = new int[categoryManager.Categories.Count];

        var rounds = GameManager.Instance.MyRounds().Where(m => m.Number <= GameManager.Instance.MatchDetails.CurrentRoundNumber);
        foreach (var round in rounds)
        {
            if (string.IsNullOrEmpty(round.CategoryName))
                continue;

            var categoryIndex = categoryManager.GetCategoryIndex(round.CategoryName);
            scores[categoryIndex] = round.Score;
        }

        return scores.ToList();
    }

    public Category SelectedCategory()
    {
        return currentCategory;
    }

}