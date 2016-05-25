using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryRow : MonoBehaviour
{
    public CategoryBox categoryBox;
    public Category category;

    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI scoreLabel;

    public void OnNameClicked()
    {
        Tooltip.Instance.Show(category.description, transform.position.y + 35 * PersistManager.Instance.persistCanvas.GetComponent<Transform>().localScale.y);
    }

    public void OnScoreClicked()
    {
        categoryBox.OnCategoryScoreClicked(category);
    }

    public void Set(Category category, CategoryBox categoryBox)
    {
        this.category = category;
        this.categoryBox = categoryBox;
        nameLabel.text = category.name;

        scoreLabel.text = "";
    }

    public void SetScore(int score, bool hasBeenUsed)
    {
        scoreLabel.text = score.ToString("N0");

        scoreLabel.color = hasBeenUsed ? Color.black : new Color(0.42f, 0.42f, 0.42f);
    }
}