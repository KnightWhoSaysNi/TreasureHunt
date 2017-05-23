using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreasureHunt
{
    [Serializable]    
    public class Task : TitledObject
    {
        public Task(string title)
        {
            this.Title = title;

            AllHints = new List<Hint>();
            UnrevealedHints = new List<Hint>();
            RevealedHints = new List<Hint>();
            Solution = new Solution();
            TextClue = string.Empty; 
        }

        //public Task(string title, Problem problem) : this(title)
        //{
        //    this.Problem = problem;
        //}

        //public Task(string title, Problem problem, string textClue) : this(title, problem)
        //{
        //    this.TextClue = textClue;
        //}

        //public Problem Problem { get; private set; }
        public string TextClue { get; set; }
        public Solution Solution { get; set; }
        public bool IsSolved { get; set; }
        public List<Hint> AllHints { get; set; }
        public List<Hint> UnrevealedHints { get; private set; }
        public List<Hint> RevealedHints { get; private set; }
        public bool HasHints // TODO is this necessary
        {
            get
            {
                return AllHints.Count > 0;
            }
        }
    }
}
