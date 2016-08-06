using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using LetterHeadShared.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryBox : MonoBehaviour, IGameHandler
{
    public CategoryRow[] categoryRows;

    public TextMeshProUGUI totallabel;
    public bool isPrimary;

    private Dictionary<CategoryRow, Category> categoryScoreFunctions = new Dictionary<CategoryRow, Category>();

    public CategoryHighlighter currentSelectedCategoryImage;

    void Start()
    {
        if (isPrimary)
        {
            GameScene.Instance.AddGameManger(this);
            GameManager.Instance.OnMatchDetailsLoadedEvent.AddListener(OnMatchDetailsLoaded);
            GameScene.Instance.OnStateChanged.AddListener(OnGameStateChanged);
        }

        for (int index = 0; index < ScoringManager.Instance.categoryManager.Categories.Count; index++)
        {
            var category = ScoringManager.Instance.categoryManager.Categories[index];
            categoryRows[index].Set(category, this);

            categoryScoreFunctions[categoryRows[index]] = category;
        }

        RefreshMyRounds();
    }

    private void OnMatchDetailsLoaded()
    {
        if (!string.IsNullOrEmpty(GameManager.Instance.CurrentRound().CategoryName))
        {
            var category = ScoringManager.Instance.categoryManager.GetCategory(GameManager.Instance.CurrentRound().CategoryName);
            GameManager.Instance.SelectCategory(category, true);
        }

        Refresh(GameManager.Instance.MyRounds());
    }

    private void OnGameStateChanged()
    {
        if (isPrimary && GameScene.Instance.CurrentState == GameScene.State.WaitingForCategory)
        {
            RefreshMyRounds();
        }
    }

    public void OnCategoryScoreClicked(Category category, string scoreText)
    {
        if (CategoryInvalid(category))
            return;

        if (scoreText == "0" || scoreText == "")
        {
            DialogWindowTM.Instance.Show("Select Score", "Lock in the " + category.name + " category for " + scoreText + " points?", () => DoCategorySelect(category), () => { }, "Confirm", "Cancel");
        }
        else
        {
            DoCategorySelect(category);
        }
    }

    private static bool CategoryInvalid(Category category)
    {
        if (!GameGui.CanSelectCategory())
            return true;

        if (GameManager.Instance.HasCategoryBeenUsed(category))
            return true;

        if (category.alwaysActive)
            return true;
        return false;
    }


    public void HighlightSelectableCategories()
    {
        foreach (var categoryRow in categoryRows)
        {
            if (CategoryInvalid(categoryRow.category))
                continue;

            categoryRow.ToggleHighlight(true);
        }
    }

    public void HideHighlights()
    {
        foreach (var categoryRow in categoryRows)
        {
            categoryRow.ToggleHighlight(false);
        }
    }

    private static void DoCategorySelect(Category category)
    {
        GameManager.Instance.SelectCategory(category);
        SoundManager.Instance.PlayClip("Circle Score");
    }

    public void Refresh(List<MatchRound> rounds, bool playValidCategorySound = false)
    {
        var totalScore = 0;

        foreach (var scoreFunc in categoryScoreFunctions)
        {
            var score = 0;

            var existingRound = rounds.FirstOrDefault(r => r.CategoryName == scoreFunc.Value.name);

            if (scoreFunc.Value.name == "Big Word Bonus")
            {
                foreach (var matchRound in rounds)
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
                if (isPrimary)
                    score = ScoringManager.Instance.GetCategoryScore(scoreFunc.Value);
                else
                    score = 0;

                if (scoreFunc.Value.alwaysActive)
                    totalScore += score;
            }

            var forceShow = GameManager.Instance.IsMyRound() && GameScene.Instance.CurrentState == GameScene.State.WaitingForCategory;
            scoreFunc.Key.SetScore(score, scoreFunc.Value.alwaysActive || rounds.Any(c => c.CategoryName == scoreFunc.Value.name), forceShow, playValidCategorySound);
        }

        totallabel.text = totalScore.ToString("N0");

        if(isPrimary)
            ScoringManager.Instance.currentRoundScore = totalScore;
    }

    public void SetCurrentlySelectedCategory(Category category, bool instant = false)
    {
        currentSelectedCategoryImage.gameObject.SetActive(true);

        var box = categoryScoreFunctions.First(c => c.Value == category).Key;
        currentSelectedCategoryImage.transform.position = box.scoreLabel.transform.position;

        if (instant)
        {
            currentSelectedCategoryImage.Show();
        }
        else
        {
            currentSelectedCategoryImage.StartAnimation();
        }
    }

    public void OnReset()
    {
        currentSelectedCategoryImage.gameObject.SetActive(false);
    }

    public void RefreshMyRounds(bool playValidCategorySound = false)
    {
        Refresh(GameManager.Instance.MyRounds(), playValidCategorySound);
    }
}
