using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoringManager : Singleton<ScoringManager>
{
    private List<string> submittedWords = new List<string>();

    public void OnWordSubmit()
    {
        var word = Speller.Instance.CurrentWord();

        if(WordManager.Instance.IsWordValid(word))
        {
            OnValidWordSubmitted(word);
        }
    }

    private void OnValidWordSubmitted(string word)
    {
        //if(submittedWords.Contains(word))
            //return;

        WordBox.Instance.AddWord(word);
        submittedWords.Add(word);
    }

    private void Start()
    {
    }
}