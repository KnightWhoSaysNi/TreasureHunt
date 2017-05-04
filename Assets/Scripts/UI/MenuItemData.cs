using System;

[Serializable]
public struct MenuItemData
{
    public string header;
    public Menu menu;

    [UnityEngine.Tooltip("This is used only for buttons that are inside a TreasureHunt game to indicate if the particular game/problem/task has been solved.")]
    public bool isCheckMarkRequired;
    [UnityEngine.Tooltip("Problems and tasks require remove buttons in creation/edit mode.")]
    public bool isRemoveRequired;

    public MenuItemData(string header, Menu menu, bool isCheckMarkRequired = false, bool isRemoveRequired = false)
    {
        this.header = header;
        this.menu = menu;
        this.isCheckMarkRequired = isCheckMarkRequired;
        this.isRemoveRequired = isRemoveRequired;
    }
}

