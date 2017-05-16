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
    public InputField headerInputField;

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
    public InputField hintInputField;

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
    [Space(10)]
    public RectTransform answerPlayOptions;
    public Button checkAnswer;
    public Button cancelAnswer;
    public RectTransform answerCreationOptions;

    [Header("Save reminder")]
    public GameObject saveReminderPanel;
    public Text saveReminderText;
    public GameObject saveReminderButtons;

    [Header("Back")]
    public RectTransform backPanel;

    #endregion

    #region - Event Handlers and methods for UI elements -

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
                // As the last item on the stack is a Task, Task panel is currently active

                // A check to see if Task, Answer or Hint input fields are empty
                if (!CanTaskBeSaved(false))
                {
                    saveReminderPanel.SetActive(true);
                    saveReminderText.text = "Your task is not yet saved. If you go back it will be deleted. Are you sure you want to go back?";
                    saveReminderButtons.SetActive(true);

                    // Stopping the 'Back' process until the player decides what to do
                    return;                    
                }

                gameMenu.gameObject.SetActive(true);
                taskPanel.gameObject.SetActive(false);
                answerPanel.gameObject.SetActive(false);
            }

            GameMenuTuple previousGameMenu = gameMenuStack.Pop();
            UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.MenuItemType);
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

    public void ConfirmGoingBack(bool isBackConfirmed)
    {
        if (isBackConfirmed)
        {
            ContinueWithBackMethod();
        }
        else
        {
            // Player decided to stay and complete the task
        }
    }

    private void ContinueWithBackMethod()
    {
        // Player decided to delete the task and go back
        gameManager.RemoveTask(gameManager.CurrentTask);

        gameMenu.gameObject.SetActive(true);
        taskPanel.gameObject.SetActive(false);
        answerPanel.gameObject.SetActive(false);

        GameMenuTuple previousGameMenu = gameMenuStack.Pop();
        UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.MenuItemType);
    }

    #endregion

    #region - Other Menus -

    public void ShowAllTreasureHunts()
    {
        string header = "All Treasure Hunts";
        UpdateGameMenu(header, gameManager.AllTreasureHunts);
    }

    private void UpdateGameMenu(string header, IEnumerable listOfItems, MenuItemType menuItemType = MenuItemType.TreasureHunt)
    {
        SetHeader(header, menuItemType);

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
                //stackItem.Title = header;
                stackItem.ListOfItems = listOfItems;
                stackItem.MenuItemType = menuItemType;
                gameMenuStack.Push(stackItem);
            });

            switch (menuItemType) // TODO Create separate methods for all cases
            {
                case MenuItemType.TreasureHunt:
                    menuItem.TreasureHunt = (TreasureHunt.TreasureHunt)item;
                    button.onClick.AddListener(() => 
                    {
                        gameMenuStack.Peek().Title = header;

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
                        gameMenuStack.Peek().Title = gameManager.CurrentTreasureHunt.Title;

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
                        gameMenuStack.Peek().Title = gameManager.CurrentProblem.Title;

                        gameManager.CurrentTask = menuItem.Task;                  
                        ActivateTask();
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

        if (gameManager.GameMode == GameMode.CreationMode)
        {
            // Adding an Add button for TreasureHunt/Problem/Task
            switch (menuItemType)
            {
                case MenuItemType.TreasureHunt:
                    MenuItemPool.Instance.GetAddTreasureHunt(gameMenu);
                    break;
                case MenuItemType.Problem:
                    MenuItemPool.Instance.GetAddProblem(gameMenu);
                    break;
                case MenuItemType.Task:
                    MenuItemPool.Instance.GetAddTask(gameMenu);
                    break;
                default:
                    break;
            }
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

    #region - Task related methods - 

    public void CheckAnswer()
    {
        // TODO show Correct "sign" if the answer is correct and maybe a big red X if it's incorrect
        // TODO incorpoate location in the possible solution
        string possibleAnswer = answerInputField.text;

        if (gameManager.CurrentTask.Solution.TextSolution == possibleAnswer)
        {
            gameManager.CurrentTask.IsSolved = true;            
            DisplaySolvedTask();
        }
    }

    public void CancelPlayModeTask()
    {
        answerInputField.text = string.Empty;
        // TODO reset Location input as well
    }

    public void SaveTask()
    {
        bool canTaskBeSaved = CanTaskBeSaved();
        if (!canTaskBeSaved)
        {
            // Task cannot be saved at the moment
            return;
        }
        
        if (gameManager.CurrentHint != null)
        {    
            SaveHint();
        }
        
        gameManager.CurrentTask.TextClue = taskInputField.text;
        gameManager.CurrentTask.Solution.TextSolution = answerInputField.text; // TODO incorporate the location into solution

        addHint.interactable = true;
    }

    public void CancelCreationModeTask()
    {
        taskInputField.text = gameManager.CurrentTask.TextClue;
        if (gameManager.CurrentHint != null)
        {
            hintInputField.text = gameManager.CurrentHint.Text;
        }
        answerInputField.text = gameManager.CurrentTask.Solution.TextSolution; // TODO incorporate location into solution
    }

    private void ActivateTask()
    {
        SetHeader(gameManager.CurrentTask.Title, MenuItemType.Task);

        // Show Task and Answer panels and hide Game Menu
        gameMenu.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        answerPanel.gameObject.SetActive(true);

        if (gameManager.GameMode == GameMode.PlayMode)
        {
            taskText.text = gameManager.CurrentTask.TextClue;

            if (gameManager.CurrentTask.RevealedHints.Count == 0)
            {
                // There are no revealed hints for this taks 
                hintPanel.gameObject.SetActive(false);
            }
            else
            {
                // There are revealed hints for this Task
                RefreshHintNavigation();

                hintPanel.gameObject.SetActive(true);
                gameManager.CurrentHint = gameManager.CurrentTask.RevealedHints[0];
                hintText.text = gameManager.CurrentHint.Text;
            }

            if (gameManager.CurrentTask.IsSolved)
            {
                DisplaySolvedTask();
            }
            else // Task is not solved yet
            {
                answerInputField.text = string.Empty;
                answerInputField.interactable = true;
                checkAnswer.interactable = true;
                cancelAnswer.interactable = true;
            }
        }
        else // GameMode is Creation Mode
        {
            taskInputField.text = gameManager.CurrentTask.TextClue;
            addHint.interactable = true;     

            if (gameManager.CurrentTask.AllHints.Count == 0)
            {
                // No hints for this task yet
                hintPanel.gameObject.SetActive(false);
            }
            else
            {
                // Task has hints
                RefreshHintNavigation();

                hintPanel.gameObject.SetActive(true);
                gameManager.CurrentHint = gameManager.CurrentTask.AllHints[0];
                hintInputField.text = gameManager.CurrentHint.Text;
            }

            answerInputField.text = gameManager.CurrentTask.Solution.TextSolution;
            answerInputField.interactable = true;
        }
    }

    private void DisplaySolvedTask()
    {
        answerInputField.text = gameManager.CurrentTask.Solution.TextSolution; // TODO Incorporate the Location into the solution
        answerInputField.interactable = false;
        checkAnswer.interactable = false;
        cancelAnswer.interactable = false;

        if (gameManager.CurrentTask.RevealedHints.Count != 0)
        {
            hintText.text = gameManager.CurrentTask.RevealedHints[0].Text;
        }
    }

    private bool CanTaskBeSaved(bool isReminderNeeded = true)
    {
        bool canTaskBeSaved = true;
        string saveMessage = string.Empty;

        if (taskInputField.text == string.Empty)
        {
            saveMessage = "You cannot save an empty task. Please write some clue or delete the task.";
            canTaskBeSaved = false;
        }
        else if (answerInputField.text == string.Empty)
        {
            saveReminderPanel.SetActive(true);
            saveMessage = "You cannot save a task without an answer.";
            canTaskBeSaved = false;
        }
        else if (gameManager.CurrentHint != null && !CanHintBeSaved(false))
        {
            // Hint InputField is empty
            saveMessage = "You cannot save an empty hint. Please either add some text or remove the hint.";
            canTaskBeSaved = false;
        }

        if (isReminderNeeded)
        {
            saveReminderPanel.SetActive(true);
            saveReminderText.text = saveMessage;
        }

        return canTaskBeSaved;
    }

    #endregion

    #region - Hint related methods -

    public void RevealHint()
    {
        // This can only be called if there are some Unrevealed hints and there are Hint points available
        Hint hint = gameManager.CurrentTask.UnrevealedHints[0];
        gameManager.CurrentTask.UnrevealedHints.RemoveAt(0);
        gameManager.CurrentTask.RevealedHints.Add(hint);
        gameManager.CurrentHint = hint;
        gameManager.CurrentTreasureHunt.HintPointsAvailable--;

        hintPanel.gameObject.SetActive(true);
        hintText.text = hint.Text;

        RefreshHintNavigation();
    }

    public void ShowPreviousHint()
    {
        if (gameManager.GameMode == GameMode.PlayMode)
        {
            ShowHint(-1);
        }
        else if (CanHintBeSaved()) // Creation Mode
        {
            if (gameManager.CurrentHint.Text == string.Empty)
            {
                // Current Hint is empty, but hintInputField.text is not
                SaveHint();
            }
            ShowHint(-1);
        }
    }

    public void ShowNextHint()
    {
        if (gameManager.GameMode == GameMode.PlayMode)
        {
            ShowHint(1);
        }
        else if (CanHintBeSaved()) // Creation Mode 
        {
            if (gameManager.CurrentHint.Text == string.Empty)
            {
                // Current Hint is empty, but hintInputField.text is not
                SaveHint();
            }
            ShowHint(1);
        }
    }

    private void SaveHint()
    {
        gameManager.CurrentHint.Text = hintInputField.text;
    }

    private bool CanHintBeSaved(bool isReminderNeeded = true)
    {
        bool canHintBeSaved = true;

        if (hintInputField.text == string.Empty)
        {
            canHintBeSaved = false;

            if (isReminderNeeded)
            {
                saveReminderPanel.SetActive(true);
                saveReminderText.text = "Current hint is empty. Please either add some text or remove the hint.";
            }
        }

        return canHintBeSaved;
    }

    private void ShowHint(int direction)
    {
        if (direction != 1 && direction != -1)
        {
            print("ShowHint's parameter must be either -1 or 1 in order to show previous and next hint respectively.");
            return;
        }

        int currentHintIndex;
        if (gameManager.GameMode == GameMode.PlayMode)
        {
            // RevealedHints are used in PlayMode
            currentHintIndex = gameManager.CurrentTask.RevealedHints.IndexOf(gameManager.CurrentHint);
            currentHintIndex = currentHintIndex + direction;
            if (currentHintIndex < 0)
            {
                currentHintIndex = gameManager.CurrentTask.RevealedHints.Count - 1;
            }
            currentHintIndex %= gameManager.CurrentTask.RevealedHints.Count;
            gameManager.CurrentHint = gameManager.CurrentTask.RevealedHints[currentHintIndex];

            hintText.text = gameManager.CurrentHint.Text;
        }
        else // Creation Mode
        {
            // AllHints are used in CreationMode
            currentHintIndex = gameManager.CurrentTask.AllHints.IndexOf(gameManager.CurrentHint);
            currentHintIndex = currentHintIndex + direction;
            if (currentHintIndex < 0)
            {
                currentHintIndex = gameManager.CurrentTask.AllHints.Count - 1;
            }
            currentHintIndex %= gameManager.CurrentTask.AllHints.Count;
            gameManager.CurrentHint = gameManager.CurrentTask.AllHints[currentHintIndex];

            hintInputField.text = gameManager.CurrentHint.Text;
        }
    }

    private void RefreshHintNavigation()
    {
        if (gameManager.GameMode == GameMode.PlayMode)
        {
            // RevealedHints are used in PlayMode
            if (gameManager.CurrentTask.RevealedHints.Count > 1)
            {
                SetHintNavigationOptions(true);
            }
            else
            {
                // There are no Revealed Hints or there is only 1 Hint
                SetHintNavigationOptions(false);
            }
        }
        else // Creation Mode
        {
            // AllHints are used in CreationMode
            if (gameManager.CurrentTask.AllHints.Count > 1)
            {
                SetHintNavigationOptions(true);
            }
            else
            {
                // There are no Hints at all or there is only 1 Hint
                SetHintNavigationOptions(false);
            }
        }
    }

    private void SetHintNavigationOptions(bool isInteractable)
    {
        previousHint.interactable = isInteractable;
        nextHint.interactable = isInteractable;
    }

    #endregion

    #region - Header related methods -

    public void ChangeHeader(string newTitle)
    {
        if (newTitle == string.Empty)
        {
            // New Title cannot be an empty string
            return;
        }

        MenuItemType currentMenuItemType = gameMenuStack.Peek().MenuItemType;

        switch (currentMenuItemType)
        {
            case MenuItemType.TreasureHunt:
                gameManager.CurrentTreasureHunt.ChangeTitle(newTitle);
                break;
            case MenuItemType.Problem:
                gameManager.CurrentProblem.ChangeTitle(newTitle);
                break;
            case MenuItemType.Task:
                gameManager.CurrentTask.ChangeTitle(newTitle);
                break;            
            default:                
                break;
        }
    }

    private void SetHeader(string header, MenuItemType menuItemType = MenuItemType.TreasureHunt)
    {
        if (gameManager.GameMode == GameMode.PlayMode)
        {
            ActivateHeaderObjects(true);
        }
        else if (menuItemType == MenuItemType.TreasureHunt)
        {
            // Game is in CreationMode but currently a list of all TreasureHunts 
            // is displayed so header cannot be edited
            ActivateHeaderObjects(true);
        }
        else 
        {
            // Header can be edited -> Titles can be changed
            ActivateHeaderObjects(false);
        }

        headerText.text = header;
        headerInputField.text = header;
    }

    /// <summary>
    /// Activates <see cref="headerText"/> and deactivates <see cref="headerInputField"/>
    /// game objects if the specified value is true, or vice versa if it's false.
    /// </summary>
    /// <param name="isActive">True for uneditable header. False for editable header.</param>
    private void ActivateHeaderObjects(bool isActive)
    {
        headerText.gameObject.SetActive(isActive);
        headerInputField.gameObject.SetActive(!isActive);
    }

    #endregion

    public void RemoveMenuItem(MenuItem menuItem)
    {
        print("RemoveMenuItem called");
        MenuItemPool.Instance.ReclaimMenuItem(menuItem);

        switch (menuItem.MenuItemType)
        {
            case MenuItemType.TreasureHunt:
                gameManager.RemoveTreasureHunt(menuItem.TreasureHunt);
                break;
            case MenuItemType.Problem:
                gameManager.RemoveProblem(menuItem.Problem);
                break;
            case MenuItemType.Task:
                gameManager.RemoveTask(menuItem.Task);
                break;
            default:
                break;
        }
    }

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
        gameManager.HintCreated += OnHintCreated;

        gameManager.HintRemoved += OnHintRemoved;
    }

    #region - GameManager events -

    private void OnGameModeChanged()
    {
        bool isInPlayMode = gameManager.GameMode == GameMode.PlayMode;

        // Header
        ActivateHeaderObjects(isInPlayMode);
        
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

    #region - OnCreated Handlers -

    private void OnTreasureHuntCreated()
    {
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = "All Treasure Hunts";
        stackItem.ListOfItems = gameManager.AllTreasureHunts;
        stackItem.MenuItemType = MenuItemType.TreasureHunt;
        gameMenuStack.Push(stackItem);

        UpdateGameMenu(gameManager.CurrentTreasureHunt.Title, gameManager.CurrentTreasureHunt.Problems, MenuItemType.Problem);
    }

    private void OnProblemCreated()
    {
        // Game is already in Creation Mode
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = gameManager.CurrentTreasureHunt.Title;
        stackItem.ListOfItems = gameManager.CurrentTreasureHunt.Problems;
        stackItem.MenuItemType = MenuItemType.Problem;
        gameMenuStack.Push(stackItem);

        UpdateGameMenu(gameManager.CurrentProblem.Title, gameManager.CurrentProblem.Tasks, MenuItemType.Task);        
    }

    private void OnTaskCreated()
    {
        // Game is in Creation Mode
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = gameManager.CurrentProblem.Title;
        stackItem.ListOfItems = gameManager.CurrentProblem.Tasks;
        stackItem.MenuItemType = MenuItemType.Task;
        gameMenuStack.Push(stackItem);

        ActivateTask();
    }

    private void OnHintCreated()
    {
        // No new hints can be added as long as the current one is empty
        addHint.interactable = false;

        hintInputField.text = string.Empty;        
        hintPanel.gameObject.SetActive(true);

        RefreshHintNavigation();
    }

    #endregion

    #region - OnRemoved Handlers -

    private void OnHintRemoved()
    {
        // In case the CurrentHint.Text was just an empty string
        addHint.interactable = true;

        if (gameManager.CurrentTask.AllHints.Count != 0)
        {
            gameManager.CurrentHint = gameManager.CurrentTask.AllHints[0];
            hintInputField.text = gameManager.CurrentHint.Text;
        }
        else
        {
            // Current Task has no more hints
            hintInputField.text = string.Empty;
            hintPanel.gameObject.SetActive(false);
        }

        RefreshHintNavigation();
    }

    #endregion

    #endregion
}

public class GameMenuTuple
{
    public string Title { get; set; }
    public IEnumerable ListOfItems { get; set; }
    public MenuItemType MenuItemType { get; set; }
}

