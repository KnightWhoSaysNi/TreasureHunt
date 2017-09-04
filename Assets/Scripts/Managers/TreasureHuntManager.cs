using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TreasureHunt;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class TreasureHuntManager : MonoBehaviour
{
    #region - Fields -

    private bool areTreasureHuntsLoaded;
    private bool isTreasureHuntSaved;    

    #endregion

    #region - Events -    

    public event Action TreasureHuntsLoaded;
    public event Action TreasureHuntSaveStarted;
    public event Action TreasureHuntSaved;

    public event Action TreasureHuntCreated;
    public event Action ProblemCreated;
    public event Action TaskCreated;
    public event Action HintCreated;
    public event Action HintRemoved;
    public event Action HintRevealed;

    public event Action TaskSolved;

    #endregion

    #region - Properties -
        
    public TreasureHunt.TreasureHunt CurrentTreasureHunt { get; set; }
    public Problem CurrentProblem { get; set; }
    public Task CurrentTask { get; set; }
    public Hint CurrentHint { get; set; }
    
    public List<TreasureHunt.TreasureHunt> AllTreasureHunts { get; private set; }

    #endregion

    #region - Public Methods -    

    #region - Create Treasure Hunt/Problem/Task/Hint -

    /// <summary>
    /// Creates a new, unnamed treasure hunt, and saves it.
    /// </summary>
    public void CreateTreasureHunt()
    {
        // TODO make better naming system for treasure hunts, problems and tasks
        TreasureHunt.TreasureHunt newTreasureHunt = new TreasureHunt.TreasureHunt("Unnamed Treasure Hunt " + (AllTreasureHunts.Count + 1)); 
        AllTreasureHunts.Add(newTreasureHunt);
        CurrentTreasureHunt = newTreasureHunt;        

        if (TreasureHuntCreated != null)
        {
            TreasureHuntCreated();
        }

        SaveTreasureHunt();
    }

    /// <summary>
    /// Creates a new problem and saves the treasure hunt.
    /// </summary>
    public void CreateProblem()
    {
        Problem newProblem = new Problem("Problem " + (CurrentTreasureHunt.Problems.Count + 1)); // TODO 
        CurrentTreasureHunt.Problems.Add(newProblem);
        CurrentProblem = newProblem;

        if (ProblemCreated != null)
        {
            ProblemCreated();
        }

        SaveTreasureHunt();
    }

    /// <summary>
    /// Creates a new task and saves the treasure hunt.
    /// </summary>
    public void CreateTask()
    {
        Task newTask = new Task("Task " + (CurrentProblem.Tasks.Count + 1)); // TODO 
        CurrentProblem.Tasks.Add(newTask);
        CurrentTask = newTask;

        if (TaskCreated != null)
        {
            TaskCreated();
        }

        SaveTreasureHunt();
    }

    /// <summary>
    /// Creates a new hint and saves it.
    /// </summary>
    public void CreateHint()
    {
        Hint newHint = new Hint();
        CurrentTask.AllHints.Add(newHint);
        CurrentTask.UnrevealedHints.Add(newHint);
        CurrentHint = newHint;

        if (HintCreated != null)
        {
            HintCreated();
        }

        SaveTreasureHunt();
    }

    #endregion

    #region - Remove TreasureHunt/Problem/Task/Hint -
    
    public void RemoveTreasureHunt(TreasureHunt.TreasureHunt treasureHunt)
    {
        AllTreasureHunts.Remove(treasureHunt);
        PersistenceService.Instance.RemoveTreasureHunt(treasureHunt);
    } 

    public void RemoveProblem(Problem problem)
    {
        CurrentTreasureHunt.Problems.Remove(problem);
        SaveTreasureHunt();
    }

    public void RemoveTask(Task task)
    {
        CurrentProblem.Tasks.Remove(task);
        SaveTreasureHunt();
    }

    /// <summary>
    /// Removes the current hint and raises the HintRemoved event.
    public void RemoveHint()
    {
        SilentlyRemoveHint(CurrentHint);        

        if (HintRemoved != null)
        {
            HintRemoved();
        }
    }

    /// <summary>
    /// Removes the specified hint from the current task without raising any events. 
    /// Saves treasure hunt after removing the hint.
    /// </summary>
    /// <param name="hint"></param>
    public void SilentlyRemoveHint(Hint hint)
    {
        CurrentTask.AllHints.Remove(hint);
        CurrentTask.RevealedHints.Remove(hint);
        CurrentTask.UnrevealedHints.Remove(hint);

        CurrentHint = null;

        SaveTreasureHunt();
    }

    #endregion

    #region - Change Hint Points -

    public void ChangeTreasureHuntHintPoints(int newHintPointValue)
    {
        if (CurrentTreasureHunt.StartingHintPoints != newHintPointValue)
        {
            CurrentTreasureHunt.StartingHintPoints = newHintPointValue;
            SaveTreasureHunt();
        }
    }

    public void ChangeProblemHintPoints(int newHintPointValue)
    {
        if (CurrentProblem.HintPoints != newHintPointValue)
        {
            CurrentProblem.HintPoints = newHintPointValue;
            SaveTreasureHunt();
        }
    }

    #endregion

    /// <summary>
    /// Saves the current treasure hunt. If
    /// </summary>
    /// <param name="oldTitle"></param>
    public void SaveTreasureHunt(string oldTitle = null)
    {
        if (TreasureHuntSaveStarted != null)
        {
            TreasureHuntSaveStarted();
        }

        PersistenceService.Instance.SaveTreasureHunt(CurrentTreasureHunt, oldTitle);
    }

    /// <summary>
    /// Checks if all treasure hunts contain a treasure hunt with the specified title.
    /// </summary>
    public bool ContainsTreasureHuntTitle(string title)
    {
        foreach (var treasureHunt in AllTreasureHunts)
        {
            if (treasureHunt.Title == title)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Compares the given string to the text solution of the current task. Ignores white spaces and case.
    /// </summary>
    public void CheckTextSolution(string possibleSolution)
    {
        if (CurrentTask.Solution.TextSolution.Trim().ToLower() == possibleSolution.Trim().ToLower())
        {
            CurrentTask.IsSolved = true;
            SaveTreasureHunt();

            if (TaskSolved != null)
            {
                TaskSolved();
            }
        }
    }

    public void SolveTask()
    {
        CurrentTask.IsSolved = true;

        SaveTreasureHunt();
    }

    // This MUST be called before HintOptions.RevealHint in order to work correctly
    public void RevealHint()
    {
        // This will only be called if there are some Unrevealed hints and there are Hint points available
        Hint hint = CurrentTask.UnrevealedHints[0];
        CurrentTask.UnrevealedHints.RemoveAt(0);
        CurrentTask.RevealedHints.Add(hint);
        CurrentHint = hint;
        CurrentTreasureHunt.UsedHintPoints++;

        SaveTreasureHunt();
        
        if (HintRevealed != null)
        {
            HintRevealed();
        }
    }

    #endregion

    #region - Private Methods -

    private void Update()
    {
        if (isTreasureHuntSaved)
        {
            if (TreasureHuntSaved != null)
            {
                TreasureHuntSaved();
            }

            isTreasureHuntSaved = false;
        }

        if (areTreasureHuntsLoaded)
        {
            if (TreasureHuntsLoaded != null)
            {
                TreasureHuntsLoaded();
            }

            areTreasureHuntsLoaded = false;
        }
    }

    private void Start()
    {
        PersistenceService.Instance.LoadTreasureHunts();       
        PersistenceService.Instance.Loaded += OnLoaded;
        PersistenceService.Instance.Saved += OnSaved;
    }

    private void OnLoaded(List<TreasureHunt.TreasureHunt> obj)
    {
        AllTreasureHunts = obj;
        areTreasureHuntsLoaded = true;
    }

    private void OnSaved() // Not running on a main thread
    {
        isTreasureHuntSaved = true;
    }

    #endregion
}

