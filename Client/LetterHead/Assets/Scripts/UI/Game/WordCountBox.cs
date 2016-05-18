using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordCountBox : Singleton<WordCountBox>
{
    public TextMeshProUGUI wordLength3Label;
    public TextMeshProUGUI wordLength4Label;
    public TextMeshProUGUI wordLength5Label;
    public TextMeshProUGUI wordLength6Label;
    public TextMeshProUGUI wordLength7Label;
    public TextMeshProUGUI totalLabel;
    public TextMeshProUGUI bestLetterLabel;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var words = ScoringManager.Instance.Words();

        wordLength3Label.text = words.Count(w => w.Length == 3).ToString("N0");
        wordLength4Label.text = words.Count(w => w.Length == 4).ToString("N0");
        wordLength5Label.text = words.Count(w => w.Length == 5).ToString("N0");
        wordLength6Label.text = words.Count(w => w.Length == 6).ToString("N0");
        wordLength7Label.text = words.Count(w => w.Length >= 7).ToString("N0");

        totalLabel.text = words.Count.ToString("N0");
    }
}
