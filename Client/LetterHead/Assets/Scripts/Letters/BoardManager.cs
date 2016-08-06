using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using UnityEngine;
using System.Collections;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class BoardManager : Singleton<BoardManager>
{
    public float tileScale = 1;
    public Vector2 gridSize;
    public Vector2 tileSpacing;

    private List<Tile> tiles = new List<Tile>();
    private float xStartOffset;
    private float yStartOffset;

    private bool initilized;

    [HideInInspector]
    public Vector2 tileSize;

    private string forcedSpellWord = "";

    public enum RemoveEffect
    {
        None, Starburst, Keep
    }

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        StartCoroutine(DoGloss());
    }

    private IEnumerator DoGloss()
    {
        yield break;

        yield return new WaitForSeconds(Random.Range(20, 30));

        for (int p = 0; p < (gridSize.y * 2); p++)
        {
            for (int q = Mathf.Max(0, p - (int)gridSize.x); q < Mathf.Min(p + 1, gridSize.x); q++)
            {
                var x = q;
                var y = p - q;

                GetTile(x, y).DoGloss();
            }

            yield return new WaitForSeconds(0.07f);
        }

        StartCoroutine(DoGloss());
    }

    // Use this for initialization
    void Initilize()
    {
        if (initilized)
            return;

        initilized = true;

        var gridRect = GetComponent<RectTransform>().rect;

        tileSize = TileManager.Instance.tilePrefab.GetComponent<RectTransform>().GetSize() * tileScale;

        xStartOffset = -(gridRect.width / 2) + 1;
        xStartOffset += (gridRect.width - (gridSize.x * ((tileSize.x) + tileSpacing.x)) + tileSpacing.x) / 2;

        yStartOffset = -(gridRect.height / 2);
        yStartOffset += (gridRect.height - (gridSize.y * ((tileSize.y) + tileSpacing.y)) + tileSpacing.y) / 2;
    }
    

    private Tile GetTileByLetter(LetterDefinition letterDefinition, bool disabledOnly)
    {
        foreach (var tile in tiles)
        {
            if (tile.IsEnabled() && disabledOnly)
                continue;

            if (tile.letterDefinition == letterDefinition)
                return tile;
        }

        return null;
    }


    public void SetBoardLetters(string letters, bool hidden)
    {
        Initilize();

        foreach (var tile in new List<Tile>(tiles))
        {
            RemoveTile(tile, RemoveEffect.None);
        }

        var letterNum = 0;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (!GetTile(x, y) && letterNum < letters.Length)
                {
                    var tile = AddNewTile(x, y, TileManager.Instance.CharToLetter(letters[letterNum]));

                    tile.ID = letterNum + 1;

                    if (hidden)
                    {
                        tile.HideLetter();
                    }

                    RepositionTile(tile);

                    letterNum++;
                }
            }
        }

        if (GameManager.Instance.IsMyRound())
        {
            BackgroundWordSearch.Instance.DoSearch();
        }
    }


    public void Shuffle()
    {
        tiles.Shuffle();

        var letterNum = 0;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if(letterNum >= tiles.Count)
                    break;

                tiles[letterNum].xBoardPos = x;
                tiles[letterNum].yBoardPos = y;
                RepositionTile(tiles[letterNum]);

                letterNum++;
            }
        }
    }

    public void RevealLetters()
    {
        foreach (var tile in tiles)
        {
            tile.RevealLetter(Random.Range(0f, 0.4f));
        }
    }

    public void GenerateRandomBoard(string preventLetters = "")
    {
        Initilize();

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (!GetTile(x, y))
                {
                    var tile = AddNewTile(x, y, GetRandomLetter(preventLetters));
                    RepositionTile(tile);
                }
            }
        }
    }

    private Tile AddNewTile(int x, int y, LetterDefinition letter)
    {
        var tile = TileManager.Instance.GetTile(letter);
        tile.Mode = Tile.TileMode.Grid;

        tile.transform.SetParent(transform);
        tile.transform.localScale = new Vector3(tileScale, tileScale, tileScale);

        tile.xBoardPos = x;
        tile.yBoardPos = y;

        tiles.Add(tile);

        return tile;
    }

    private void RepositionTile(Tile tile, bool animate = false, Vector2? startingPosOffset = null)
    {
        var pos = BoardPosToLocalSpace(tile.xBoardPos, tile.yBoardPos);

        if (animate)
        {
            if (startingPosOffset.HasValue)
            {
                // Start the tile off somewhere else

                var startPos = BoardPosToLocalSpace(tile.xBoardPos + (int)startingPosOffset.Value.x, tile.yBoardPos + (int)startingPosOffset.Value.y);
                tile.transform.localPosition = startPos;
            }

            tile.AnimateToPosition(pos, .175f);
        }
        else
        {
            tile.transform.localPosition = pos;
        }
    }

    private Vector2 BoardPosToLocalSpace(int xBoardPos, int yBoardPos)
    {
        var xPos = (xBoardPos * ((tileSize.x) + tileSpacing.x)) + xStartOffset + (tileSize.x / 2);
        var yPos = -((yBoardPos * ((tileSize.y) + tileSpacing.y)) + yStartOffset) - tileSize.y / 2;

        return new Vector2(xPos, yPos);
    }

    public Tile GetTileById(int id)
    {
        return tiles.FirstOrDefault(t => t.ID == id);
    }

    public void SelectTile(Tile tile)
    {
        tile.Select();
        Speller.Instance.AddTile(tile);
    }

    public void DeselectTile(Tile tile)
    {
        tile.Deselect();
        Speller.Instance.RemoveTile(tile);
    }

    public void ClearCurrentWord()
    {
        var selected = SelectedTiles();

        foreach (var tile in selected)
        {
            DeselectTile(tile);
        }

        Speller.Instance.ClearTiles();
    }

    public List<Tile> SelectedTiles()
    {
        return tiles.Where(t => t.IsSelected()).ToList();
    }

    public void RemoveSelectedTiles(RemoveEffect effect)
    {
        var selected = SelectedTiles();

        foreach (var tile in selected)
        {
            RemoveTile(tile, effect);
        }

        var replenishDelay = 0f;

        if (effect == RemoveEffect.Starburst)
            replenishDelay = 0.5f;

        TimerManager.AddEvent(replenishDelay, ShiftAndReplenish);
    }
    

    private List<Tile> NearbyTiles(Tile fromTile)
    {
        return tiles.Where(t =>
            t.xBoardPos == fromTile.xBoardPos && t.yBoardPos == fromTile.yBoardPos - 1 ||
            t.xBoardPos == fromTile.xBoardPos && t.yBoardPos == fromTile.yBoardPos + 1 ||
            t.xBoardPos == fromTile.xBoardPos + 1 && t.yBoardPos == fromTile.yBoardPos ||
            t.xBoardPos == fromTile.xBoardPos - 1 && t.yBoardPos == fromTile.yBoardPos
            ).ToList();
    }

    public void ShiftAndReplenish()
    {
        ShiftTilesDown();
        ReplenishTiles();
    }

    public void RemoveTile(Tile tile, RemoveEffect effect)
    {
        tile.Deselect();
        if (effect == RemoveEffect.None)
        {
            PoolManager.Pools["UI"].Despawn(tile.transform);
        }
        else if (effect == RemoveEffect.Starburst)
        {
            tile.StarburstFadeout();
        }
        else if (effect == RemoveEffect.Keep)
        {
            
        }

        tiles.Remove(tile);
    }

    private void ReplenishTiles()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            var columnMissingCt = -1;
            for (int y = (int)gridSize.y - 1; y >= 0; y--)
            {
                var tile = GetTile(x, y);

                if (!tile)
                {
                    if (columnMissingCt == -1)
                    {
                        columnMissingCt = y + 1;
                    }

                    var newTile = AddNewTile(x, y, GetRandomLetter());
                    RepositionTile(newTile, true, new Vector2(0, -columnMissingCt));
                }
            }
        }
    }

    private LetterDefinition GetRandomLetter(string preventLetters = "")
    {
        while (true)
        {
            var letter = TileManager.Instance.GetRandomLetter();
            if (tiles.Count(t => t.letterDefinition == letter) >= letter.maxTiles)
            {
                continue;
            }

            if (preventLetters.Contains(letter.letter))
                continue;

            return letter;
        }
    }


    private void ShiftTilesDown()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = (int)gridSize.y - 1; y > 0; y--)
            {
                var tile = GetTile(x, y);

                if (!tile)
                {
                    // No tile here, shift the ones above it down
                    for (int tempY = y - 1; tempY >= 0; tempY--)
                    {
                        var aboveTile = GetTile(x, tempY);

                        if (aboveTile)
                        {
                            aboveTile.yBoardPos = y;
                            RepositionTile(aboveTile, true);
                            break;
                        }
                    }
                }
            }
        }
    }

    private Tile GetTile(int x, int y)
    {
        return tiles.FirstOrDefault(t => t.xBoardPos == x && t.yBoardPos == y);
    }

    public void PopulateWord(string populatedWord)
    {
        Initilize();

        var positions = GetFreeBoardSpots(populatedWord.Length);
        if (positions == null)
        {
            Debug.LogError("No room to place the word " + populatedWord);
            return;
        }

        var chars = populatedWord.ToCharArray();
        var ct = 0;
        foreach (var c in chars)
        {
            var letter = TileManager.Instance.CharToLetter(c);
            var tile = AddNewTile((int)positions[ct].x, (int)positions[ct].y, letter);
            RepositionTile(tile);
            ct++;
        }
    }

    private List<Vector2> GetFreeBoardSpots(int length)
    {
        var maxTiles = (int)(gridSize.x * gridSize.y);
        if (maxTiles - tiles.Count < length)
            return null;

        var positions = new List<Vector2>();
        while (positions.Count < length)
        {
            var randX = Random.Range(0, (int)gridSize.x);
            var randY = Random.Range(0, (int)gridSize.y);

            var pos = new Vector2(randX, randY);
            if (!GetTile(randX, randY) && !positions.Contains(pos))
            {
                positions.Add(pos);
            }
        }

        return positions;
    }

    public List<Tile> Tiles(bool availableOnly = false, bool manualSelect = false)
    {
        if (availableOnly)
            return tiles.Where(t => t.CanBeSelected(manualSelect)).ToList();

        return tiles;
    }
    
    private IEnumerator SpellWord(string longestWord)
    {
        foreach (var letter in longestWord)
        {
            var tile = tiles.First(t => !t.IsSelected() && t.letterDefinition.letter.ToUpper()[0] == letter && t.CanBeSelected(true));
            SelectTile(tile);
            tile.selectSound.Play();
            yield return new WaitForSeconds(0.4f);
        }
    }

    public void DisableAllTiles()
    {
        foreach (var tile in tiles)
        {
            tile.SetEnabled(false);
        }
    }
    public void EnableAllTiles()
    {
        foreach (var tile in tiles)
        {
            tile.SetEnabled(true);
        }
    }

    public string GetForcedSpellWord()
    {
        return forcedSpellWord;
    }
    

    public void ShakeLetters(string[] letters)
    {
        foreach (var tile in tiles)
        {
            if (letters.Contains(tile.letterDefinition.letter))
            {
                tile.Shake();
            }
        }
    }

    public Tile GetTileWithLetter(string letter)
    {
        foreach (var tile in tiles)
        {
            if (tile.letterDefinition.letter.ToLower() == letter.ToLower())
            {
                return tile;
            }
        }

        return null;
    }

    public void ColorSelectedTiles(Color color)
    {
        foreach (var selectedTile in SelectedTiles())
        {
            selectedTile.MarkAsUsed();
        }
    }

    public void ColorTiles(int usedLetterIds, Color color)
    {
        foreach (var tile in tiles)
        {
            if (((1 << tile.ID) & usedLetterIds) == (1 << tile.ID))
            {
                tile.MarkAsUsed();
            }
        }
    }
}
