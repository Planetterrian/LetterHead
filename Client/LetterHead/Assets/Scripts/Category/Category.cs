using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Category
{
    public string name;
    public string description;

    public Func<string, int> getWordScore;

}