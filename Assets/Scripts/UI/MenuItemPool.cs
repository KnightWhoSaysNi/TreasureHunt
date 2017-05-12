using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MenuItemPool : MonoBehaviour
{
    private int currentItemCount;
    private Queue<MenuItem> inactiveMenuItems;
    private Queue<MenuItem> activeMenuItems;

    public int poolStartingCount = 10; // TODO perhaps put this in constants
    public MenuItem menuItemPrefab;
    public MenuItem addProblem;
    public MenuItem addTask;

    public static MenuItemPool Instance { get; private set; }  

    public MenuItem GetMenuItem(Transform parent)
    {
        if (inactiveMenuItems.Count == 0)
        {
            ExpandPool();
        }

        MenuItem menuItem = inactiveMenuItems.Dequeue();
        ActivateMenuItem(menuItem, parent);
        activeMenuItems.Enqueue(menuItem);

        return menuItem;
    }

    public MenuItem GetAddProblem(Transform parent)
    {
        ActivateMenuItem(addProblem, parent);

        return addProblem;
    }

    public MenuItem GetAddTask(Transform parent)
    {
        ActivateMenuItem(addTask, parent);

        return addTask;
    }

    public void ReclaimMenuItems()
    {
        while (activeMenuItems.Count > 0)
        {
            MenuItem menuItem = activeMenuItems.Dequeue();
            ResetMenuItem(menuItem);
            inactiveMenuItems.Enqueue(menuItem);
        }

        if (!addProblem.transform.IsChildOf(this.transform))
        {
            ResetMenuItem(addProblem);
        }
        if (!addTask.transform.IsChildOf(this.transform))
        {
            ResetMenuItem(addTask);
        }
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

        inactiveMenuItems = new Queue<MenuItem>();
        activeMenuItems = new Queue<MenuItem>();

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

            inactiveMenuItems.Enqueue(newMenuItem);
        }        
    }

    private void ExpandPool()
    {
        InstantiateMenuItems(currentItemCount);
        currentItemCount *= 2;
    }

    private void ResetMenuItem(MenuItem menuItem)
    {
        DeactivateMenuItem(menuItem);

        Button button = menuItem.GetComponent<Button>(); // TODO protect against null reference exceptions        
        button.onClick.RemoveAllListeners();

        menuItem.text.text = string.Empty;
        menuItem.removeButton.SetActive(false);
        menuItem.checkMark.SetActive(false);
    }

    private void ActivateMenuItem(MenuItem menuItem, Transform parent)
    {
        menuItem.transform.SetParent(parent, false);
        menuItem.gameObject.SetActive(true);
    }

    private void DeactivateMenuItem(MenuItem menuItem)
    {
        menuItem.transform.SetParent(this.transform, false);
        menuItem.gameObject.SetActive(false);
    }
}

