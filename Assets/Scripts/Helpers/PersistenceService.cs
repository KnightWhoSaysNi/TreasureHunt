using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TreasureHunt;
using UnityEngine;

public sealed class PersistenceService 
{
    private static readonly object lockObject;
    private static PersistenceService instance;

    private static string persistentDataPath;
    private string oldTitle;
    private BackgroundWorker saveWorker;
    private BackgroundWorker loadWorker;
    
    private PersistenceService()
    {   
        saveWorker = new BackgroundWorker();
        saveWorker.DoWork += SaveWorkerDoWork; // Runs on a separate thread, not the main UI thread
        saveWorker.RunWorkerCompleted += OnSaveWorkerCompleted; // Also runs on a separate (third) thread

        loadWorker = new BackgroundWorker();
        loadWorker.DoWork += LoadWorkerDoWork;
        loadWorker.RunWorkerCompleted += OnLoadWorkerCompleted;
    }

    static PersistenceService()
    {
        persistentDataPath = Constants.persistentDataPath;
        lockObject = new object();
    }

    public event Action Saved;
    public event Action<List<TreasureHunt.TreasureHunt>> Loaded;

    public static PersistenceService Instance
    {
        get
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = new PersistenceService();
                }
                return instance;
            }
        }
    }        

    // TODO catch exceptions
    public void SaveTreasureHunt(TreasureHunt.TreasureHunt treasureHunt, string oldTitle = null) // Main thread // TODO remove setting path as an argument
    {
        this.oldTitle = oldTitle;
        saveWorker.RunWorkerAsync(treasureHunt);                     
    }

    // TODO catch exceptions
    public void LoadTreasureHunts() // Main thread // TODO remove setting path as an argument
    {
        MonoBehaviour.print("LoadTreasureHunts on: " + System.Threading.Thread.CurrentThread.ManagedThreadId);       
        loadWorker.RunWorkerAsync();
    }
    
    private void SaveWorkerDoWork(object sender, DoWorkEventArgs e) // IS in fact on a separate thread
    {
        TreasureHunt.TreasureHunt treasureHunt = e.Argument as TreasureHunt.TreasureHunt;
        if (treasureHunt == null)
        {
            throw new ArgumentException("You must use a TreasureHunt as argument for DoWorkEventArgs");
        }

        string filePath = persistentDataPath + "/" + treasureHunt.Title + Constants.extension;
        string oldFilePath = persistentDataPath + "/" + oldTitle + Constants.extension;

        // Normal save
        SaveFile(treasureHunt, filePath);

        if (oldTitle != null && File.Exists(oldFilePath))
        {
            // Renaming a file
            File.Delete(oldFilePath);

            // Necessary for the next method call
            oldTitle = null;
        }
    }

    // Catch exceptions
    private void SaveFile(TreasureHunt.TreasureHunt treasureHunt, string filePath)
    {
        using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
        {
            BinaryFormatter binFormatter = new BinaryFormatter();
            binFormatter.Serialize(stream, treasureHunt);
        }
    }

    private void OnSaveWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) // NOT ON THE MAIN THREAD even though worker was created from the MAIN THREAD!
    {
        if (e.Cancelled)
        {
            // Player cannot cancel at the moment. If changed this part becomes possible
        }
        else if (e.Error != null)
        {
            // TODO do something if there was an error
            MonoBehaviour.print(e.Error.Message);
        }
        else
        {
            // no problems saving
            if (Saved != null)
            {
                Saved();
            }
        }
    }

    // TODO catch exceptions
    private void LoadWorkerDoWork(object sender, DoWorkEventArgs e) // Not on the main thread
    {
        List<TreasureHunt.TreasureHunt> treasureHunts = new List<TreasureHunt.TreasureHunt>();

        string searchPattern = "*.th";
        string[] allTreasureHuntsPaths = Directory.GetFiles(persistentDataPath, searchPattern);

        for (int i = 0; i < allTreasureHuntsPaths.Length; i++)
        {
            using (Stream stream = File.Open(allTreasureHuntsPaths[i], FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();

                TreasureHunt.TreasureHunt treasureHunt = binFormatter.Deserialize(stream) as TreasureHunt.TreasureHunt;
                if (treasureHunt != null)
                {
                    treasureHunts.Add(treasureHunt);
                }
            }
        }

        System.Timers.Timer timer = new System.Timers.Timer(1000); // THIS NEVER STOPS, EVEN IN EDITOR
        timer.Elapsed += Timer_Elapsed;
        timer.Start();
        while (timerTime > 0)
        {
            // do nothing
            MonoBehaviour.print("waiting for the timer");
        }
        

        e.Result = treasureHunts;
    }

    int timerTime = 5;
    private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (timerTime > 0)
        {
            timerTime--;
        }
        else
        {
            ((System.Timers.Timer)sender).Stop(); // This should stop the above timer
        }
    }

    private void OnLoadWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) // Not on main thread, but not on the same thread as the worker
    {
        List<TreasureHunt.TreasureHunt> treasureHunts;
        if (e.Error != null)
        {
            // TODO do something if there was an error
            MonoBehaviour.print(e.Error.Message);
            treasureHunts = new List<TreasureHunt.TreasureHunt>();
        }
        else
        {
            // no problems loading
            treasureHunts = (List<TreasureHunt.TreasureHunt>)e.Result;
        }

        if (Loaded != null)
        {
            Loaded(treasureHunts);
        }
    }
}

