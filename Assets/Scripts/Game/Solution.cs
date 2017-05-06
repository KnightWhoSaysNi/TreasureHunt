using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Solution
    {
        public Solution(string textSolution, bool hasLocationSolution = false)
        {
            this.TextSolution = textSolution;
            this.HasLocationSolution = hasLocationSolution;
        }

        public Solution(string textSolution, Location locationSolution) : this(textSolution, true)
        {
            this.LocationSolution = locationSolution;
        }

        public string TextSolution { get; set; }
        public bool HasLocationSolution { get; set; }
        public Location LocationSolution { get; set; }
    }
}
