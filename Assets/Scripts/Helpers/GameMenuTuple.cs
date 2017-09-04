using System;
using System.Collections;

/// <summary>
/// A class used in game manager's game menu stack which holds information about the last game menu 
/// that the player visited.
/// </summary>
public class GameMenuTuple
{
    public string Title { get; set; }
    public IEnumerable ListOfItems { get; set; }
    public GameItemType GameItemType { get; set; }
}
