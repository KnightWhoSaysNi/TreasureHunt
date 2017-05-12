using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]    
    public class Task : ITitle
    {
        public Task(string title)
        {
            this.Title = title;
            Hints = new List<Hint>();
            Solution = new Solution();
            TextClue = string.Empty;
        }

        public Task(string title, Problem problem) : this(title)
        {
            this.Problem = problem;
        }

        public Task(string title, Problem problem, string textClue) : this(title, problem)
        {
            this.TextClue = textClue;
        }

        public Problem Problem { get; private set; }
        public string Title { get; set; }
        public string TextClue { get; set; }
        public Solution Solution { get; set; }
        public bool IsSolved { get; set; }
        public List<Hint> Hints { get; set; }
        public bool HasHints
        {
            get
            {
                return Hints.Count > 0;
            }
        }
    }
}
