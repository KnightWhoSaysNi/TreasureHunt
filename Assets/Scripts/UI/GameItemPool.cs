using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameItemPool : MonoBehaviour
{
    private int currentItemCount;
    private List<GameItem> inactiveGameItems;
    private List<GameItem> activeGameItems;

    public int poolStartingCount = 10; // TODO perhaps put this in constants
    public GameItem gameItemPrefab;

    public GameObject addTreasureHunt;
    public GameObject addProblem;
    public GameObject addTask;

    public static GameItemPool Instance { get; private set; }  

    /// <summary>
    /// Gets the first available - inactive game item and assigns it to the new, specified parent.
    /// That game item is then added to the active game items list.
    /// </summary>
    /// <param name="parent">New parent of the game item.</param>
    public GameItem GetGameItem(Transform parent)
    {
        if (inactiveGameItems.Count == 0)
        {
            ExpandPool();
        }

        GameItem gameItem = inactiveGameItems[0];
        inactiveGameItems.RemoveAt(0);
        ActivateItem(gameItem.gameObject, parent);
        activeGameItems.Add(gameItem);

        return gameItem;
    }

    #region  - Addition GameObjects -

    /// <summary>
    /// Gets the addTreasureHunt game object (button) and assigns a new parent to it.
    /// </summary>
    /// <param name="parent">New parent of the addTreasureHunt game object.</param>
    public void GetAddTreasureHunt(Transform parent)
    {
        ActivateItem(addTreasureHunt, parent);
    }

    /// <summary>
    /// Gets the addProblem game object (button) and assigns a new parent to it.
    /// </summary>
    /// <param name="parent">New parent of the addProblem game object.</param>
    public void GetAddProblem(Transform parent)
    {
        ActivateItem(addProblem, parent);
    }

    /// <summary>
    /// Gets the addTask game object (button) and assigns a new parent to it.
    /// </summary>
    /// <param name="parent">New parent of the addTask game object.</param>
    public void GetAddTask(Transform parent)
    {
        ActivateItem(addTask, parent);
    }

    #endregion
    
    /// <summary>
    /// Clears the active game items list and all addition game objects (buttons) and assigns them a new parent - this.transform.
    /// All active game objects are returned to the inactive list.
    /// </summary>
    public void ReclaimGameItems()
    {
        while (activeGameItems.Count > 0)
        {
            GameItem gameItem = activeGameItems[0];
            ReclaimGameItem(gameItem, 0);
        }

        if (!addTreasureHunt.transform.IsChildOf(this.transform))
        {
            DeactivateItem(addTreasureHunt);
        }
        if (!addProblem.transform.IsChildOf(this.transform))
        {
            DeactivateItem(addProblem);
        }
        if (!addTask.transform.IsChildOf(this.transform))
        {
            DeactivateItem(addTask);
        }
    }

    /// <summary>
    /// Removes the specified game item from the active game items list, resets it and returns it to 
    /// the inactive game items. If the index is higher than -1 removes the item at that index, otherwise
    /// the whole list is traversed to find that particular game item, which is then removed.
    /// </summary>
    /// <param name="gameItem">Game item to be reclaimed.</param>
    /// <param name="index">Index of the game item. -1 if the index is not known.</param>
    public void ReclaimGameItem(GameItem gameItem, int index = -1)
    {
        if (index > -1)
        {
            activeGameItems.RemoveAt(index);
        }
        else
        {
            activeGameItems.Remove(gameItem);
        }

        ResetGameItem(gameItem);
        inactiveGameItems.Add(gameItem);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

        inactiveGameItems = new List<GameItem>();
        activeGameItems = new List<GameItem>();

        currentItemCount = poolStartingCount;
        InstantiateGameItems(currentItemCount);
    }

    /// <summary>
    /// Instantiates the specified number of new game items and adds them to the inactive game items list.
    /// </summary>
    private void InstantiateGameItems(int numberOfGameItems)
    {
        for (int i = 0; i < numberOfGameItems; i++)
        {
            GameItem newGameItem = Instantiate(gameItemPrefab);
            ResetGameItem(newGameItem);

            inactiveGameItems.Add(newGameItem);
        }        
    }

    /// <summary>
    /// Doubles the count of items and adds new game items to the inactive game items.
    /// </summary>
    private void ExpandPool()
    {
        InstantiateGameItems(currentItemCount);
        currentItemCount *= 2;
    }

    /// <summary>
    /// Deactivates the specified game item and resets its important values.
    /// </summary>
    private void ResetGameItem(GameItem gameItem)
    {
        DeactivateItem(gameItem.gameObject);

        Button button = gameItem.GetComponent<Button>(); // TODO protect against null reference exceptions    
        button.interactable = true;    
        button.onClick.RemoveAllListeners();

        gameItem.text.text = string.Empty;
        gameItem.removeButton.SetActive(false);
        gameItem.checkMark.SetActive(false);
    }

    /// <summary>
    /// Activates the given game object and sets its parent to the specified transform.
    /// </summary>
    private void ActivateItem(GameObject item, Transform parent)
    {
        item.transform.SetParent(parent, false);
        item.SetActive(true);
    }

    /// <summary>
    /// Deactivates the given game object and sets its parent to this.transform.
    /// </summary>
    private void DeactivateItem(GameObject item)
    {
        item.transform.SetParent(this.transform, false);
        item.SetActive(false);
    }
}

