using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Create new Menu")]
public class Menu : ScriptableObject
{
    public string header;
    public GameMenuType menuType = GameMenuType.TreasureHunt;
    public List<MenuItemData> menuItems;    

    public enum GameMenuType { TreasureHunt, Problem, Task};
}


