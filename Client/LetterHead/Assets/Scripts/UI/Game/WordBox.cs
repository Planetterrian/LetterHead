using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordBox : MonoBehaviour
{
    public RectTransform contentBox;
    public GameObject rowPrefab;
    public Color altBackgroundColor;
    public int startingRows;
    public float rowHeight;

    private List<GameObject> rows = new List<GameObject>();

    private void Awake()
    {
    }

    private void Start()
    {
        for (int i = 0; i < startingRows; i++)
        {
            AddRow();
        }
    }

    private void AddRow()
    {
        var rowGo = GameObject.Instantiate(rowPrefab) as GameObject;
        rowGo.transform.SetParent(contentBox);
        rowGo.transform.ResetToOrigin();
        rows.Add(rowGo);

        contentBox.SetHeight(rowHeight * rows.Count);
    }
}