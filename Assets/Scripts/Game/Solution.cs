using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Solution
    {
        public Solution()
        {
            TextSolution = string.Empty;
        }

        public Solution(string textSolution)
        {
            this.TextSolution = textSolution;
        }

        public Solution(string textSolution, Location locationSolution) : this(textSolution)
        {
            HasLocationSolution = true;
            this.LocationSolution = locationSolution;
        }

        public string TextSolution { get; set; }
        public bool HasLocationSolution { get; set; }
        public Location LocationSolution { get; set; } // TODO maybe guard against a call to this when HasLocationSolution == false
    }
}
