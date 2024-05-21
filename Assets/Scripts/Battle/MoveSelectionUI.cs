using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<Text> moveTexts;
    [SerializeField] Text moveDescription;
    [SerializeField] Text movePower;
    [SerializeField] Text moveAccuracy;

    int currentSelection = 0;
    Color highlightedColor;
    List<int> movesAccuracy = new List<int>();
    List<int> movesPower = new List<int>();
    List<string> movesDescription = new List<string>();


    private void Start () {
        highlightedColor = GlobalSettings.Instance.HighlightedColor;
    }

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {

        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveTexts[i].text = currentMoves[i].Name;
            movesPower.Add(currentMoves[i].Power);
            movesAccuracy.Add(currentMoves[i].Accuracy);
            movesDescription.Add(currentMoves[i].Description);
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
        movesPower.Add(newMove.Power);
        movesAccuracy.Add(newMove.Accuracy);
        movesDescription.Add(newMove.Description);
    }

    public void HandleMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
            if (currentSelection > PokemonBase.MaxNumMoves)
                currentSelection = 0;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = PokemonBase.MaxNumMoves;
        }

        currentSelection %= PokemonBase.MaxNumMoves + 1;

        UpdateMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateMoveSelection(int currentSelection)
    {

        for (int i = 0; i < PokemonBase.MaxNumMoves + 1; i++)
        {

            if (i == currentSelection)
            {
                moveTexts[i].color = highlightedColor;
                moveDescription.text = movesDescription[currentSelection];
                movePower.text = "POWER " + movesPower[currentSelection];
                moveAccuracy.text = "ACCURACY " + movesAccuracy[currentSelection];
            }
            else
                moveTexts[i].color = Color.black;
        }
    }
}
