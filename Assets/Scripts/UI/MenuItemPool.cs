using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MenuItemPool : MonoBehaviour
{
    private int currentItemCount;
    private List<MenuItem> inactiveMenuItems;
    private List<MenuItem> activeMenuItems;

    public int poolStartingCount = 10; // TODO perhaps put this in constants
    public MenuItem menuItemPrefab;

    public GameObject addTreasureHunt;
    public GameObject addProblem;
    public GameObject addTask;

    public static MenuItemPool Instance { get; private set; }  

    public MenuItem GetMenuItem(Transform parent)
    {
        if (inactiveMenuItems.Count == 0)
        {
            ExpandPool();
        }

        MenuItem menuItem = inactiveMenuItems[0];
        inactiveMenuItems.RemoveAt(0);
        ActivateItem(menuItem.gameObject, parent);
        activeMenuItems.Add(menuItem);

        return menuItem;
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

    public void ReclaimMenuItems()
    {
        while (activeMenuItems.Count > 0)
        {
            MenuItem menuItem = activeMenuItems[0];
            ReclaimMenuItem(menuItem, 0);
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

    public void ReclaimMenuItem(MenuItem menuItem, int index = -1)
    {
        if (index != -1)
        {
            activeMenuItems.RemoveAt(index);
        }
        else
        {
            activeMenuItems.Remove(menuItem);
        }

        ResetMenuItem(menuItem);
        inactiveMenuItems.Add(menuItem);
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

        inactiveMenuItems = new List<MenuItem>();
        activeMenuItems = new List<MenuItem>();

        currentItemCount = poolStartingCount;
        InstantiateMenuItems(currentItemCount);
    }

    private void Start()
    {
        
    }

    private void InstantiateMenuItems(int numberOfMenuItems)
    {
        for (int i = 0; i < numberOfMenuItems; i++)
        {
            MenuItem newMenuItem = Instantiate(menuItemPrefab);
            ResetMenuItem(newMenuItem);

            inactiveMenuItems.Add(newMenuItem);
        }        
    }

    private void ExpandPool()
    {
        InstantiateMenuItems(currentItemCount);
        currentItemCount *= 2;
    }

    private void ResetMenuItem(MenuItem menuItem)
    {
        DeactivateItem(menuItem.gameObject);

        Button button = menuItem.GetComponent<Button>(); // TODO protect against null reference exceptions        
        button.onClick.RemoveAllListeners();

        menuItem.text.text = string.Empty;
        menuItem.removeButton.SetActive(false);
        menuItem.checkMark.SetActive(false);
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

