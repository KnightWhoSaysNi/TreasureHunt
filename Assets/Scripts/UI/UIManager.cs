using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
public class UIManager : MonoBehaviour
{
    private GameManager gameManager;

    private Dictionary<string, Menu> menuDictionary;
    private Menu currentMenu;
    private Stack<Menu> menuStack;
    
    private bool isTaskActive;
    
    #region - Inspector Variables -

    [Header("Header")]
    public Text headerText;
    public RectTransform headerInputField;

    [Header("Menu")]
    public Menu mainMenu;
    public RectTransform menuItemsPanel;
    public Transform menuContent;

    [Header("Task")]
    public RectTransform taskPanel;
    public Text taskText;
    public RectTransform taskInputField;

    [Header("Hint")]
    public RectTransform hintPanel;
    public Text hintText;
    public RectTransform hintInputField;

    [Space(10)] // Hint Navigation
    public RectTransform hintNavigationPanel;
    public Button previousHint;
    public Button nextHint;

    [Space(10)] // Hint options
    public RectTransform hintPlayOptions;
    public RectTransform hintCreationOptions;
    public Button revealHint;
    public Text remainingHintsText;
    public Button addHint;
    public Button removeHint;

    [Header("Answer")]
    public RectTransform answerPanel;
    public RectTransform answerInputField;
    public RectTransform answerPlayOptions;
    public RectTransform answerCreationOptions;

    [Header("Back")]
    public RectTransform backPanel;

    #endregion

    #region - Event Handlers for menu items -

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        Menu previousMenu = menuStack.Peek();
        OpenMenu(previousMenu, true);
    }

    #endregion

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
        
        currentMenu = mainMenu;
        
        menuDictionary = new Dictionary<string, Menu>();
        menuStack = new Stack<Menu>();

        UpdateMenu();
    }
        

    private void OpenMenu(Menu newMenu, bool isBackUsed = false)
    {
        if (!isBackUsed)
        {
            menuStack.Push(currentMenu);
        }
        else
        {
            menuStack.Pop();
        }

        currentMenu = newMenu;
        headerText.text = newMenu.header;
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        if (isTaskActive)
        {
            
        }
        else
        {   
            menuDictionary.Clear();
            MenuItemPool.Instance.ReclaimMenuItems();        

            UpdateMenuItems();
        }
    }

    private void UpdateMenuItems()
    {
        foreach (MenuItemData menuItemData in currentMenu.menuItems)
        {
            Transform menuItem = MenuItemPool.Instance.GetMenuItem(menuContent);

            string menuHeader = menuItemData.header;
            menuItem.GetComponentInChildren<Text>().text = menuHeader; // TODO this could throw exception if there is no Text component in the button            

            Menu menu = menuItemData.menu;
            if (menu == null)
            {
                menu = new Menu();
                menu.menuItems = new List<MenuItemData>();
                menu.header = menuHeader;
            }
            menuDictionary.Add(menuHeader, menu);            

            Button button = menuItem.GetComponent<Button>(); // TODO check if null
            // If menuItemData.menu is used directly in AddListener by the time it is called it will not be available
            button.onClick.AddListener(() => OpenMenu(menuDictionary[menuHeader]));

            if (menuHeader == "Create a new Treasure Hunt")
            {
                button.onClick.AddListener(gameManager.CreateTreasureHunt);
            }

            GameObject removeButton = menuItem.FindChild("Remove").gameObject;  // TODO check if null | constant 
            if (menuItemData.isRemoveRequired)
            {
                removeButton.SetActive(true);
            }
            else
            {
                removeButton.SetActive(false);
            }

            GameObject checkMark = menuItem.FindChild("Check mark").gameObject; // TODO check if null | constant 
            if (menuItemData.isCheckMarkRequired)
            {
                checkMark.SetActive(true);
            }
            else
            {
                checkMark.SetActive(false);
            }
        }

        if (currentMenu.menuType == Menu.MenuType.Problem)
        {
            MenuItemPool.Instance.GetAddProblem(menuContent);
        }
        else if (currentMenu.menuType == Menu.MenuType.Task)
        {
            MenuItemPool.Instance.GetAddTask(menuContent);
        }


        // Show/hide back panel
        if (currentMenu.header == "Main Menu")
        {
            // Back button is not needed
            backPanel.gameObject.SetActive(false);
        }
        else
        {
            // Back button is needed            
            backPanel.gameObject.SetActive(true);
            //Menu previousMenu = menuStack.Peek();           
        }
    }
    
    private void OnTaskActivated()
    {
        menuItemsPanel.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        answerPanel.gameObject.SetActive(true);

        isTaskActive = true;

        SetUIToGameMode(gameManager.GameMode);
    }     

    private void OnTaskDeactivated()
    {
        menuItemsPanel.gameObject.SetActive(true);
        taskPanel.gameObject.SetActive(false);
        answerPanel.gameObject.SetActive(false);

        isTaskActive = false;

        SetUIToGameMode(gameManager.GameMode);
    }

    private void SetUIToGameMode(GameMode gameMode)
    {
        bool isInPlayMode = gameMode == GameMode.PlayMode;

        // Header
        headerText.gameObject.SetActive(isInPlayMode);
        headerInputField.gameObject.SetActive(!isInPlayMode);

        // Task
        taskText.gameObject.SetActive(isInPlayMode);
        taskInputField.gameObject.SetActive(!isInPlayMode);

        // Hint
        hintText.gameObject.SetActive(isInPlayMode);
        hintInputField.gameObject.SetActive(!isInPlayMode);

        hintPlayOptions.gameObject.SetActive(isInPlayMode);
        hintCreationOptions.gameObject.SetActive(!isInPlayMode);

        // Answer
        answerPlayOptions.gameObject.SetActive(isInPlayMode);
        answerCreationOptions.gameObject.SetActive(!isInPlayMode);
    }
}