using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TreasureHunt;

public class GameManager : MonoBehaviour
{
    private List<TreasureHunt.TreasureHunt> allTreasureHunts;
    private TreasureHunt.TreasureHunt currentTreasureHunt;
    private Problem currentProblem;
    private Task currentTask;

    public event Action TaskActivated;
    public event Action TaskDeactivated;    

    public GameMode GameMode { get; private set; }

    #region - Create Treasure Hunt/Problem/Task -
    public void CreateTreasureHunt()
    {
        TreasureHunt.TreasureHunt newTreasureHunt = new TreasureHunt.TreasureHunt("Unnamed Treasure Hunt " + (allTreasureHunts.Count + 1));
        allTreasureHunts.Add(newTreasureHunt);
        currentTreasureHunt = newTreasureHunt;
        print("New Treasure Hunt created.");
        foreach (var th in allTreasureHunts)
        {
            print(th.Title);
        }
    }

    public void CreateProblem()
    {
        Problem newProblem = new Problem("Problem " + (currentTreasureHunt.Problems.Count + 1));
        currentProblem = newProblem;
    }

    public void CreateTask()
    {
        Task newTask = new Task(currentProblem, "Task " + (currentProblem.Tasks.Count + 1)); // TODO Task can be removed and a duplicate name can occur
        currentTask = newTask;
    }
    #endregion

    #region - Change Title -
    public void ChangeTreasureHuntTitle(string newTitle)
    {
        currentTreasureHunt.Title = newTitle;
    }

    public void ChangeProblemTitle(string newTitle)
    {
        currentProblem.Title = newTitle;
    }

    public void ChangeTaskTitle(string newTitle)
    {
        currentTask.Title = newTitle;
    }
    #endregion

    #region - Change Treasure Hunt/Problem/Task -
    public void ChangeTreasureHunt(TreasureHunt.TreasureHunt newTreasureHunt)
    {
        currentTreasureHunt = newTreasureHunt;
    }

    public void ChangeProblem(Problem newProblem)
    {
        currentProblem = newProblem;
    }

    public void ChangeTask(Task newTask)
    {
        currentTask = newTask;
    }
    #endregion

    private void Start()
    {
        GameMode = GameMode.PlayMode;

        allTreasureHunts = new List<TreasureHunt.TreasureHunt>();
        // TODO deserialize treasure hunts and add them to allTresaureHunts
    }

    private void OpenProblem()
    {
        if (TaskActivated != null)
        {
            TaskActivated();
        }
    }

    private void CloseProblem()
    {
        if (TaskDeactivated != null)
        {
            TaskDeactivated();
        }
    }
}

public enum GameMode { PlayMode, CreationMode };