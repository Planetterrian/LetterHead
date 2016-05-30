using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class PowerupRow : MonoBehaviour
{
    public TextMeshProUGUI badgeLabel;

    public void SetCount(int count)
    {
        badgeLabel.text = count.ToString("N0");
    }

    public void OnBuyClicked(string packageName)
    {
        
    }
}
