using System;
using System.Collections.Generic;
using System.Linq;
using LetterHeadShared;
using TMPro;
using uTools;
using UnityEngine;
using UnityEngine.UI;

public class CategoryRow : MonoBehaviour
{
    public CategoryBox categoryBox;
    public Category category;

    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI scoreLabel;
    public uTweenAlpha highlight;

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
        categoryBox.OnCategoryScoreClicked(category, scoreLabel.text);
    }

    public void ToggleHighlight(bool state)
    {
        if (state)
            highlight.Play();
        else
        {
            if(highlight.factor > 0)
                highlight.Play(PlayDirection.Reverse);
        }
    }

    public void Set(Category category, CategoryBox categoryBox)
    {
        this.category = category;
        this.categoryBox = categoryBox;
        nameLabel.text = category.name;

        scoreLabel.text = "";
    }

    public void SetScore(int score, bool hasBeenUsed, bool forceShow, bool playValidCategorySound = false)
    {
        if (score == 0 && !hasBeenUsed && !forceShow)
            scoreLabel.text = "";
        else
        {
            if (playValidCategorySound && score > 0 && !category.alwaysActive && (scoreLabel.text == "" || scoreLabel.text == "0"))
            {
                SoundManager.Instance.PlayClip("CategoryValid");
                Debug.Log("New Category Valid");
            }

            scoreLabel.text = score.ToString("N0");
        }

        //scoreLabel.color = hasBeenUsed ? Color.black : new Color(0.42f, 0.42f, 0.42f);
    }
}