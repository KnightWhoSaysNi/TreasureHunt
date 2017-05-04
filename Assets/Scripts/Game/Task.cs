using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    public class Task
    {
        public Task(Problem problem, string title)
        {
            this.Problem = problem;
            this.Title = title;
            Hints = new List<Hint>();
        }

        public Task(Problem problem, string title, string textClue):this(problem, title)
        {
            this.TextClue = textClue;
        }

        public Problem Problem { get; private set; }
        public string Title { get; set; }
        public string TextClue { get; set; }
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
