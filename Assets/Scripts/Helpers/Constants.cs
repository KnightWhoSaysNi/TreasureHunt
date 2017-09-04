using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Constants
{
    public static readonly string persistentDataPath = Application.persistentDataPath;

    public const string AllTreasureHunts = "All Treasure Hunts";
    public const string UnnamedTreasureHunt = "Unnamed Treasure Hunt";
    public const string GameMenuName = "Game Menu";
    public const string MainMenuName = "Main Menu";
    public const string GameManagerTag = "Game Manager";

    public const string Extension = ".th";
    public const string TreasureHuntStartingHintPoints = "Starting hint points:";
    public const string ProblemRewardHintPoints = "Reward hint points:";
    public const string RemainingHintPointsMessageSingular = " hint point remaining";
    public const string RemainingHintPointsMessagePlural = " hint points remaining";
    public const string HintPointsRewardedFirstPart = "You got ";
    public const string HintPointsRewardedSecondPart = " hint point(s) as a reward";

    public const string TaskNotSavedGoingBack = "Your task is not yet saved. If you go back it will be deleted. Are you sure you want to go back?";
    public const string SameNameTreasureHunt = "A treasure hunt with the same name already exists.";
    public const string EmptyTaskClueSave = "You cannot save an empty task. Please write some clue or delete the task.";
    public const string EmptyAnswerSave = "You cannot save a task without an answer.";
    public const string EmptyHintSave = "You cannot save an empty hint. Please either add some text or remove the hint.";
    public const string TaskSolvedByText = "Correct !";
    public const string TaskSolvedByLocation = "You've reached the target destination ! Congratulations !";

    public const string LoadingMessage = "Loading...";
    public const string SavingMessage = "Saving...";

    public const int MinHintPoints = 0;
    public const int MaxHintPoints = 100;

    public const int DefaultLocationRadius = 100;

    public const string NoLocationSolution = "Task has no set location.";
    public const string StartingLocationService = "Starting location service...";
    public const string LocationServiceNotStarted = "Location service couldn't start.";
    public const string LocationServiceDisabledByUser = "Please enable Location.";

    public const int LocationServiceWaitTimeInSeconds = 20;
    public const float LocationServiceRunTimeInSeconds = 300;
    public const float LocationServiceUpdateTimeInSeconds = 1;
    public const float DesiredAccuracyInMeters = 1;
    public const float UpdateDistanceInMeters = 1;

    public const string EmptyPassword = "Password cannot be empty";    
}

