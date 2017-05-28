using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]
    public class Problem : TitledObject
    {
        public Problem(string title)
        {
            this.Title = title;
            Tasks = new List<Task>();
        }

        public List<Task> Tasks { get; set; }
        public bool IsSolved
        {
            get
            {
                foreach (Task task in Tasks)
                {
                    if (!task.IsSolved)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        public int HintPoints { get; set; }
    }
}
