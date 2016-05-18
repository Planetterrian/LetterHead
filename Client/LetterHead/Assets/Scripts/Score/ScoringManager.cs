using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using UnityEngine;

public class ScoringManager : Singleton<ScoringManager>
{
    private List<string> submittedWords = new List<string>();
    private HashSet<int> usedLetterIds = new HashSet<int>();

    private Category currentCategory;
    public CategoryManager categoryManager = new CategoryManager();


    public void OnWordSubmit()
    {
        var word = Speller.Instance.CurrentWord();

        if(WordManager.Instance.IsWordValid(word) && CanAcceptWord(word))
        {
            OnValidWordSubmitted(word);
        }
    }

    private bool CanAcceptWord(string word)
    {
        //if(submittedWords.Contains(word))
        //return false;

        return true;
    }

    private void OnValidWordSubmitted(string word)
    {
        foreach (var tile in Speller.Instance.tiles)
        {
            usedLetterIds.Add(tile.ID);
        }

        WordBox.Instance.AddWord(word);
        submittedWords.Add(word);

        UpdateCurrentRoundScore();
        WordCountBox.Instance.Refresh();
        CategoryBox.Instance.Refresh();
    }

    private void Start()
    {
    }

    public List<string> Words()
    {
        return submittedWords;
    }

    public int UniqueLetterCount()
    {
        return usedLetterIds.Count;
    }

    public int TotalScore()
    {
        if (!GameManager.Instance)
            return 0;

        var score = 0;
        var rounds = GameManager.Instance.MyRounds();

        foreach (var matchRound in rounds)
        {
            score += matchRound.Score;
        }

        return score;
    }

    public void OnCategorySelected(Category category)
    {
        currentCategory = category;

        GameManager.Instance.MyCurrentRound().CategoryName = category.name;
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
        return category.GetScore(Words(), UniqueLetterCount());
    }
}