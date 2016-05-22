using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Pagination
{
    public class PaginationButton : MonoBehaviour
    {
        public Text Text;
        public Button Button;

        [Tooltip("Only used by the First/Last Previous/Next buttons, although you can add custom buttons using this flag as well if you wish.")]
        public bool DontUpdate = false;

        public void SetText(string text)
        {
            Text.text = text;
        }

        public void SetText(int pageNumber)
        {
            SetText(pageNumber.ToString());
        }
    }
}
