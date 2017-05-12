using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TreasureHunt;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameManager : MonoBehaviour
{
    private GameMode gameMode;

    public event Action TreasureHuntCreated;
    public event Action ProblemCreated;
    public event Action TaskCreated;
    public event Action GameModeChanged;

    public TreasureHunt.TreasureHunt CurrentTreasureHunt { get; set; }
    public Problem CurrentProblem { get; set; }
    public Task CurrentTask { get; set; }
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

    public void GoToPlayMode()
    {
        GameMode = GameMode.PlayMode;
    }

    public void GoToCreationMode()
    {
        GameMode = GameMode.CreationMode;
    }
    
    #region - Create Treasure Hunt/Problem/Task -

    public void CreateTreasureHunt()
    {
        TreasureHunt.TreasureHunt newTreasureHunt = new TreasureHunt.TreasureHunt("Unnamed Treasure Hunt " + (AllTreasureHunts.Count + 1)); // TODO 
        AllTreasureHunts.Add(newTreasureHunt);
        CurrentTreasureHunt = newTreasureHunt;

        if (TreasureHuntCreated != null)
        {
            TreasureHuntCreated();
        }
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
    }

    #endregion

    #region - Change Title -

    public void ChangeTreasureHuntTitle(string newTitle)
    {
        CurrentTreasureHunt.Title = newTitle;
    }

    public void ChangeProblemTitle(string newTitle)
    {
        CurrentProblem.Title = newTitle;
    }

    public void ChangeTaskTitle(string newTitle)
    {
        CurrentTask.Title = newTitle;
    }

    #endregion    

    private void Start()
    {
        GameMode = GameMode.PlayMode;

        AllTreasureHunts = new List<TreasureHunt.TreasureHunt>();
        // TODO deserialize treasure hunts and add them to allTresaureHunts

        TreasureHunt.TreasureHunt testTreasureHunt = new TreasureHunt.TreasureHunt("Test Treasure Hunt");
        Problem testProblem1 = new Problem("Test Problem 1", testTreasureHunt);
        Problem testProblem2 = new Problem("Test Problem 2", testTreasureHunt);
        testProblem2.IsSolved = true;
        Problem testProblem3 = new Problem("Test Problem 3", testTreasureHunt);
        Task testTask11 = new Task("Test Task 11");
        testTask11.TextClue = "If I was to say \"Ni\", what would your gift to me be?";
        testTask11.Solution.TextSolution = "A shrubbery";

        Task testTask12 = new Task("Test Task 12");
        Task testTask21 = new Task("Test Task 21");
        testTask21.IsSolved = true;
        testTask21.TextClue = "Name a famous elf from the Tolkien universe";
        testTask21.Solution.TextSolution = "Glorfindel";

        Task testTask31 = new Task("Test Task 31");
        Task testTask32 = new Task("Test Task 32");
        Task testTask33 = new Task("Test Task 33");
        Task testTask34 = new Task("Test Task 34");
        testTask34.IsSolved = true;
        testTask34.Solution.TextSolution = "Marry Wattson";

        Task testTask35 = new Task("Test Task 35");
        testProblem1.Tasks.Add(testTask11);
        testProblem1.Tasks.Add(testTask12);
        testProblem2.Tasks.Add(testTask21);
        testProblem3.Tasks.Add(testTask31);
        testProblem3.Tasks.Add(testTask32);
        testProblem3.Tasks.Add(testTask33);
        testProblem3.Tasks.Add(testTask34);
        testProblem3.Tasks.Add(testTask35);
        //testTreasureHunt.Problems.Add(testProblem1);
        //testTreasureHunt.Problems.Add(testProblem2);
        //testTreasureHunt.Problems.Add(testProblem3);


        AllTreasureHunts.Add(testTreasureHunt);

        //IFormatter formatter = new BinaryFormatter();
        //Stream stream = new FileStream("My Test Treasure Hunt.bin", FileMode.Create, FileAccess.Write);
        //formatter.Serialize(stream, testTreasureHunt);
        //stream.Close();

        //Stream newStream = new FileStream("My Test Treasure Hunt.bin", FileMode.Open, FileAccess.Read);
        //TreasureHunt.TreasureHunt deserializedTreasureHunt = (TreasureHunt.TreasureHunt)formatter.Deserialize(newStream);
        //AllTreasureHunts.Add(deserializedTreasureHunt);
        //newStream.Close();

        //File.Move("My Test Treasure Hunt.bin", "Renamed Test Treasure Hunt.bin");
    }    
}

public enum GameMode { PlayMode, CreationMode };