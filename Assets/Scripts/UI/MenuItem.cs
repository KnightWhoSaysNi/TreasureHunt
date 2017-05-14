using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    public GameObject removeButton;
    public GameObject checkMark;
    public Text text;
    
    public MenuItemType MenuItemType { get; set; }

    public TreasureHunt.TreasureHunt TreasureHunt { get; set; }
    public TreasureHunt.Problem Problem { get; set; }
    public TreasureHunt.Task Task { get; set; }
}

public enum MenuItemType { TreasureHunt, Problem, Task}

