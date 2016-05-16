using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DawgSharp;
using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class WordManager : Singleton<WordManager>
{
    public string wordListFileName;
    public string powerWordListFileName;
    public bool debugValid;

    private Dawg<bool> dawg;
    private Dawg<bool> powerWordDawg;

    public class SpelledWord
    {
        public LinkedList<Tile> tiles; 
        public string word;
        public int score;
    }

	// Use this for initialization
	void Start ()
	{
        //BuildDawg();
	    //BuildPowrWordDawg();

        StartCoroutine(LoadWordList());
        StartCoroutine(LoadPowerWordList());
    }

    private void BuildDawg()
    {
        var dawgBuilder = new DawgBuilder<bool>();
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "sowpods.txt");

        var result = "";
        result = System.IO.File.ReadAllText(filePath);


        var lines = result.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

        var lineLen = lines.Length;
        for (int index = 0; index < lineLen; index++)
        {
            var s = lines[index];
            if (s.Length > 2)
                dawgBuilder.Insert(s, true);
        }

        var dawg = dawgBuilder.BuildDawg();

        dawg.SaveTo(File.Create(System.IO.Path.Combine(Application.streamingAssetsPath, "DAWG.bytes")), (writer, value) => writer.Write(value)); // explained below
    }
    
    private IEnumerator LoadWordList()
    {
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, wordListFileName);

        byte[] result;
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            result = www.bytes;
        }
        else
            result = System.IO.File.ReadAllBytes(filePath);

        dawg = Dawg<bool>.Load(new MemoryStream(result), reader => reader.ReadBoolean());           // explained below
    }


    private IEnumerator LoadPowerWordList()
    {
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, powerWordListFileName);

        byte[] result;
        if (filePath.Contains("://"))
        {
            WWW www = new WWW(filePath);
            yield return www;
            result = www.bytes;
        }
        else
            result = System.IO.File.ReadAllBytes(filePath);

        powerWordDawg = Dawg<bool>.Load(new MemoryStream(result), reader => reader.ReadBoolean());           // explained below
    }


    public bool IsWordValid(string word)
    {
        if (debugValid && Application.isEditor)
            return true;

        word = word.ToUpper();
        return dawg[word];
    }

    internal IEnumerator GetLongestWord(Action<string> callback, bool powerWord)
    {
        var yoDawn = powerWord ? powerWordDawg : dawg;

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
                if(!prefixes.Contains(prefix))
                    prefixes.Add(prefix);
            }
        }

        yield return new WaitForEndOfFrame();

        var longestLength = 0;
        var longestWord = "";
        var prefixCount = prefixes.Count;

        for (int i = 0; i < prefixCount; i++)
        {
            var prefix = prefixes.ElementAt(i);

            var words = yoDawn.MatchPrefix(prefix).ToList();
            for (int index = 0; index < words.Count; index++)
            {
                Debug.Log(words.Count);
                var entry = words[index];
                var word = entry.Key;

                // Ignore words that are shorter than our existing longest
                if (word.Length <= longestLength)
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

                if (canSpell)
                {
                    yield return new WaitForEndOfFrame();
                    longestLength = word.Length;
                    longestWord = word;
                }
            }
        }

        callback(longestWord);
    }
}
