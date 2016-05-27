using System.Collections.Generic;
using PathologicalGames;
using UnityEngine;
using System.Collections;

public class TileManager : Singleton<TileManager>
{
    public LetterDefinition[] letters;
    public GameObject tilePrefab;

    public Sprite[] tileSprites;

    private float maxArrow;

	// Use this for initialization
    protected override void Awake()
    {
        base.Awake();
   
        foreach (var letterDefinition in letters)
        {
            maxArrow += letterDefinition.distibutionWeight;
        }
	}


    public Tile GetTile(LetterDefinition letter)
    {
        //var tileGo = Instantiate(tilePrefab) as GameObject;
        var tileGo = PoolManager.Pools["UI"].Spawn(tilePrefab);
        var tile = tileGo.GetComponent<Tile>();
        tile.SetLetter(letter, tileSprites[Random.Range(0, tileSprites.Length)]);

        tile.GetComponent<RectTransform>().SetSize(BoardManager.Instance.tileSize);

        return tile;
    }


    public LetterDefinition GetRandomLetter()
    {
        var arrow = Random.Range(0, maxArrow);

        var count = 0f;
        foreach (var letterDefinition in letters)
        {
            count += letterDefinition.distibutionWeight;

            if (arrow < count)
            {
                return letterDefinition;
            }
        }

        return null;
    }

    public LetterDefinition CharToLetter(char letter)
    {
        var letterIndex = (int)letter - 97;
        if (letterIndex < 0 || letterIndex > 26)
        {
            Debug.Log("Invalid letter " + letter);
            return null;
        }

        var letterDefinition = letters[letterIndex];
        return letterDefinition;
    }
}
