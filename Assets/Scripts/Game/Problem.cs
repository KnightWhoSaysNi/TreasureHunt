using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    public class Problem
    {

        public Problem(string title)
        {
            this.Title = title;
            Tasks = new Dictionary<string, Task>();
        }

        public Dictionary<string, Task> Tasks { get; set; }
        public string Title { get; set; }
        public bool IsSolved { get; set; }
    }
}
