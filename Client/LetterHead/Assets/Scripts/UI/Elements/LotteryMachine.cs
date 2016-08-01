using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LetterHeadShared;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LotteryMachine : MonoBehaviour
{
    public GameObject rowPrefab;
    public RectTransform maskParent;
    public AnimationCurve spinCurve;
    public float spinTime;
    public float[] odds;
    public Action<Powerup.Type> onWinAction;
    public AudioSource beepAudioSource;

    public float rowSpacing = 100;
    public float spinSpeed = 1;
    private float spinTimeRemaining;
    private int lastRowIndex;

    private float heightExtent;
    private List<LotteryRow> rows = new List<LotteryRow>();

    public enum State
    {
        Idle, Spinning, Finished
    }

    public State state;
    public Sprite[] powerupSprites;

    // Use this for initialization
	void Start () {

        var maskHeight = maskParent.GetHeight();
	    heightExtent = (maskHeight) + rowSpacing * 2;

        Initilize(); //temp
	}

    public void Initilize()
    {
        state = State.Idle;
        maskParent.DeleteChildren();
        rows.Clear();

        var initialElementCount = Mathf.CeilToInt(heightExtent/rowSpacing);

        lastRowIndex = initialElementCount - 1;

        for (int i = 0; i < initialElementCount; i++)
        {
            var row = AddRow(i, Vector3.zero);

            if (i > 1)
                row.bloopPlayed = true;
        }
    }

    private LotteryRow AddRow(int verticalPosition, Vector3 offset)
    {
        var rowGo = Instantiate(rowPrefab);
        rowGo.transform.SetParent(maskParent);
        rowGo.transform.ResetToOrigin();
        rowGo.name = verticalPosition.ToString();
        
        var row = rowGo.GetComponent<LotteryRow>();
        InitilizeRow(row, verticalPosition, offset);

        rows.Add(row);

        return row;
    }

    private void InitilizeRow(LotteryRow row, int verticalPosition, Vector3 offset)
    {
        row.transform.localPosition = CalculatePositionForRow(verticalPosition) + offset;
        row.bloopPlayed = false;

        var booster = GetRandomPowerup();
        row.image.sprite = powerupSprites[(int) booster];
        row.powerupType = booster;
        //row.text.color = SkinDefinition.RarityColor(booster);
    }

    private Powerup.Type GetRandomPowerup()
    {
        return (Powerup.Type) Random.Range(0, 4);
    }


    private int GetRowIndexFromPosition(Vector3 pos)
    {
        var zeroPos = CalculatePositionForRow(0);

        var offset = (zeroPos.y + (rowSpacing / 2)) - pos.y;

        return Mathf.FloorToInt(offset/rowSpacing);
    }

    private Vector3 CalculatePositionForRow(int index)
    {
        var pos = new Vector3(0, (-(heightExtent/2) + (heightExtent - (index * rowSpacing))) - (rowSpacing/2), 0);

        return pos;
    }

    public void Spin()
    {
        state = State.Spinning;
        spinTimeRemaining = spinTime;
    }

    void Update()
    {
        var middleRowindex = 1;

        if (state == State.Spinning)
        {
            bool finalSelection = false;

            spinTimeRemaining -= Time.deltaTime;
            var elapsedPct = 1 - (spinTimeRemaining/spinTime);
            if (elapsedPct > 0.93f)
            {
                finalSelection = true;
                elapsedPct = 0.93f;
            }

            var speed = spinCurve.Evaluate(elapsedPct) * spinSpeed;
                
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var row = rows[i];

                var newPos = row.transform.localPosition;
                newPos.y -= Time.deltaTime * speed;

                if (newPos.y <= 0 && !row.bloopPlayed)
                {
                    PlayBloop(row);
                }

                var rowIndex = GetRowIndexFromPosition(row.transform.localPosition);
                var offsetAmount = CalculatePositionForRow(rowIndex).y - newPos.y;

                if (rowIndex == middleRowindex)
                {
                    if (finalSelection && offsetAmount > -8 && offsetAmount < 10)
                    {
                        StopSpinning(row);
                    }
                }

                if (newPos.y < -((heightExtent / 2) + (rowSpacing / 2)))
                {
                    InitilizeRow(row, rowIndex - (lastRowIndex + 1), new Vector3(0, -offsetAmount, 0));
                }
                else
                {
                    row.transform.localPosition = newPos;
                }
            }
        }
    }

    private void PlayBloop(LotteryRow row)
    {
        if(SoundManager.Instance.Muted())
            return;

        row.bloopPlayed = true;
        beepAudioSource.PlayOneShot(beepAudioSource.clip);
    }

    private void StopSpinning(LotteryRow winningRow)
    {
        state = State.Finished;

        SoundManager.Instance.PlayClip("Lottery Finished");
        var winningPowerup = winningRow.powerupType;
        Debug.Log("Won powerup of " + winningPowerup);
        onWinAction(winningPowerup);
    }
}
