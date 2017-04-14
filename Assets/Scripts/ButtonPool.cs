using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ButtonPool : MonoBehaviour
{
    private int currentButtonGroupCount;
    private Queue<Transform> inactiveButtonGroups;
    private Queue<Transform> activeButtonGroups;

    private int currentButtonCount;
    private Queue<Transform> inactiveButton;
    private Queue<Transform> activeButton;

    public int poolStartingCount = 10; // TODO perhaps put this in constants
    public GameObject buttonGroupPrefab;
    public GameObject buttonPrefab;

    public static ButtonPool Instance { get; private set; }

    public Transform BackButton { get; private set; }    

    public Transform GetButtonGroup(Transform parent)
    {
        if (inactiveButtonGroups.Count == 0)
        {
            ExpandButtonPool();
        }

        Transform buttonGroup = inactiveButtonGroups.Dequeue();
        buttonGroup.SetParent(parent, false);
        buttonGroup.gameObject.SetActive(true);
        activeButtonGroups.Enqueue(buttonGroup);

        return buttonGroup;
    }

    public void ReclaimButtons()
    {
        while (activeButtonGroups.Count > 0)
        {
            Transform buttonGroup = activeButtonGroups.Dequeue();
            ResetButtonGroup(buttonGroup);
            inactiveButtonGroups.Enqueue(buttonGroup);
        }
    }

    public void ShowBackButton(Transform parent)
    {
        BackButton.SetParent(parent, false);
        BackButton.gameObject.SetActive(true);
    }

    public void HideBackButton()
    {
        BackButton.SetParent(this.transform, false);
        BackButton.gameObject.SetActive(false);
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

        inactiveButtonGroups = new Queue<Transform>();
        activeButtonGroups = new Queue<Transform>();

        currentButtonGroupCount = poolStartingCount;
        InstantiateButtons(currentButtonGroupCount);

        BackButton = Instantiate(buttonPrefab).transform;
        BackButton.name = "Back Button";
        HideBackButton();
    }

    private void Start()
    {
        
    }

    private void InstantiateButtons(int numberOfButtons)
    {
        for (int i = 0; i < numberOfButtons; i++)
        {
            Transform newButtonGroup = Instantiate(buttonGroupPrefab).transform;
            ResetButtonGroup(newButtonGroup);

            inactiveButtonGroups.Enqueue(newButtonGroup);
        }        
    }

    private void ExpandButtonPool()
    {
        InstantiateButtons(currentButtonGroupCount);
        currentButtonGroupCount *= 2;
    }

    private void ResetButtonGroup(Transform buttonGroup)
    {
        buttonGroup.gameObject.SetActive(false);
        buttonGroup.SetParent(this.transform, false);

        Button button = buttonGroup.FindChild("Button").GetComponent<Button>(); // TODO protect against null reference exceptions
        button.onClick.RemoveAllListeners();
        button.GetComponentInChildren<Text>().text = string.Empty;

        GameObject imageGameObject = buttonGroup.FindChild("Check mark").gameObject;
        imageGameObject.SetActive(false);
    }
}

