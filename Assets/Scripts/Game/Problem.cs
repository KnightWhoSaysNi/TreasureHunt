﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Problem : ITitle
    {
        public Problem(string title)
        {
            this.Title = title;
            Tasks = new List<Task>();
        }

        public Problem(string title, TreasureHunt treasureHunt) : this(title)
        {
            this.TreasureHunt = treasureHunt;
            treasureHunt.Problems.Add(this);
        }

        public List<Task> Tasks { get; set; }
        public TreasureHunt TreasureHunt { get; set; }
        public string Title { get; set; }
        public bool IsSolved { get; set; }
        public int HintPoints { get; set; }
    }
}
