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

    private GameObject tooltipAnchor;

    void Awake()
    {
        tooltipAnchor = new GameObject("anchor", typeof(RectTransform));
        tooltipAnchor.transform.SetParent(transform);
        tooltipAnchor.transform.ResetToOrigin();

        tooltipAnchor.GetComponent<RectTransform>().anchoredPosition = new Vector2(tooltipAnchor.GetComponent<RectTransform>().anchoredPosition.x, tooltipAnchor.GetComponent<RectTransform>().anchoredPosition.y + 78);
    }

    public void OnNameDown()
    {
        Tooltip.Instance.Show(category.description, tooltipAnchor.transform.position.y);
    }

    public void OnNameUp()
    {
        Tooltip.Instance.Hide();
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