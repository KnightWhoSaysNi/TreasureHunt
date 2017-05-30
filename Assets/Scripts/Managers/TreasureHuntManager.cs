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

    public void CreateTreasureHunt()
    {
        TreasureHunt.TreasureHunt newTreasureHunt = new TreasureHunt.TreasureHunt("Unnamed Treasure Hunt " + (AllTreasureHunts.Count + 1)); // TODO 
        AllTreasureHunts.Add(newTreasureHunt);
        CurrentTreasureHunt = newTreasureHunt;        

        if (TreasureHuntCreated != null)
        {
            TreasureHuntCreated();
        }

        SaveTreasureHunt();
    }

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

    public void CreateTask()
    {
        Task newTask = new Task("Task " + (CurrentProblem.Tasks.Count + 1)); // TODO Task can be removed and a duplicate name can occur
        CurrentProblem.Tasks.Add(newTask);
        CurrentTask = newTask;

        if (TaskCreated != null)
        {
            TaskCreated();
        }

        SaveTreasureHunt();
    }

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

    public void RemoveHint()
    {
        SilentlyRemoveHint(CurrentHint);        

        if (HintRemoved != null)
        {
            HintRemoved();
        }

        SaveTreasureHunt();
    }

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

    public void SaveTreasureHunt(string oldTitle = null)
    {
        if (TreasureHuntSaveStarted != null)
        {
            TreasureHuntSaveStarted();
        }

        PersistenceService.Instance.SaveTreasureHunt(CurrentTreasureHunt, oldTitle);
    }

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
        PersistenceService.Instance.Loaded += Instance_Loaded;
        PersistenceService.Instance.Saved += OnSaved;
    }

    private void Instance_Loaded(List<TreasureHunt.TreasureHunt> obj)
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

public enum GameMode { PlayMode, CreationMode };