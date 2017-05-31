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

    public void GetAddTreasureHunt(Transform parent)
    {
        ActivateItem(addTreasureHunt, parent);
    }

    public void GetAddProblem(Transform parent)
    {
        ActivateItem(addProblem, parent);
    }

    public void GetAddTask(Transform parent)
    {
        ActivateItem(addTask, parent);
    }

    #endregion

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

    public void ReclaimGameItem(GameItem gameItem, int index = -1)
    {
        if (index != -1)
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

    private void Start()
    {
        
    }

    private void InstantiateGameItems(int numberOfGameItems)
    {
        for (int i = 0; i < numberOfGameItems; i++)
        {
            GameItem newGameItem = Instantiate(gameItemPrefab);
            ResetGameItem(newGameItem);

            inactiveGameItems.Add(newGameItem);
        }        
    }

    private void ExpandPool()
    {
        InstantiateGameItems(currentItemCount);
        currentItemCount *= 2;
    }

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

    private void ActivateItem(GameObject item, Transform parent)
    {
        item.transform.SetParent(parent, false);
        item.SetActive(true);
    }

    private void DeactivateItem(GameObject item)
    {
        item.transform.SetParent(this.transform, false);
        item.SetActive(false);
    }
}

