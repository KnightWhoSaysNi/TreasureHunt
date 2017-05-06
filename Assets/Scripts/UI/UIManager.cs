using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(GameManager))]
public class UIManager : MonoBehaviour
{
    private GameManager gameManager;

    private Dictionary<string, Menu> gameMenuDictionary;
    private Transform currentMenu;
    private Menu currentGameMenu;
    private Stack<Transform> menuStack;
    private Stack<Menu> gameMenuStack;
    
    private bool isTaskActive;
    
    #region - Inspector Variables -

    [Header("Header")]
    public Text headerText;
    public RectTransform headerInputField;

    [Header("Menu")]
    public Transform mainMenu;
    public RectTransform menuItemsPanel;
    public Transform gameMenu;

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

    #region - Main Menu -

    public void Quit()
    {
        Application.Quit();
    }

    public void Back()
    {
        //if (isGameMenuOpened)
        //{
        //    // Treasure Hunt list, or Problems or Tasks are opened
        //    // TODO
        //Menu previousMenu = gameMenuStack.Peek();
        //OpenMenu(previousMenu, true);
        //}

        // Regular menu - Not a Treasure Hunt list or its problems/tasks
        currentMenu.gameObject.SetActive(false);
        Transform menu = menuStack.Pop();
        UpdateCurrentMenu(menu);
        currentMenu.gameObject.SetActive(true);
    }

    #endregion

    #region - Play a game -

    public void ShowAllTreasureHunts()
    {
        Menu newMenu = new Menu();

    }

    #endregion

    public void AddMenuToStack(Transform menu)
    {
        menuStack.Push(menu);

        if (menu.name == "Main Menu")
        {
            backPanel.gameObject.SetActive(true);
        }
    }

    public void UpdateCurrentMenu(Transform menu)
    {
        currentMenu = menu;
        headerText.text = currentMenu.name; // TODO resolve the creation mode case and also in Game menu Menu class is used, not Transform with .name
        if (currentMenu.name == "Game Menu")
        {
            // TODO Header needs to be set to the appropriate text            
        }

        if (menu.name == "Main Menu")
        {
            backPanel.gameObject.SetActive(false);
        }
    }

    #endregion

    private void Start()
    {
        gameManager = GetComponent<GameManager>();

        currentMenu = mainMenu;

        gameMenuDictionary = new Dictionary<string, Menu>();
        menuStack = new Stack<Transform>();
        gameMenuStack = new Stack<Menu>();

        //UpdateMenu();
    }        

    private void OpenMenu(Menu newMenu, bool isBackUsed = false)
    {
        if (!isBackUsed)
        {
            gameMenuStack.Push(currentGameMenu);
        }
        else
        {
            gameMenuStack.Pop();
        }

        currentGameMenu = newMenu;
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
            gameMenuDictionary.Clear();
            MenuItemPool.Instance.ReclaimMenuItems();        

            UpdateMenuItems();
        }
    }

    private void UpdateMenuItems()
    {        
        foreach (MenuItemData menuItemData in currentGameMenu.menuItems)
        {
            Transform menuItem = MenuItemPool.Instance.GetMenuItem(gameMenu).transform;

            string menuHeader = menuItemData.header;
            menuItem.GetComponentInChildren<Text>().text = menuHeader; // TODO this could throw exception if there is no Text component in the button            

            Menu menu = menuItemData.menu;
            if (menu == null)
            {
                menu = new Menu();
                menu.menuItems = new List<MenuItemData>();
                menu.header = menuHeader;
            }
            gameMenuDictionary.Add(menuHeader, menu);            

            Button button = menuItem.GetComponent<Button>(); // TODO check if null
            // If menuItemData.menu is used directly in AddListener by the time it is called it will not be available
            button.onClick.AddListener(() => OpenMenu(gameMenuDictionary[menuHeader]));

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

        if (currentGameMenu.menuType == Menu.GameMenuType.Problem)
        {
            MenuItemPool.Instance.GetAddProblem(gameMenu);
        }
        else if (currentGameMenu.menuType == Menu.GameMenuType.Task)
        {
            MenuItemPool.Instance.GetAddTask(gameMenu);
        }


        // Show/hide back panel
        if (currentGameMenu.header == "Main Menu")
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

    private void OnTreasureHuntCreated()
    {
        // TODO In GameMenu show appropriate options
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