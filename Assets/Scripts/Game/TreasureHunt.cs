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
        public float PercentComplete
        {
            get
            {
                int completedProblems = 0;
                foreach (Problem problem in Problems)
                {
                    if (problem.IsSolved)
                    {
                        completedProblems++;
                    }
                    else
                    {
                        // unsolved problem reached
                        break;
                    }
                }

                return completedProblems / Problems.Count;
            }
        }
        public int HintPointsAvailable
        {
            get
            {
                int hintPoints = StartingHintPoints;
                foreach (var problem in Problems)
                {
                    if (problem.IsSolved)
                    {
                        hintPoints += problem.HintPoints;
                    }
                }
                return hintPoints;
            }
            set { }
        }
        public int StartingHintPoints { get; set; }
    }
}

