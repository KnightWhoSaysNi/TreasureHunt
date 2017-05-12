using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TreasureHunt;

[RequireComponent(typeof(GameManager))]
public class UIManager : MonoBehaviour
{
    private GameManager gameManager;

    private Transform currentMenu;
    private Stack<Transform> menuStack;
    private Stack<GameMenuTuple> gameMenuStack;    
    
    #region - Inspector Variables -

    [Header("Header")]
    public Text headerText;
    public RectTransform headerInputField;

    [Header("Menu")]
    public Transform mainMenu;
    public RectTransform menuPanel;
    public Transform gameMenu;

    [Header("Task")]
    public RectTransform taskPanel;
    public Text taskText;
    public InputField taskInputField;

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
    public InputField answerInputField;
    public RectTransform answerPlayOptions;
    public Button checkAnswer;
    public Button cancelAnswer;
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
        if (gameMenuStack.Count != 0)
        {
            if (gameMenuStack.Peek().MenuItemType == MenuItemType.Task)
            {
                // As the last game menu held a Task, Task panel is currently active
                gameMenu.gameObject.SetActive(true);
                taskPanel.gameObject.SetActive(false);
                answerPanel.gameObject.SetActive(false);
                
                var stackItem = gameMenuStack.Pop();
                SetHeader(stackItem.Title);
            }
            else
            {
                GameMenuTuple previousGameMenu = gameMenuStack.Pop();
                UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.MenuItemType);
            }
        }
        else
        {
            // Regular menu - Not a Treasure Hunt list or its problems/tasks
            currentMenu.gameObject.SetActive(false);
            Transform menu = menuStack.Pop();
            UpdateCurrentMenu(menu);
            currentMenu.gameObject.SetActive(true);
        }
    }

    #endregion

    #region - Other Menus -

    public void ShowAllTreasureHunts(string header)
    {        
        UpdateGameMenu(header, gameManager.AllTreasureHunts);
    }

    private void UpdateGameMenu(string header, IEnumerable listOfItems, MenuItemType menuItemType = MenuItemType.TreasureHunt)
    {
        SetHeader(header);

        MenuItemPool.Instance.ReclaimMenuItems();        

        foreach (var item in listOfItems)
        {
            MenuItem menuItem = MenuItemPool.Instance.GetMenuItem(gameMenu);
            menuItem.MenuItemType = menuItemType;

            string title = ((ITitle)item).Title;
            menuItem.text.text = title;

            if (gameManager.GameMode == GameMode.CreationMode)
            {
                menuItem.removeButton.SetActive(true);
            }

            Button button = menuItem.GetComponent<Button>();
            // GameMenuTuple add to the stack
            button.onClick.AddListener(() =>
            {
                GameMenuTuple stackItem = new GameMenuTuple();
                stackItem.Title = header;
                stackItem.ListOfItems = listOfItems;
                stackItem.MenuItemType = menuItemType;
                gameMenuStack.Push(stackItem);
            });

            switch (menuItemType) // TODO Create separate methods for all cases
            {
                case MenuItemType.TreasureHunt:
                    menuItem.TreasureHunt = (TreasureHunt.TreasureHunt)item;
                    button.onClick.AddListener(() => {
                        UpdateGameMenu(title, menuItem.TreasureHunt.Problems, MenuItemType.Problem);
                        gameManager.CurrentTreasureHunt = menuItem.TreasureHunt;
                    });

                    if (gameManager.GameMode == GameMode.PlayMode)
                    {
                        menuItem.checkMark.SetActive(menuItem.TreasureHunt.IsCompleted);
                    }
                    break;
                case MenuItemType.Problem:
                    menuItem.Problem = (Problem)item;
                    if (menuItem.Problem.Tasks.Count == 1)
                    {
                        // TODO skip showing Tasks if the game is in Play Mode and just open the one Task
                        // If the game is in Creation Mode show Task and the Add Task button
                    }
                    button.onClick.AddListener(() =>
                    {
                        UpdateGameMenu(title, menuItem.Problem.Tasks, MenuItemType.Task);
                        gameManager.CurrentProblem = menuItem.Problem;
                    });

                    if (gameManager.GameMode == GameMode.PlayMode)
                    {
                        menuItem.checkMark.SetActive(menuItem.Problem.IsSolved);
                    }
                    break;
                case MenuItemType.Task:
                    menuItem.Task = (Task)item;
                    // TODO Add button listener which shows the Task panel with the appropriate Task
                    button.onClick.AddListener(() =>
                    {                        
                        ActivateTask(menuItem.Task);
                    });

                    if (gameManager.GameMode == GameMode.PlayMode && menuItem.Task.IsSolved)
                    {
                        menuItem.checkMark.SetActive(true);
                    }

                    break;
                default:
                    break;
            }                     
        }
    }
    
    private void SetHeader(string header)
    {
        headerText.text = header;
        headerInputField.Find("Placeholder").GetComponent<Text>().text = header;
        headerInputField.Find("Text").GetComponent<Text>().text = header;
    }

    private void ActivateTask(Task task)
    {
        gameManager.CurrentTask = task;

        // Show Task and Answer panels and hide Game Menu
        gameMenu.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        answerPanel.gameObject.SetActive(true);       
                
        SetHeader(task.Title);

        if (gameManager.GameMode == GameMode.PlayMode)
        {
            taskText.text = task.TextClue;
            taskText.gameObject.SetActive(true);
            taskInputField.gameObject.SetActive(false);

            if (task.IsSolved)
            {
                answerInputField.text = task.Solution.TextSolution;
                answerInputField.interactable = false;
                checkAnswer.interactable = false;
                cancelAnswer.interactable = false;
            }
            else
            {
                answerInputField.text = string.Empty;
                answerInputField.interactable = true;
                checkAnswer.interactable = true;
                cancelAnswer.interactable = true;
            }
        }
        else // GameMode is Creation Mode
        {
            taskInputField.text = task.TextClue;
            taskText.gameObject.SetActive(false);
            taskInputField.gameObject.SetActive(true);

            answerInputField.text = task.Solution.TextSolution;
            answerInputField.interactable = true;
        }
    }

    #endregion

    #region - Menu change => Adding menu to stack and updating current menu - 

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

    #endregion

    private void Start()
    {
        gameManager = GetComponent<GameManager>();

        currentMenu = mainMenu;

        menuStack = new Stack<Transform>();
        gameMenuStack = new Stack<GameMenuTuple>();

        
        gameManager.GameModeChanged += OnGameModeChanged;
        gameManager.TreasureHuntCreated += OnTreasureHuntCreated;
        gameManager.ProblemCreated += OnProblemCreated;
        gameManager.TaskCreated += OnTaskCreated;
    }           

    private void OnTreasureHuntCreated()
    {
        // TODO In GameMenu show appropriate options
        gameManager.GoToCreationMode();

        UpdateGameMenu(gameManager.CurrentTreasureHunt.Title, gameManager.CurrentTreasureHunt.Problems, MenuItemType.Problem);
        MenuItemPool.Instance.GetAddProblem(gameMenu);
    }

    private void OnProblemCreated()
    {
        // Game is already in Creation Mode
        UpdateGameMenu(gameManager.CurrentProblem.Title, gameManager.CurrentProblem.Tasks, MenuItemType.Task);
        MenuItemPool.Instance.GetAddTask(gameMenu);
    }

    private void OnTaskCreated()
    {
        // Game is in Creation Mode
        ActivateTask(gameManager.CurrentTask);
    }

    private void OnGameModeChanged()
    {
        bool isInPlayMode = gameManager.GameMode == GameMode.PlayMode;

        // Header // TODO this should be set somewhere else
        //headerText.gameObject.SetActive(isInPlayMode);
        //headerInputField.gameObject.SetActive(!isInPlayMode);

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

public class GameMenuTuple
{
    public string Title { get; set; }
    public IEnumerable ListOfItems { get; set; }
    public MenuItemType MenuItemType { get; set; }
}

