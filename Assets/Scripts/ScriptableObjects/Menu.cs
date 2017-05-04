using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Menu", menuName = "Create new Menu")]
public class Menu : ScriptableObject
{
    public string header;
    public MenuType menuType = MenuType.Regular;
    public List<MenuItemData> menuItems;    

    public enum MenuType { Regular, Problem, Task};
}


