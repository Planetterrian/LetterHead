using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using UnityEngine;

public class ScoringManager : Singleton<ScoringManager>
{
    private List<string> submittedWords = new List<string>();
    private HashSet<int> usedLetterIds = new HashSet<int>(); 

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
}