using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable] 
    public class TreasureHunt : TitledObject
    {
        public TreasureHunt(string title)
        {
            this.Title = title;
            Problems = new List<Problem>();
        }

        public List<Problem> Problems { get; set; }
        public bool IsCompleted
        {
            get
            {
                foreach (Problem problem in Problems)
                {
                    if (!problem.IsSolved)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public float PercentComplete { get; set; }
        public Task LastPlayedTask { get; set; }
        public int HintPointsAvailable { get; set; }
    }
}

