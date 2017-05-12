using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable] 
    public class TreasureHunt : ITitle
    {
        public TreasureHunt(string title)
        {
            this.Title = title;
            Problems = new List<Problem>();
        }

        public List<Problem> Problems { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public float PercentComplete { get; set; }
        public Task LastPlayedTask { get; set; }
    }
}

