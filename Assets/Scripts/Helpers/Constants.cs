using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Constants
{
    public static readonly string persistentDataPath = Application.persistentDataPath;

    public const string Extension = ".th";
    public const string TreasureHuntStartingHintPoints = "Starting hint points:";
    public const string ProblemRewardHintPoints = "Reward hint points:";

    public const string TaskSolvedByText = "Correct !";
    public const string TaskSolvedByLocation = "You've reached the target destination ! Congratulations !";

    public const string LoadingMessage = "Loading...";
    public const string SavingMessage = "Saving...";

    public const int MinHintPoints = 0;
    public const int MaxHintPoints = 100;

    public const int DefaultLocationRadius = 100;

    public const string StartingLocationServiceMessage = "Starting location service...";
    public const string LocationServiceNotStarted = "Location service couldn't start.";
    public const string LocationServiceDisabledByUser = "Please enable Location.";

    public const int LocationServiceWaitTimeInSeconds = 20;
    public const float LocationServiceRunTimeInSeconds = 300;
    public const float LocationServiceUpdateTimeInSeconds = 1;
    public const float DesiredAccuracyInMeters = 1;
    public const float UpdateDistanceInMeters = 1;
}

