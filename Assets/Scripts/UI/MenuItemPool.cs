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
    public Transform addProblem;
    public Transform addTask;

    public static MenuItemPool Instance { get; private set; }  

    public MenuItem GetMenuItem(Transform parent)
    {
        if (inactiveMenuItems.Count == 0)
        {
            ExpandPool();
        }

        MenuItem menuItem = inactiveMenuItems.Dequeue();
        ActivateMenuItem(menuItem.transform, parent);
        activeMenuItems.Enqueue(menuItem);

        return menuItem;
    }

    public Transform GetAddProblem(Transform parent)
    {
        ActivateMenuItem(addProblem, parent);

        return addProblem;
    }

    public Transform GetAddTask(Transform parent)
    {
        ActivateMenuItem(addTask, parent);

        return addTask;
    }

    public void ReclaimMenuItems()
    {
        while (activeMenuItems.Count > 0)
        {
            MenuItem menuItem = activeMenuItems.Dequeue();
            ResetMenuItem(menuItem.transform);
            inactiveMenuItems.Enqueue(menuItem);
        }

        if (!addProblem.IsChildOf(this.transform))
        {
            ResetMenuItem(addProblem);
        }
        if (!addTask.IsChildOf(this.transform))
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
            ResetMenuItem(newMenuItem.transform);

            inactiveMenuItems.Enqueue(newMenuItem);
        }        
    }

    private void ExpandPool()
    {
        InstantiateMenuItems(currentItemCount);
        currentItemCount *= 2;
    }

    private void ResetMenuItem(Transform menuItem)
    {
        DeactivateMenuItem(menuItem);

        Button button = menuItem.GetComponent<Button>(); // TODO protect against null reference exceptions        
        button.onClick.RemoveAllListeners();
        button.GetComponentInChildren<Text>().text = string.Empty;

        GameObject removeButton = menuItem.FindChild("Remove").gameObject; // TODO put both remove and check mark in a class which gets them directly from prefab... if possible
        removeButton.SetActive(false);

        GameObject checkMark = menuItem.FindChild("Check mark").gameObject;
        checkMark.SetActive(false);
    }

    private void ActivateMenuItem(Transform menuItem, Transform parent)
    {
        menuItem.SetParent(parent, false);
        menuItem.gameObject.SetActive(true);
    }

    private void DeactivateMenuItem(Transform menuItem)
    {
        menuItem.SetParent(this.transform, false);
        menuItem.gameObject.SetActive(false);
    }
}

