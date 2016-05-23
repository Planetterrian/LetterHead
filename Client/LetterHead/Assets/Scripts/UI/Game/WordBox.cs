using System;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using UnityEngine;
using UnityEngine.UI;

public class WordBox : Singleton<WordBox>, IGameHandler
{
    public RectTransform contentBox;
    public GameObject rowPrefab;
    public Color altBackgroundColor;
    public ScrollRect scroll;

    public int startingRows;
    public float rowHeight;

    private List<WordRow> rows = new List<WordRow>();

    private void Start()
    {
        GameScene.Instance.AddGameManger(this);
        //AddStartingRows();

        ScrollToBottom();
    }

    private void AddStartingRows()
    {
        for (int i = 0; i < startingRows; i++)
        {
            AddRow();
        }
    }

    private WordRow AddRow()
    {
        var rowGo = PoolManager.Pools["UI"].Spawn(rowPrefab.transform);
        rowGo.transform.SetParent(contentBox);
        rowGo.transform.ResetToOrigin();

        var row = rowGo.GetComponent<WordRow>();
        row.SetWord("");

        rows.Add(row);

        row.background.color = rows.Count % 2 == 0 ? Color.white : altBackgroundColor;

        contentBox.SetHeight(rowHeight * rows.Count);

        return row;
    }

    public void AddWord(string word)
    {
        word = word.UppercaseFirst();

        foreach (var wordRow in rows)
        {
            if (string.IsNullOrEmpty(wordRow.word))
            {
                wordRow.SetWord(word);
                ScrollToBottom();
                return;
            }
        }

        AddRow().SetWord(word);
        ScrollToBottom();
    }

    private void ScrollToBottom()
    {
        scroll.verticalNormalizedPosition = 0;
    }

    public void OnReset()
    {
        foreach (var wordRow in rows)
        {
            PoolManager.Pools["UI"].Despawn(wordRow.transform, PoolManager.Pools["UI"].transform);

        }
        rows.Clear();

        AddStartingRows();
        ScrollToBottom();
    }
}