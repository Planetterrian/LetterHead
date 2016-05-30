using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBox : Singleton<CategoryBox>, IGameHandler
{
    public CategoryRow[] categoryRows;

    public TextMeshProUGUI totallabel;

    private Dictionary<CategoryRow, Category> categoryScoreFunctions = new Dictionary<CategoryRow, Category>();

    public Image currentSelectedCategoryImage;

    void Start()
    {
        GameScene.Instance.AddGameManger(this);
        GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);

        for (int index = 0; index < ScoringManager.Instance.categoryManager.Categories.Count; index++)
        {
            var category = ScoringManager.Instance.categoryManager.Categories[index];
            categoryRows[index].Set(category, this);

            categoryScoreFunctions[categoryRows[index]] = category;
        }

        Refresh();
    }

    private void OnMatchDetailsLoaded()
    {
        Refresh();

        if (!string.IsNullOrEmpty(GameManager.Instance.CurrentRound().CategoryName))
        {
            var category = ScoringManager.Instance.categoryManager.GetCategory(GameManager.Instance.CurrentRound().CategoryName);
            GameManager.Instance.SelectCategory(category, true);
        }
    }

    public void OnCategoryScoreClicked(Category category)
    {
        if (!GameGui.CanSelectCategory())
            return;

        if(GameManager.Instance.HasCategoryBeenUsed(category))
            return;

        if(category.alwaysActive)
            return;

        GameManager.Instance.SelectCategory(category);
    }

    public void Refresh()
    {
        var totalScore = 0;

        foreach (var scoreFunc in categoryScoreFunctions)
        {
            var score = 0;

            var existingRound = GameManager.Instance.MyRounds().FirstOrDefault(r => r.CategoryName == scoreFunc.Value.name);

            if (scoreFunc.Value.name == "Big Word Bonus")
            {
                foreach (var matchRound in GameManager.Instance.MyRounds())
                {
                    score += scoreFunc.Value.GetScore(matchRound.Words, 0, new List<int>());
                }

                totalScore += score;
            }
            else if (existingRound != null)
            {
                // This category has been used
                score = existingRound.Score;
                totalScore += score;
            }
            else
            {
                score = ScoringManager.Instance.GetCategoryScore(scoreFunc.Value);

                if (scoreFunc.Value.alwaysActive)
                    totalScore += score;
            }

            scoreFunc.Key.SetScore(score, scoreFunc.Value.alwaysActive || GameManager.Instance.MyRounds().Any(c => c.CategoryName == scoreFunc.Value.name));
        }

        totallabel.text = totalScore.ToString("N0");
    }

    public void SetCurrentlySelectedCategory(Category category)
    {
        currentSelectedCategoryImage.gameObject.SetActive(true);

        var box = categoryScoreFunctions.First(c => c.Value == category).Key.transform;
        currentSelectedCategoryImage.transform.position = box.position;
    }

    public void OnReset()
    {
        currentSelectedCategoryImage.gameObject.SetActive(false);
    }
}
