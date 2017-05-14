using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    public interface ITitle
    {
        string Title { get; set; }

        void ChangeTitle(string newTitle);
    }
}
