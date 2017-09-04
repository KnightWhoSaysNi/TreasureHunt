using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represented as a button with some text, a possible check mark and a remove button.
/// It has a game item type and can therefore hold a treasure hunt, problem or a task reference.
/// </summary>
public class GameItem : MonoBehaviour
{
    public GameObject removeButton;
    public GameObject checkMark;
    public Text text;
    
    public GameItemType GameItemType { get; set; }

    public TreasureHunt.TreasureHunt TreasureHunt { get; set; }
    public TreasureHunt.Problem Problem { get; set; }
    public TreasureHunt.Task Task { get; set; }
}