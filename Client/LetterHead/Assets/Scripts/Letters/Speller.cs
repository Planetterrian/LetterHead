﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using uTools;

public class Speller : Singleton<Speller>, IGameHandler
{
    public float maxTileScale = 1;
    public float padding;
    public Color tileColor;
    public LinkedList<Tile> tiles = new LinkedList<Tile>();

    public uTweener[] invalidTweens;
    public uTweener[] invalidTweensDupe;

    private float tileWidthDivider = 1.75f;

    [Serializable]
    public class SpellerEvent : UnityEvent
    {
    }

    public SpellerEvent onSpellerChangedEvent;

    void Start()
    {
        GameScene.Instance.AddGameManger(this);
        GameScene.Instance.OnStateChanged.AddListener(OnStateChanged);
        GameGui.Instance.timer.OnTimeExpired.AddListener(OnTimeExpired);
    }

    void Update()
    {
        CheckSubmitButtons();
    }

    private void OnTimeExpired()
    {
    }

    private void OnStateChanged()
    {
    }

    public Tile AddTile(Tile originalTile)
    {
        var tileOriginalSize = BoardManager.Instance.tileSize;
        var tile = TileManager.Instance.GetTile(originalTile.letterDefinition);
        
        tile.transform.SetParent(transform);
        tile.GetComponent<RectTransform>().SetWidth(tileOriginalSize.x / tileWidthDivider);
        tile.SetColor(tileColor);
        tile.MarkAsUsed();
        tile.Mode = Tile.TileMode.SpelledWord;
        tile.referencedTileID = originalTile.ID;
        tiles.AddLast(tile);

        tile.transform.position = originalTile.transform.position;
        
        RepositionLetters(tile);

        onSpellerChangedEvent.Invoke();

        return tile;
    }


    private void RepositionLetters(Tile popInTile = null)
    {
        var rect = GetComponent<RectTransform>().rect;

        var tileOriginalSize = BoardManager.Instance.tileSize;
        tileOriginalSize.x /= tileWidthDivider;

        var tileScale = rect.width / (tileOriginalSize.x * tiles.Count);

        if (tileScale > maxTileScale)
            tileScale = maxTileScale;

        var tileSize = tileOriginalSize.x * tileScale;
        var xPos = -(((tiles.Count - 1) * ((tileSize) + padding)) / 2);
        var scale = new Vector3(tileScale, tileScale, tileScale);

        if (popInTile)
            popInTile.PopIn(scale);

        foreach (var tile in tiles)
        {
            Tile tile1 = tile;

            tile.transform.localScale = scale;
            tile.AnimateToPosition(new Vector2(xPos, 0), .3f, () => tile1.moveTospellerAnimationCompleted = true, !tile.moveTospellerAnimationCompleted);

            if (popInTile && tile != popInTile)
            {
                tile.CancelPopIn();
            }

            xPos += (tileSize + padding);
        }
    }

    private void CheckSubmitButtons()
    {
        if (tiles.Count > 2 && !GameGui.Instance.submitWordButton.interactable && ShouldShowWordButtons())
        {
            GameGui.Instance.submitWordButton.interactable = true;
        }
        else if ((tiles.Count <= 2 || !ShouldShowWordButtons()) && GameGui.Instance.submitWordButton.interactable)
        {
            GameGui.Instance.submitWordButton.interactable = false;
        }

        if (tiles.Count > 0 && !GameGui.Instance.clearWordButton.interactable && ShouldShowWordButtons())
        {
            GameGui.Instance.clearWordButton.interactable = true;
        }
        else if ((tiles.Count == 0 || !ShouldShowWordButtons()) && GameGui.Instance.clearWordButton.interactable)
        {
            GameGui.Instance.clearWordButton.interactable = false;
        }
    }

    private bool ShouldShowWordButtons()
    {
        if (!GameScene.Instance.IsGameActive())
            return false;

        return true;
    }
    
    public void GlowLetters(Action flashCompletedAction)
    {
        StartCoroutine(DoGlowLetters(flashCompletedAction));
    }

    private IEnumerator DoGlowLetters(Action flashCompletedAction)
    {
        var delay = 0.1f - (tiles.Count / 200f);
        if (delay < 0.02f)
            delay = 0.02f;

        foreach (var tile in tiles)
        {
            tile.PulseGlow();
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(.2f);

        if(flashCompletedAction != null)
            flashCompletedAction();
    }

    public void RemoveTile(Tile originalTile)
    {
        var tile = GetReferencedTile(originalTile.ID);

        if(tile)
            DoTileRemove(tile);
    }

    public void DoTileRemove(Tile tile)
    {
        tiles.Remove(tile);
        tile.PopOut();

        RepositionLetters();

        onSpellerChangedEvent.Invoke();
    }

    private Tile GetReferencedTile(int originalTileId)
    {
        return tiles.FirstOrDefault(t => t.referencedTileID == originalTileId);
    }


    public string CurrentWord()
    {
        return string.Join("", tiles.Select(t => t.letterDefinition.letter).ToArray());
    }

    public void ClearTiles()
    {
        foreach (var tile in tiles)
        {
            tile.PopOut();
        }

        tiles.Clear();

        onSpellerChangedEvent.Invoke();
    }

    public void OnReset()
    {
        ClearTiles();
    }

    public void OnInvalidWord(bool isDuplicate)
    {
        if (isDuplicate)
        {
            foreach (var invalidTween in invalidTweensDupe)
            {
                invalidTween.Rewind();
                invalidTween.Play();
            }
        }
        else
        {
            foreach (var invalidTween in invalidTweens)
            {
                invalidTween.Rewind();
                invalidTween.Play();
            }
        }
    }
}
