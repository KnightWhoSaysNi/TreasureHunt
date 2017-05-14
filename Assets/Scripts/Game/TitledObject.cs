using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    public class TitledObject : ITitle
    {
        public string Title { get; set; }

        public void ChangeTitle(string newTitle)
        {
            Title = newTitle;
        }
    }
}
