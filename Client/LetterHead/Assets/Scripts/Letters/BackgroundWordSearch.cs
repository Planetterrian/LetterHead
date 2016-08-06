using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using ThreadPriority = System.Threading.ThreadPriority;

public class BackgroundWordSearch : Singleton<BackgroundWordSearch>
{
    private Thread thread;
    private Search searcher;

    private List<string> longWords = new List<string>();
    public List<string> LongWords
    {
        get
        {
            return longWords;
        }
        private set
        {
            longWords = value;
        }
    }

    public bool hasResults;

    public Action OnResults;


    public void DoSearch()
    {
        LongWords.Clear();
        hasResults = false;

        var tiles = BoardManager.Instance.Tiles(true, true);
        var letters = new List<char>();

        for (int index = 0; index < tiles.Count; index++)
        {
            var tile = tiles[index];
            letters.Add(tile.letterDefinition.letter.ToUpper()[0]);
        }
        
        if (thread != null && thread.IsAlive)
        {
            searcher.abort = true;
            thread.Abort();
        }

        searcher = new Search(letters, OnSearchFinished);
        thread = new Thread(searcher.Execute);
        thread.Priority = ThreadPriority.Lowest;
        thread.Start();
    }

    public void EndEarly()
    {
        if(searcher == null)
            return;

        searcher.earlyTerminate = true;
    }

    public void OnSearchFinished(List<string> results)
    {
        LongWords = results;
        hasResults = true;

        if (OnResults != null)
            OnResults();
    }

    private class Search
    {
        private List<char> letters;
        private Action<List<string>> onResult;

        public bool abort;
        public bool earlyTerminate;

        public Search(List<char> _letters,  Action<List<string>> _onResult)
        {
            letters = _letters;
            onResult = _onResult;
        }

        public void Execute()
        {
            var yoDawn = WordManager.Instance.DawgObj;
            var minLength = 6;

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

                    if (earlyTerminate)
                    {
                        Debug.Log("Terminating search early.");
                        Loom.QueueOnMainThread(() =>
                        {
                            onResult(longWords);
                        });

                        return;
                    }

                    if(abort)
                        return;

                    // Ignore words that are shorter than our existing longest
                    if (word.Length < minLength)
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

                    if (canSpell && !WordManager.Instance.IsBadWord(word))
                    {
                        Thread.Sleep(10);
                        longWords.Add(word);
                    }
                }
            }

            Loom.QueueOnMainThread(() =>
            {
                onResult(longWords);
            });
        }
    }
}