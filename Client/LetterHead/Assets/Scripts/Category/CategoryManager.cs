using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CategoryManager : MonoBehaviour
{
    private List<Category> categories = new List<Category>();

	// Use this for initialization
	void Start () {
	    categories.Add(new Category()
	                   {
	                       name = "Roll Call",
                           description = "Use all letters at least once",
                           getWordScore = s =>
                           {
                               if (s.Length == 10)
                                   return 20;

                               return 0;
                           }

        });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
