using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DynamicCellSize : MonoBehaviour
{
    private GridLayoutGroup grid;

    private void Awake()
    {

    }

    private void Start()
    {
        grid = GetComponent<GridLayoutGroup>();

        var childCount = grid.transform.childCount;
        var width = GetComponent<RectTransform>().GetWidth();

        grid.cellSize = new Vector2(width / (float)childCount, grid.cellSize.y);
    }
}