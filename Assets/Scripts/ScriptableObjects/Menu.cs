using System.Collections.Generic;
using UnityEngine;
using TreasureHunt.UI;

[CreateAssetMenu(fileName = "Menu", menuName = "Create new Menu")]
public class Menu : ScriptableObject
{
    public List<ButtonInfo> buttons;    
}
