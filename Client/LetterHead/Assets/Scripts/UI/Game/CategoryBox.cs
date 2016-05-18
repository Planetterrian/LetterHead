using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class CategoryBox : Singleton<CategoryBox>
{
    public TextMeshProUGUI[] categoryTitles;
    public TextMeshProUGUI[] categoryValues;

    public TextMeshProUGUI totallabel;

    private Dictionary<TextMeshProUGUI, Category> categoryScoreFunctions = new Dictionary<TextMeshProUGUI, Category>();

    public Image currentSelectedCategoryImage;

    void Start()
    {
        for (int index = 0; index < ScoringManager.Instance.categoryManager.Categories.Count; index++)
        {
            var category = ScoringManager.Instance.categoryManager.Categories[index];
            categoryTitles[index].text = category.name;
            categoryTitles[index].GetComponentInParent<Button>().onClick.AddListener(() => OnCategoryClicked(category));
            categoryScoreFunctions[categoryValues[index]] = category;
        }

        Refresh();
    }

    private void OnCategoryClicked(Category category)
    {
        if (!GameGui.CanSelectCategory())
            return;

        if(GameManager.Instance.HasCategoryBeenUsed(category))
            return;

        GameManager.Instance.SelectCategory(category);
    }

    public void Refresh()
    {
        foreach (var scoreFunc in categoryScoreFunctions)
        {
            var score = scoreFunc.Value.GetScore(ScoringManager.Instance.Words(),
                ScoringManager.Instance.UniqueLetterCount());

            if(score > 0)
                scoreFunc.Key.text = score.ToString("N0");
            else
                scoreFunc.Key.text = "";

            scoreFunc.Key.color = GameManager.Instance.MyRounds().Any(c => c.CategoryName == scoreFunc.Value.name) ? Color.black : new Color(0.42f, 0.42f, 0.42f);
        }

        totallabel.text = ScoringManager.Instance.TotalScore().ToString("N0");
    }

    public void SetCurrentlySelectedCategory(Category category)
    {
        currentSelectedCategoryImage.gameObject.SetActive(true);

        var box = categoryScoreFunctions.First(c => c.Value == category).Key.transform.parent.parent;
        currentSelectedCategoryImage.transform.position = box.position;
    }
}
