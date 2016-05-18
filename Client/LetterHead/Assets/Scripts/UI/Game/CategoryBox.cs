using System;
using System.Collections.Generic;
using LetterHeadShared;
using TMPro;
using UnityEngine;

class CategoryBox : Singleton<CategoryBox>
{
    public TextMeshProUGUI[] categoryTitles;
    public TextMeshProUGUI[] categoryValues;

    private Dictionary<TextMeshProUGUI, Category> categoryScoreFunctions = new Dictionary<TextMeshProUGUI, Category>(); 

    void Start()
    {
        for (int index = 0; index < ScoringManager.Instance.categoryManager.Categories.Count; index++)
        {
            var category = ScoringManager.Instance.categoryManager.Categories[index];
            categoryTitles[index].text = category.name;

            categoryScoreFunctions[categoryValues[index]] = category;
        }

        Refresh();
    }

    public void Refresh()
    {
        foreach (var categoryScoreFunction in categoryScoreFunctions)
        {
            categoryScoreFunction.Key.text = categoryScoreFunction.Value.GetScore(ScoringManager.Instance.Words(),
                ScoringManager.Instance.UniqueLetterCount()).ToString("N0");
            ;
        }
    }
}
