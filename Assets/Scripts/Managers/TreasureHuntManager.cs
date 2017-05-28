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
    private bool areTreasureHuntsLoaded;
    private bool isTreasureHuntSaved;
    private GameMode gameMode;

    public event Action GameModeChanged;

    public event Action TreasureHuntsLoaded;
    public event Action TreasureHuntSaveStarted;
    public event Action TreasureHuntSaved;

    public event Action TreasureHuntCreated;
    public event Action ProblemCreated;
    public event Action TaskCreated;
    public event Action HintCreated;
    public event Action HintRemoved;

    public TreasureHunt.TreasureHunt CurrentTreasureHunt { get; set; }
    public Problem CurrentProblem { get; set; }
    public Task CurrentTask { get; set; }
    public Hint CurrentHint { get; set; }

    public GameMode GameMode
    {
        get
        {
            return gameMode;
        }
        set
        {
            if (gameMode != value)
            {
                gameMode = value;

                if (GameModeChanged != null)
                {
                    GameModeChanged();
                }
            }
        }
    }
    public List<TreasureHunt.TreasureHunt> AllTreasureHunts { get; private set; }

    #region - Switch Game Modes - 

    public void GoToPlayMode()
    {
        GameMode = GameMode.PlayMode;
    }

    public void GoToCreationMode()
    {
        GameMode = GameMode.CreationMode;
    }

    #endregion

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

    public bool IsSolutionCorrect(string textSolution)
    {
        if (CurrentTask.Solution.TextSolution.Trim().ToLower() == textSolution.Trim().ToLower())
        {
            return true;
        }

        return false;
    }

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
        GameMode = GameMode.PlayMode;

        PersistenceService.Instance.Saved += OnSaved;

        PersistenceService.Instance.LoadTreasureHunts();       

        PersistenceService.Instance.Loaded += Instance_Loaded;

        //File.Move("My Test Treasure Hunt.bin", "Renamed Test Treasure Hunt.bin");
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
}

public enum GameMode { PlayMode, CreationMode };