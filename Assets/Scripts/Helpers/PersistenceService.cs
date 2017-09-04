using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public sealed class PersistenceService 
{
    private static readonly object lockObject;
    private static PersistenceService instance;

    private static string persistentDataPath;
    private string oldTitle;
    private BackgroundWorker saveWorker;
    private BackgroundWorker loadWorker;
    private BackgroundWorker removeWorker;
    
    static PersistenceService()
    {
        persistentDataPath = Constants.persistentDataPath;
        lockObject = new object();
    }

    private PersistenceService()
    {   
        saveWorker = new BackgroundWorker();
        saveWorker.DoWork += SaveWorkerDoWork; // Runs on a separate thread, not the main UI thread
        saveWorker.RunWorkerCompleted += OnSaveWorkerCompleted; // Also runs on a separate (third) thread

        loadWorker = new BackgroundWorker();
        loadWorker.DoWork += LoadWorkerDoWork;
        loadWorker.RunWorkerCompleted += OnLoadWorkerCompleted;

        removeWorker = new BackgroundWorker();
        removeWorker.DoWork += RemoveWorkerDoWork;
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
        
    public void SaveTreasureHunt(TreasureHunt.TreasureHunt treasureHunt, string oldTitle = null) // Main thread
    {
        this.oldTitle = oldTitle;
        if (!saveWorker.IsBusy)
        {
            saveWorker.RunWorkerAsync(treasureHunt);                     
        }
    }
        
    public void LoadTreasureHunts() // Main thread // TODO remove setting path as an argument
    {
        loadWorker.RunWorkerAsync();
    }

    public void RemoveTreasureHunt(TreasureHunt.TreasureHunt treasureHunt)
    {
        if (!removeWorker.IsBusy)
        {
            removeWorker.RunWorkerAsync(treasureHunt);
        }
    }
        
    private void SaveWorkerDoWork(object sender, DoWorkEventArgs e) // IS in fact on a separate thread
    {
        TreasureHunt.TreasureHunt treasureHunt = e.Argument as TreasureHunt.TreasureHunt;
        if (treasureHunt == null)
        {
            throw new ArgumentException("You must use a TreasureHunt as argument for DoWorkEventArgs");
        }

        string filePath = persistentDataPath + "/" + treasureHunt.Title + Constants.Extension;
        string oldFilePath = persistentDataPath + "/" + oldTitle + Constants.Extension;       

        // Normal save
        SaveFile(treasureHunt, filePath);

        if (oldTitle != null && File.Exists(oldFilePath))
        {
            // Treasure Hunt was renamed and saved as a new file so the old one is deleted
            try
            {
                File.Delete(oldFilePath);
            }
            catch (UnauthorizedAccessException exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
            catch (IOException exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
            catch(Exception exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
            

            // Necessary for the next method call
            oldTitle = null;
        }
    }
        
    private void SaveFile(TreasureHunt.TreasureHunt treasureHunt, string filePath)
    {
        try
        {
            using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                binFormatter.Serialize(stream, treasureHunt);
            }
        }
        catch(ArgumentException e)
        {
            MonoBehaviour.print(e.Message); // TODO actually let the player know of the problem
        }
        catch (IOException e)
        {
            MonoBehaviour.print(e.Message); // TODO actually let the player know of the problem
        }
        catch (UnauthorizedAccessException e)
        {
            MonoBehaviour.print(e.Message); // TODO actually let the player know of the problem
        }
        catch(SerializationException e)
        {
            MonoBehaviour.print(e.Message); // TODO actually let the player know of the problem
        }
        catch(Exception e)
        {
            MonoBehaviour.print(e.Message); // TODO actually let the player know of the problem
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

    private void LoadWorkerDoWork(object sender, DoWorkEventArgs e) // Not on the main thread
    {
        List<TreasureHunt.TreasureHunt> treasureHunts = new List<TreasureHunt.TreasureHunt>();

        string searchPattern = "*" + Constants.Extension;
        try
        {
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

            e.Result = treasureHunts;
        }
        catch (ArgumentException exc)
        {
            MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
        }
        catch (IOException exc)
        {
            MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
        }
        catch (UnauthorizedAccessException exc)
        {
            MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
        }
        catch(SerializationException exc)
        {
            MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
        }
        catch(Exception exc)
        {
            MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
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

    private void RemoveWorkerDoWork(object sender, DoWorkEventArgs e)
    {
        TreasureHunt.TreasureHunt treasureHunt = e.Argument as TreasureHunt.TreasureHunt;

        string filePath = persistentDataPath + "/" + treasureHunt.Title + Constants.Extension;

        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (UnauthorizedAccessException exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
            catch (IOException exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
            catch (Exception exc)
            {
                MonoBehaviour.print(exc.Message); // TODO actually let the player know of the problem
            }
        }
    }
}

