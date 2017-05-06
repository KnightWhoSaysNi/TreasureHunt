using System;
using System.Collections.Generic;
using UnityEngine;

public class MenuItem : MonoBehaviour
{
    public MenuItemType menuItemType;

    public TreasureHunt.TreasureHunt TreasureHunt { get; set; }
    public TreasureHunt.Problem Problem { get; set; }
    public TreasureHunt.Task Task { get; set; }
}

public enum MenuItemType { TreasureHunt, Problem, Task, Addition}

