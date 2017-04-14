using System;

namespace TreasureHunt.UI
{
    [Serializable]
    public struct ButtonInfo
    {
        public string header;
        public Menu menu;

        [UnityEngine.Tooltip("This is used only for buttons that are inside a TreasureHunt game to indicate if the particular game/problem/task has been solved")]
        public bool isCheckmarkRequired;

        public ButtonInfo(string header, Menu menu, bool isCheckmarkRequired = false)
        {
            this.header = header;
            this.menu = menu;
            this.isCheckmarkRequired = isCheckmarkRequired;
        }
    }
}
