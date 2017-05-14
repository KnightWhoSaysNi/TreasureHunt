using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Hint
    {
        public Hint()
        {
            Text = string.Empty;
        }

        public Hint(string hintText)
        {
            Text = hintText;
        }

        public string Text { get; set; }
    }
}
