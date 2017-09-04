using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TreasureHunt;
using System;

[RequireComponent(typeof(TreasureHuntManager), typeof(LocationManager))]
public class GameManager : MonoBehaviour
{
    private GameMode gameMode;

    private TreasureHuntManager treasureHuntManager;
    private LocationManager locationManager;
        
    private Transform currentMenu; // Transform of the currently active menu, like Main menu, Play a game, Options...
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

    [Header("Hint points")]
    public GameObject hintPointsPanel;
    public Text hintPointsText;
    public Text hintPoints;

    [Header("Answer")]
    public RectTransform answerPanel;
    public InputField answerInputField;    
    [Space(10)]
    public RectTransform answerPlayOptions;
    public Button checkAnswer;
    public Button cancelAnswer;
    public RectTransform answerCreationOptions;
    [Space(10)]
    public GameObject taskSolvedPanel;
    public Text taskSolvedText;

    [Header("Advanced options")]
    public GameObject advancedOptionsPanel;
    public GameObject advancedPlayOptions;
    public GameObject advancedCreationOptions;

    [Header("Location options")]
    public Toggle useCurrentLocation;
    public Button checkCoordinates;
    public Button checkPosition;
    public Button stopPositionCheck;
    public InputField latitude;
    public InputField longitude;
    public Slider radiusSlider;

    [Header("Message panel")]
    public GameObject messagePanel;
    public Text messageText;
    public GameObject saveReminderButtons;

    [Header("Back")]
    public RectTransform backPanel;

    [Header("Working Spinner")]
    public GameObject workingSpinnerPanel;
    public Text workingSpinnerText;

    [Header("Password")]
    public GameObject passwordPanel;
    public GameObject passwordFirstSave;
    public GameObject passwordAlreadySet;
    [Space(10)]
    public InputField firstPassword;
    public InputField enterPassword;
    public InputField currentPassword;
    public InputField newPassword;

    [Header("Save location")]
    public Text saveLocation;

    #endregion

    #region - Events -

    private event Action GameModeChanged;

    #endregion

    #region - Properties -

    public GameMode GameMode
    {
        get
        {
            return gameMode;
        }
        set
        {
            if (gameMode != value)
            {
                gameMode = value;

                if (GameModeChanged != null)
                {
                    GameModeChanged();
                }
            }
        }
    }
    public bool IsPasswordEntered { get; set; }

    #endregion

    #region - Event handlers and methods for UI elements -

    #region - Main Menu -

    /// <summary>
    /// Displays the location of the save folder. In case the player wants to play
    /// a treasure hunt created by someone else he needs to put it in that specific folder.
    /// </summary>
    public void ShowSaveFolderLocation()
    {
        saveLocation.text = Constants.persistentDataPath;        
    }

    public void Quit()
    {
        Application.Quit(); 
    }

    /// <summary>
    /// Goes back to the previous menu/game menu.
    /// </summary>
    public void Back()
    {
        if (gameMenuStack.Count != 0)
        {
            GoBackGameMenuStack();
        }
        else
        {
            // Regular menu - Not a Treasure Hunt list or its problems/tasks
            GoBackMenuStack();
        }
    }

    /// <summary>
    /// Player was asked to go back and have his/her task deleted or stay and finish it.
    /// </summary>
    /// <param name="isBackConfirmed">True if the player wants to have the current task deleted and go back. 
    /// False for not going back and finishing the task.</param>
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

    /// <summary>
    /// Going back to the last item in <see cref="gameMenuStack"/>. If the game is in creation mode and a task is active
    /// a check is made to see if it is saved or not before continuing with the 'back' process.
    /// </summary>
    private void GoBackGameMenuStack()
    {
        // In case password panel is active close it when using Back
        if (passwordPanel.activeSelf)
        {
            ClosePasswordPanel();
        }
               
        if (gameMenuStack.Peek().GameItemType == GameItemType.Task)
        {
            // As the last item on the stack is a task, task panel is currently active

            // In CreationMode: A check to see if task, answer or hint input fields are empty
            if (GameMode == GameMode.CreationMode && !CanTaskBeSaved(false))
            {
                // Task is not saved
                messagePanel.SetActive(true); // TODO put this and the next line in a separate method
                messageText.text = Constants.TaskNotSavedGoingBack;
                saveReminderButtons.SetActive(true);

                // Stopping the 'Back' process until the player decides what to do
                return;
            }

            // In case the task solved panel was still visible
            taskSolvedPanel.SetActive(false);

            gameMenu.gameObject.SetActive(true);
            taskPanel.gameObject.SetActive(false);
            answerPanel.gameObject.SetActive(false);
        }

        GameMenuTuple previousGameMenu = gameMenuStack.Pop();
        UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.GameItemType);
    }

    /// <summary>
    /// Deactivates the current menu and activates (and pops) the first one in the <see cref="menuStack"/>.
    /// </summary>
    private void GoBackMenuStack()
    {        
        currentMenu.gameObject.SetActive(false);
        Transform menu = menuStack.Pop();
        UpdateCurrentMenu(menu);
        currentMenu.gameObject.SetActive(true);
    }

    /// <summary>
    /// Player decided to delete the current task and go back.
    /// </summary>
    private void ContinueWithBackMethod()
    {        
        treasureHuntManager.RemoveTask(treasureHuntManager.CurrentTask);

        gameMenu.gameObject.SetActive(true);
        taskPanel.gameObject.SetActive(false);
        answerPanel.gameObject.SetActive(false);

        GameMenuTuple previousGameMenu = gameMenuStack.Pop();
        UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.GameItemType);
    }

    #endregion

    #region - Other Menus -

    /// <summary>
    /// Creates a TreasureHunt.
    /// </summary>
    public void CreateTreasureHunt()
    {
        if (treasureHuntManager.AllTreasureHunts == null)
        {
            // Treasure hunts not yet loaded
            SetHeader(Constants.UnnamedTreasureHunt, GameItemType.Problem);
            StartCoroutine(WaitForPersistenceServiceLoaded());
        }
        else
        {
            // Treasure hunts already loaded (if there were any)            
            treasureHuntManager.CreateTreasureHunt();
        }
    }

    /// <summary>
    /// Displays all treasure hunts to the player.
    /// </summary>
    public void ShowAllTreasureHunts()
    {
        string header = Constants.AllTreasureHunts;
        
        if (treasureHuntManager.AllTreasureHunts == null)
        {
            SetHeader(header);            
            // Loading of saved treasure hunts not finished 
            workingSpinnerPanel.SetActive(true);
            workingSpinnerText.text = Constants.LoadingMessage;
        }
        else
        {
            // Treasure hunts loaded, so display them to the player
            UpdateGameMenu(header, treasureHuntManager.AllTreasureHunts);
        }
    }

    /// <summary>
    /// Updates the game menu with the specified list of items, of the specified type and sets the appropriate events.
    /// </summary>
    /// <param name="header">New header for the game menu.</param>
    /// <param name="listOfItems">List of items to be displayed in the game menu.</param>
    /// <param name="gameItemType">Type of the items displayed.</param>
    private void UpdateGameMenu(string header, IEnumerable listOfItems, GameItemType gameItemType = GameItemType.TreasureHunt)
    {
        SetHeader(header, gameItemType);
        GameItemPool.Instance.ReclaimGameItems();

        // In case problems are displayed this is used
        bool isProblemInteractable = true; 

        foreach (var item in listOfItems)
        {
            GameItem gameItem = GameItemPool.Instance.GetGameItem(gameMenu);
            gameItem.GameItemType = gameItemType;

            string title = ((ITitle)item).Title;
            gameItem.text.text = title;

            if (GameMode == GameMode.CreationMode)
            {
                gameItem.removeButton.SetActive(true);
            }

            Button button = gameItem.GetComponent<Button>();
            // When button is used add the current list of items and their type to the gameMenuStack
            button.onClick.AddListener(() =>
            {
                GameMenuTuple stackItem = new GameMenuTuple();
                //stackItem.Title = header;
                stackItem.ListOfItems = listOfItems;
                stackItem.GameItemType = gameItemType;
                gameMenuStack.Push(stackItem);
            });

            switch (gameItemType) 
            {
                case GameItemType.TreasureHunt:
                    SetGameItemTreasureHunt(item, gameItem, button, header, title);                 
                    break;
                case GameItemType.Problem:
                    SetGameItemProblem(item, gameItem, button, title, ref isProblemInteractable);
                    break;
                case GameItemType.Task:
                    SetGameItemTask(item, gameItem, button);
                    break;
                default:
                    break;
            }                     
        }

        if (GameMode == GameMode.CreationMode)
        {
            UpdateGameMenuCreationMode(gameItemType);
        }
    }

    /// <summary>
    /// Sets the specified GameItem's TreasureHunt and checkMark if in PlayMode. 
    /// Also sets up the event handler for the GameItem's button.
    /// </summary>
    /// <param name="item">Boxed GameItem.</param>
    /// <param name="gameItem">GameItem being set.</param>
    /// <param name="button">Button of the specified GameItem.</param>
    private void SetGameItemTreasureHunt(System.Object item, GameItem gameItem, Button button, string currentHeader, string newHeader)
    {
        gameItem.TreasureHunt = (TreasureHunt.TreasureHunt)item;

        if (GameMode == GameMode.PlayMode)
        {
            // If the TreasureHunt is completed adds a check mark next to it
            gameItem.checkMark.SetActive(gameItem.TreasureHunt.IsCompleted);
        }

        button.onClick.AddListener(() =>
        {
            if (GameMode == GameMode.CreationMode && !IsPasswordEntered)
            {
                // Password not entered
                passwordPanel.SetActive(true);
                if (gameItem.TreasureHunt.Password == null)
                {
                    // Setting up password for the first time
                    passwordFirstSave.SetActive(true);
                    passwordAlreadySet.SetActive(false);
                }
                else
                {
                    // Treasure Hunt already has a password
                    passwordFirstSave.SetActive(false);
                    passwordAlreadySet.SetActive(true);
                }
            }

            // Setting up the appropriate title for the headerText when Back() is used
            gameMenuStack.Peek().Title = currentHeader;
                        
            UpdateGameMenu(newHeader, gameItem.TreasureHunt.Problems, GameItemType.Problem);
            treasureHuntManager.CurrentTreasureHunt = gameItem.TreasureHunt;

            hintPoints.text = treasureHuntManager.CurrentTreasureHunt.StartingHintPoints.ToString();
        });        
    }

    /// <summary>
    /// Sets the specified GameItem's Problem and checkMark if in PlayMode.
    /// Also sets up the event handler for the GameItem's button.
    /// </summary>
    /// <param name="item">Boxed GameItem.</param>
    /// <param name="gameItem">GameItem being set.</param>
    /// <param name="button">Button of the specified GameItem.</param>
    /// <param name="isProblemInteractable">False if the previous Problem is not yet solved, true otherwise.</param>
    private void SetGameItemProblem(System.Object item, GameItem gameItem, Button button, string newHeader, ref bool isProblemInteractable)
    {
        gameItem.Problem = (Problem)item;

        if (GameMode == GameMode.PlayMode)
        {
            gameItem.checkMark.SetActive(gameItem.Problem.IsSolved);
            gameItem.GetComponent<Button>().interactable = isProblemInteractable;

            if (gameItem.Problem.Tasks.Count == 1)
            {
                // TODO Skip showing tasks and just open the one task
            }
        }

        // Sets the variable for the next problem in the list of problems
        isProblemInteractable = gameItem.Problem.IsSolved;

        button.onClick.AddListener(() =>
        {
            // Setting up the appropriate title for the headerText when Back() is used
            gameMenuStack.Peek().Title = treasureHuntManager.CurrentTreasureHunt.Title;

            UpdateGameMenu(newHeader, gameItem.Problem.Tasks, GameItemType.Task);
            treasureHuntManager.CurrentProblem = gameItem.Problem;

            hintPoints.text = treasureHuntManager.CurrentProblem.HintPoints.ToString();
        });
    }

    /// <summary>
    /// Sets the specified GameItem's Task and checkMark if in PlayMode.
    /// Also sets up the event handler for the GameItem's button.
    /// </summary>
    /// <param name="item">Boxed GameItem.</param>
    /// <param name="gameItem">GameItem being set.</param>
    /// <param name="button">Button of the specified GameItem.</param>
    private void SetGameItemTask(System.Object item, GameItem gameItem, Button button)
    {
        gameItem.Task = (Task)item;

        if (GameMode == GameMode.PlayMode)
        {            
            gameItem.checkMark.SetActive(gameItem.Task.IsSolved);
        }

        button.onClick.AddListener(() =>
        {
            // Setting up the appropriate title for the headerText when Back() is used
            gameMenuStack.Peek().Title = treasureHuntManager.CurrentProblem.Title;

            treasureHuntManager.CurrentTask = gameItem.Task;
            ActivateTask();
        });        
    }

    /// <summary>
    /// Adds and appropriate Add button depending on the specified game item type. Add TreasureHunt/Problem/Task.
    /// </summary>
    /// <param name="gameItemType">The type of the game items currently being displayed.</param>
    private void UpdateGameMenuCreationMode(GameItemType gameItemType)
    {
        // Adding an Add button for TreasureHunt/Problem/Task
        switch (gameItemType)
        {
            case GameItemType.TreasureHunt:
                // A list of treasure hunts is shown
                hintPointsPanel.SetActive(false); // in case the previous game menu was a TreasureHunt
                IsPasswordEntered = false;

                GameItemPool.Instance.GetAddTreasureHunt(gameMenu);
                break;

            case GameItemType.Problem:
                // Treasure hunt is displayed with its list of problems
                hintPointsPanel.SetActive(true);
                hintPointsText.text = Constants.TreasureHuntStartingHintPoints;

                if (treasureHuntManager.CurrentTreasureHunt != null)
                {
                    // Going back from a problem
                    hintPoints.text = treasureHuntManager.CurrentTreasureHunt.StartingHintPoints.ToString();
                }

                GameItemPool.Instance.GetAddProblem(gameMenu);
                break;

            case GameItemType.Task:
                // Problem is displayed with its list of tasks
                hintPointsPanel.SetActive(true);
                hintPointsText.text = Constants.ProblemRewardHintPoints;

                if (treasureHuntManager.CurrentProblem != null)
                {
                    // Going back from a task
                    hintPoints.text = treasureHuntManager.CurrentProblem.HintPoints.ToString();
                }

                GameItemPool.Instance.GetAddTask(gameMenu);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Player tried to create a new treasure hunt before the process of loading all treasure hunts was completed.
    /// So a message is displayed until loading is finished and then <see cref="CreateTreasureHunt"/> method is called.
    /// </summary>
    private IEnumerator WaitForPersistenceServiceLoaded()
    {        
        workingSpinnerPanel.SetActive(true);
        workingSpinnerText.text = Constants.LoadingMessage;

        while (treasureHuntManager.AllTreasureHunts == null)
        {
            yield return null;
        }

        // Treasure hunts loaded
        workingSpinnerPanel.SetActive(false);
        treasureHuntManager.CreateTreasureHunt();
    }

    #endregion

    #region - Menu Transform change => Adding menu to stack and updating current menu - 

    /// <summary>
    /// The specified menu is deactivated so it's added to the <see cref="menuStack"/>.
    /// </summary>
    /// <param name="menu">Transform to be added to the menuStack.</param>
    public void AddMenuToStack(Transform menu)
    {
        menuStack.Push(menu);

        if (menu.name == Constants.MainMenuName)
        {
            // Main menu is deactivated so the back button is necessary
            backPanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Sets the specified menu to be the new <see cref="currentMenu"/> and sets the <see cref="headerText"/> text to
    /// the currentMenu's name. Hides back panel if the current menu is the main menu.
    /// </summary>
    /// <param name="menu">Menu to become the current menu.</param>
    public void UpdateCurrentMenu(Transform menu)
    {
        currentMenu = menu;
        headerText.text = currentMenu.name; 
        if (currentMenu.name == Constants.GameMenuName)
        {
            // TODO Header needs to be set to the appropriate text            
        }

        if (currentMenu.name == Constants.MainMenuName)
        {
            // If main menu is active back button is not necessary
            backPanel.gameObject.SetActive(false);
        }
    }

    #endregion

    #region - Task related methods - 
    // TODO answerInputField Placeholder needs to be different depending on the GameMode, or changed to something more appropriate if it's the same for both

    /// <summary>
    /// Checks the text from the answer input field. 
    /// </summary>
    public void CheckAnswer()
    {
        string possibleTextSolution = answerInputField.text;

        treasureHuntManager.CheckTextSolution(possibleTextSolution);
                
        if (treasureHuntManager.CurrentTask.IsSolved == false)
        {
            // Wrong answer was given, task is not solved            
            StartCoroutine(ChangeInputFieldColor(answerInputField,Color.red));            
        }
    }   

    /// <summary>
    /// Saves the task if it can be saved.
    /// </summary>
    public void SaveTask()
    {
        bool canTaskBeSaved = CanTaskBeSaved();

        if (canTaskBeSaved)
        {            
            if (treasureHuntManager.CurrentHint != null)
            {
                SaveHint();
            }

            addHint.interactable = true;
            treasureHuntManager.CurrentTask.TextClue = taskInputField.text;
            string textSolution = answerInputField.text;

            if (latitude.text != string.Empty && longitude.text != string.Empty)
            {
                // Both location and text are valid solutions
                Location location = new Location(float.Parse(latitude.text), float.Parse(longitude.text), radiusSlider.value);
                treasureHuntManager.CurrentTask.Solution = new Solution(textSolution, location);
            }
            else
            {
                // There is only a text solution
                treasureHuntManager.CurrentTask.Solution.TextSolution = textSolution;
            }

            treasureHuntManager.SaveTreasureHunt();
        }
        else
        {
            // Task cannot be saved at the moment
            return;
        }
    }

    /// <summary>
    /// Clears the answer input field in play mode.
    /// </summary>
    public void CancelPlayModeTask()
    {
        answerInputField.text = string.Empty;
    }

    /// <summary>
    /// Resets the ui elements responsible for holding the current task's properties.
    /// </summary>
    public void CancelCreationModeTask()
    {
        taskInputField.text = treasureHuntManager.CurrentTask.TextClue;
        if (treasureHuntManager.CurrentHint != null)
        {
            hintInputField.text = treasureHuntManager.CurrentHint.Text;
        }

        answerInputField.text = treasureHuntManager.CurrentTask.Solution.TextSolution;        
    }

    /// <summary>
    /// Activates the <see cref="taskSolvedPanel"/> for the specified amount of seconds and shows a 
    /// message to the player that he/she has solved the Task.
    /// </summary>
    /// <param name="seconds">Amount of time that the message will be displayed to the player.</param>
    /// <param name="isLocationReached">True if the Task is solved by reaching the target location, false otherwise.</param>
    /// <returns></returns>
    private IEnumerator ShowTaskSolvedPanel(float seconds, bool isLocationReached = false)
    {
        taskSolvedPanel.SetActive(true);
        if (isLocationReached)
        {
            // Task solved by reaching the target destination 
            taskSolvedText.text = Constants.TaskSolvedByLocation;
        }
        else
        {
            // Task solved by text
            taskSolvedText.text = Constants.TaskSolvedByText;
        }

        if (treasureHuntManager.CurrentProblem.IsSolved)
        {
            // All Tasks in the problem are solved, so the player is awarded with some hint points. The message shows how much hint points he/she has won
            taskSolvedText.text += "\n\r" + Constants.HintPointsRewardedFirstPart + treasureHuntManager.CurrentProblem.HintPoints + Constants.HintPointsRewardedSecondPart;
        }

        while (seconds > 0)
        {
            seconds -= Time.deltaTime;
            yield return null;
        }

        taskSolvedPanel.SetActive(false);
    }

    /// <summary>
    /// Changes the color of the given input field to the specified new color. 
    /// The color flashes from white to the specified color over 1 second and returns to white.
    /// </summary>
    /// <param name="inputField">Input field whose color is being changed.</param>
    private IEnumerator ChangeInputFieldColor(InputField inputField, Color newColor)
    {
        float timeInSeconds = 0;
        Image image = inputField.GetComponent<Image>();

        while (timeInSeconds < 1)
        {
            image.color = Color.Lerp(Color.white, newColor, Mathf.Sin(Mathf.PI * timeInSeconds));
            timeInSeconds += Time.deltaTime;
            yield return null;
        }
        image.color = Color.white;
    }

    /// <summary>
    /// Hides game menu and displays the current task in the task panel. Depending on which game mode is active
    /// activates different ui elements with different values.
    /// </summary>
    private void ActivateTask()
    {
        SetHeader(treasureHuntManager.CurrentTask.Title, GameItemType.Task);

        // In creation mode this was active so it needs to be set to false
        hintPointsPanel.SetActive(false); 

        // Show task and answer panels and hide game menu
        gameMenu.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        answerPanel.gameObject.SetActive(true);

        if (GameMode == GameMode.PlayMode)
        {
            ActivateTaskPlayMode();
        }
        else 
        {
            ActivateTaskCreationMode();
        }
    }

    /// <summary>
    /// Sets the appropriate ui elements to display the task in play mode.
    /// </summary>
    private void ActivateTaskPlayMode()
    {
        taskText.text = treasureHuntManager.CurrentTask.TextClue;

        if (treasureHuntManager.CurrentTask.RevealedHints.Count == 0)
        {
            // There are no revealed hints for this task
            hintPanel.gameObject.SetActive(false);
        }
        else
        {
            // There are revealed hints for this task
            RefreshHintNavigation();

            hintPanel.gameObject.SetActive(true);
            treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.RevealedHints[0];
            hintText.text = treasureHuntManager.CurrentHint.Text;
        }

        if (treasureHuntManager.CurrentTask.IsSolved)
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

    /// <summary>
    /// Sets the appropriate ui elements to display the task in creation mode.
    /// </summary>
    private void ActivateTaskCreationMode()
    {
        taskInputField.text = treasureHuntManager.CurrentTask.TextClue;
        addHint.interactable = true;

        if (treasureHuntManager.CurrentTask.AllHints.Count == 0)
        {
            // No hints for this task yet
            hintPanel.gameObject.SetActive(false);
        }
        else
        {
            // Task has hints
            RefreshHintNavigation();

            hintPanel.gameObject.SetActive(true);
            treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.AllHints[0];
            hintInputField.text = treasureHuntManager.CurrentHint.Text;
        }

        answerInputField.text = treasureHuntManager.CurrentTask.Solution.TextSolution;
        answerInputField.interactable = true;
    }

    /// <summary>
    /// Displays information about the solved task in the appropriate ui game objects.
    /// Disables interactivity of some of the buttons and input fields.
    /// </summary>
    private void DisplaySolvedTask()
    {
        answerInputField.text = treasureHuntManager.CurrentTask.Solution.TextSolution; 
        if (treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            latitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Latitude.ToString();
            longitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Longitude.ToString();
            radiusSlider.value = treasureHuntManager.CurrentTask.Solution.LocationSolution.Radius;
        }
        answerInputField.interactable = false;
        checkAnswer.interactable = false;
        cancelAnswer.interactable = false;

        checkCoordinates.interactable = false;
        checkPosition.interactable = false;

        if (treasureHuntManager.CurrentTask.RevealedHints.Count != 0)
        {
            hintText.text = treasureHuntManager.CurrentTask.RevealedHints[0].Text;
        }
    }

    /// <summary>
    /// When task is solved calls a SolveTask() method from the treasure hunt manager, gives a visual display
    /// that the task is solved and displays all the information about the solved task in the appropriate ui game objects.
    /// </summary>
    /// <param name="isLocationReached">True if the task is solved by reaching the target location, 
    /// false if it was solved with a text answer.</param>
    private void OnTaskSolved(bool isLocationReached = false)
    {
        treasureHuntManager.SolveTask();
        StartCoroutine(ChangeInputFieldColor(answerInputField, Color.green));
        StartCoroutine(ShowTaskSolvedPanel(7, isLocationReached));
        DisplaySolvedTask();

        if (treasureHuntManager.CurrentTreasureHunt.IsCompleted)
        {
            // TODO Signal to the player that the treasure hunt is completed
        }
    }

    /// <summary>
    /// A check to see if the task can be saved or if some important input fields are empty.
    /// </summary>
    /// <param name="isReminderNeeded">True if the player needs to be reminded that save is not possible, false otherwise.</param>
    /// <returns>True if the task can be saved, false if it cannot.</returns>
    private bool CanTaskBeSaved(bool isReminderNeeded = true)
    {
        bool canTaskBeSaved = false;
        string saveMessage = string.Empty;

        if (taskInputField.text == string.Empty)
        {
            // Text clue is missing
            saveMessage = Constants.EmptyTaskClueSave;
        }
        else if (answerInputField.text == string.Empty)
        {
            // Answer is missing
            messagePanel.SetActive(true);
            saveMessage = Constants.EmptyAnswerSave;
        }
        else if (treasureHuntManager.CurrentHint != null && !CanHintBeSaved(false))
        {
            // Hint is empty
            saveMessage = Constants.EmptyHintSave;
        }
        else
        {
            canTaskBeSaved = true;
        }

        if (!canTaskBeSaved && isReminderNeeded)
        {
            messagePanel.SetActive(true);
            messageText.text = saveMessage;
        }

        return canTaskBeSaved;
    }

    #endregion

    #region - Hint related methods -

    /// <summary>
    /// Shows previous hint.
    /// </summary>
    public void ShowPreviousHint()
    {
        if (GameMode == GameMode.PlayMode)
        {
            ShowHint(-1);
        }
        else if (CanHintBeSaved()) // Creation mode
        {
            if (treasureHuntManager.CurrentHint.Text == string.Empty)
            {
                // Current hint is empty, but hintInputField.text is not
                SaveHint();

                // Since the hint is saved without using the Save button, AddHint button needs to be interactable again
                addHint.interactable = true;
            }
            ShowHint(-1);
        }
    }

    /// <summary>
    /// Shows next hint.
    /// </summary>
    public void ShowNextHint()
    {
        if (GameMode == GameMode.PlayMode)
        {
            ShowHint(1);
        }
        else if (CanHintBeSaved()) // Creation mode 
        {
            if (treasureHuntManager.CurrentHint.Text == string.Empty)
            {
                // Current hint is empty, but hintInputField.text is not
                SaveHint();

                // Since the hint is saved without using the Save button, AddHint button needs to be interactable again
                addHint.interactable = true;
            }
            ShowHint(1);
        }
    }

    /// <summary>
    /// Checks if the text is empty space and if it is disables the addHint button, otherwise addHint.interactable is set to true.
    /// </summary>
    public void OnHintValueChanged(string text)
    {
        text = text.Trim();
        if (text != string.Empty)
        {
            addHint.interactable = true;
        }
        else
        {
            // New hint cannot be added if the current one is empty
            addHint.interactable = false;
        }
    }

    /// <summary>
    /// Saves the hintInputField.text to the current hint and saves the treasure hunt.
    /// </summary>
    private void SaveHint()
    {
        treasureHuntManager.CurrentHint.Text = hintInputField.text;
        treasureHuntManager.SaveTreasureHunt();
    }

    /// <summary>
    /// A check to see if the hint can be saved or not. Reminds the player that he cannot save an empty
    /// hint if the reminder is needed.
    /// </summary>
    /// <returns>True if the hint can be saved, false otherwise.</returns>
    private bool CanHintBeSaved(bool isReminderNeeded = true)
    {
        bool canHintBeSaved = true;

        if (hintInputField.text == string.Empty)
        {
            canHintBeSaved = false;

            if (isReminderNeeded)
            {
                messagePanel.SetActive(true);
                messageText.text = Constants.EmptyHintSave;
            }
        }

        return canHintBeSaved;
    }

    /// <summary>
    /// Shows next or previous hint depending on the specified direction.
    /// </summary>
    /// <param name="direction">1 for the next hint and -1 for the previous hint.</param>
    private void ShowHint(int direction)
    {
        if (direction != 1 && direction != -1)
        {
            print("ShowHint's parameter must be either -1 or 1 in order to show previous and next hint respectively.");
            return;
        }

        if (GameMode == GameMode.PlayMode)
        {
            ShowHintPlayMode(direction);
        }
        else 
        {
            ShowHintCreationMode(direction);
        }
    }

    /// <summary>
    /// Shows next or previous hint in play mode, depending on the specified direction.
    /// </summary>
    /// <param name="direction">1 for the next hint and -1 for the previous hint.</param>
    private void ShowHintPlayMode(int direction)
    {
        // RevealedHints are used in play mode
        int currentHintIndex = treasureHuntManager.CurrentTask.RevealedHints.IndexOf(treasureHuntManager.CurrentHint);
        currentHintIndex = currentHintIndex + direction;

        if (currentHintIndex < 0)
        {
            currentHintIndex = treasureHuntManager.CurrentTask.RevealedHints.Count - 1;
        }

        currentHintIndex %= treasureHuntManager.CurrentTask.RevealedHints.Count;
        treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.RevealedHints[currentHintIndex];

        hintText.text = treasureHuntManager.CurrentHint.Text;
    }

    /// <summary>
    /// Shows next or previous hint in creation mode, depending on the specified direction.
    /// </summary>
    /// <param name="direction">1 for the next hint and -1 for the previous hint.</param>
    private void ShowHintCreationMode(int direction)
    {
        // AllHints are used in creation mode
        int currentHintIndex = treasureHuntManager.CurrentTask.AllHints.IndexOf(treasureHuntManager.CurrentHint);
        currentHintIndex = currentHintIndex + direction;

        if (currentHintIndex < 0)
        {
            currentHintIndex = treasureHuntManager.CurrentTask.AllHints.Count - 1;
        }

        currentHintIndex %= treasureHuntManager.CurrentTask.AllHints.Count;
        treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.AllHints[currentHintIndex];

        hintInputField.text = treasureHuntManager.CurrentHint.Text;
    }

    /// <summary>
    /// Enables or disables <see cref="previousHint"/> and <see cref="nextHint"/>  buttons depending on 
    /// the play mode and the count of revealed hints or the count of all hints.
    /// </summary>
    private void RefreshHintNavigation()
    {
        if (GameMode == GameMode.PlayMode)
        {
            // RevealedHints are used in play mode
            if (treasureHuntManager.CurrentTask.RevealedHints.Count > 1)
            {
                SetHintNavigationOptions(true);
            }
            else
            {
                // There are no revealed hints or there is only 1 hint
                SetHintNavigationOptions(false);
            }
        }
        else // Creation mode
        {
            // AllHints are used in creation mode
            if (treasureHuntManager.CurrentTask.AllHints.Count > 1)
            {
                SetHintNavigationOptions(true);
            }
            else
            {
                // There are no hints at all or there is only 1 Hint
                SetHintNavigationOptions(false);
            }
        }
    }

    /// <summary>
    /// Sets interactability of <see cref="previousHint"/> and <see cref="nextHint"/> buttons to the specified bool value.
    /// </summary>
    private void SetHintNavigationOptions(bool isInteractable)
    {
        previousHint.interactable = isInteractable;
        nextHint.interactable = isInteractable;
    }

    #endregion

    #region - Hints points related methods -

    /// <summary>
    /// Changes starting hint points of a treasure hunt or reward hint points of a problem by the specified amount.
    /// </summary>
    public void ChangeHintPoints(int changeBy)
    {
        int points;

        if (int.TryParse(hintPoints.text, out points))
        {
            points += changeBy;
            points = Mathf.Clamp(points, Constants.MinHintPoints, Constants.MaxHintPoints);
            hintPoints.text = points.ToString();

            GameItemType gameItemType = gameMenuStack.Peek().GameItemType;
            if (gameItemType == GameItemType.TreasureHunt)
            {
                treasureHuntManager.ChangeTreasureHuntHintPoints(points);
            }
            else if (gameItemType == GameItemType.Problem)
            {
                treasureHuntManager.ChangeProblemHintPoints(points);
            }
        }
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

        GameItemType currentGameItemType = gameMenuStack.Peek().GameItemType;
        string oldTitle = treasureHuntManager.CurrentTreasureHunt.Title;

        switch (currentGameItemType)
        {
            case GameItemType.TreasureHunt:
                if (treasureHuntManager.ContainsTreasureHuntTitle(newTitle))
                {
                    messagePanel.SetActive(true);
                    messageText.text = Constants.SameNameTreasureHunt;
                    SetHeader(oldTitle, GameItemType.Problem);
                    return;
                }
                treasureHuntManager.CurrentTreasureHunt.ChangeTitle(newTitle);
                break;
            case GameItemType.Problem:
                treasureHuntManager.CurrentProblem.ChangeTitle(newTitle);
                break;
            case GameItemType.Task:
                treasureHuntManager.CurrentTask.ChangeTitle(newTitle);
                break;            
            default:                
                break;
        }

        if (currentGameItemType == GameItemType.TreasureHunt && (newTitle != oldTitle))
        {
            // Treasure Hunt's title is changed and it's different than the old one
            treasureHuntManager.SaveTreasureHunt(oldTitle);
        }
        else
        {
            treasureHuntManager.SaveTreasureHunt();
        }        
    }

    /// <summary>
    /// Sets header to the specified parameter and enables or disables header eiditing
    /// based on the game mode and the specified game item type.
    /// </summary>
    /// <param name="header">New header.</param>
    /// <param name="gameItemType">Type of the game item whose header is being changed.</param>
    private void SetHeader(string header, GameItemType gameItemType = GameItemType.TreasureHunt)
    {
        if (GameMode == GameMode.PlayMode || gameItemType == GameItemType.TreasureHunt)
        {
            // Game is either in PlayMode or in CreationMode but currently a list of all TreasureHunts 
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

    #region - Advanced options related methods - 

    /// <summary>
    /// Shows advanced options to the player for both creation and play modes.
    /// </summary>
    public void ShowAdvancedOptions()
    {
        if (GameMode == GameMode.CreationMode && treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            ShowSolvedLocationOptions();
        }
        else // Play mode
        {
            if (treasureHuntManager.CurrentTask.IsSolved)
            {
                ShowSolvedLocationOptions();
                SetCoordinatesInteractable(false);
            }
            else
            {
                // Task is not solved so reset location options
                ResetPlayModeAdvancedOptions();
                IgnoreLocation();
            }
        }
    }

    /// <summary>
    /// Closes advanced options. Stops location service if it was still active.
    /// </summary>
    public void CloseAdvancedOptions()
    {
        // In case it was used on the first openning of the advanced options
        useCurrentLocation.isOn = false;

        locationManager.StopLocationService();
    }

    /// <summary>
    /// Sets the interactable property on some buttons and input fields in advanced options panel
    /// based on whether the current task is solved or not.
    /// </summary>
    private void ResetPlayModeAdvancedOptions()
    {
        bool isSolved = treasureHuntManager.CurrentTask.IsSolved;

        // If the Task is solved buttons and input fields are not interactable
        // If the Task is not solved they are interactable
        SetCoordinatesInteractable(!isSolved);
        checkCoordinates.interactable = !isSolved;
        checkPosition.interactable = !isSolved;
        
        stopPositionCheck.interactable = false;
    }

    /// <summary>
    /// Shows latitude, longitude and the target location radius for the solved task.
    /// </summary>
    private void ShowSolvedLocationOptions()
    {
        latitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Latitude.ToString();
        longitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Longitude.ToString();
        radiusSlider.value = treasureHuntManager.CurrentTask.Solution.LocationSolution.Radius;
    }

    #endregion

    #region - Location related methods - 

    /// <summary>
    /// Calls or stops UseCurrentLocation method of the location manager based on the specified parameter.
    /// </summary>
    /// <param name="shouldUseCurrentLocation">True if current location should be used, false for stopping that process.</param>
    public void UseCurrentLocation(bool shouldUseCurrentLocation)
    {
        locationManager.UseCurrentLocation(shouldUseCurrentLocation);
    }

    /// <summary>
    /// Resets radius slider and clears latitude and longitude input fields.
    /// </summary>
    public void IgnoreLocation()
    {
        latitude.text = string.Empty;
        longitude.text = string.Empty;
        radiusSlider.value = Constants.DefaultLocationRadius;
    }

    /// <summary>
    /// If the current task has a location solution checks the inputted coordinates to see if they are withit the target radius.
    /// Displays an appropriate message if the task has no location solution.
    /// </summary>
    public void CheckCoordinates()
    {
        if (!treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            // The current task has no location solution
            ReportNoLocationSolution();
            return;
        }

        if (latitude.text != string.Empty && longitude.text != string.Empty)
        {
            // Some location coordinates entered
            bool areCoordinatesCorrect = locationManager.AreCoordinatesInTargetRadius(treasureHuntManager.CurrentTask.Solution.LocationSolution);

            if (!areCoordinatesCorrect)
            {
                StartCoroutine(ChangeInputFieldColor(latitude, Color.red));
                StartCoroutine(ChangeInputFieldColor(longitude, Color.red));
            }
        }
        else
        {
            // One or both coordinates are missing values, so do nothing
            //  TODO perhaps disable the check coordinates button unless both latitude and longitude have values
        }
    }

    /// <summary>
    /// In play mode checks the current position of the player if the current task has a location solution.
    /// Displays an appropriate message if the task has no location solution.
    /// </summary>
    public void CheckPosition()
    {
        if (!treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            // The current task has no location solution
            ReportNoLocationSolution();
            ResetPlayModeAdvancedOptions();

            checkPosition.interactable = true;
            return;
        }
        else
        {
            // Task has Location as a Solution
            SetCoordinatesInteractable(false);  
            // In case the location service doesn't start SetCoordinatesInteractable(true) is called 
            // from an appropriate event handler, so locationManager.CheckPosition needs to be called after SetCoordinatesInteractable(false)
            locationManager.CheckPosition(treasureHuntManager.CurrentTask.Solution.LocationSolution);
        }

    }

    /// <summary>
    /// Sets interactable property of latitude and longitude input fields to the specified parameter.
    /// </summary>
    public void SetCoordinatesInteractable(bool isInteractable)
    {
        latitude.interactable = isInteractable;
        longitude.interactable = isInteractable;
    }

    /// <summary>
    /// Displays an appropriate message that the task has no location solution.
    /// </summary>
    private void ReportNoLocationSolution()
    {
        messagePanel.SetActive(true);
        messageText.text = Constants.NoLocationSolution;
    }

    #endregion

    #region - Password related methods -
    
    /// <summary>
    /// Saves treasure hunt's password if it is valid.
    /// </summary>
    public void SavePassword()
    {
        if (firstPassword.text.Trim() == string.Empty)
        {
            messagePanel.SetActive(true);
            messageText.text = Constants.EmptyPassword;        
        }
        else
        {
            // Password is valid. Saving the password
            SavePassword(firstPassword.text);
        }
    }

    /// <summary>
    /// Checks if the entered password is the correct password for the chosen treasure hunt.
    /// </summary>
    public void EnterPassword()
    {
        if (enterPassword.text != treasureHuntManager.CurrentTreasureHunt.Password)
        {
            // Wrong password entered
            StartCoroutine(ChangeInputFieldColor(enterPassword, Color.red));
        }
        else
        {
            // Correct password entered
            IsPasswordEntered = true;
            ClosePasswordPanel();
        }
    }

    /// <summary>
    /// Changes treasure hunt's password.
    /// </summary>
    public void ChangePassword()
    {
        if (currentPassword.text != treasureHuntManager.CurrentTreasureHunt.Password)
        {
            // Wrong current password entered
            StartCoroutine(ChangeInputFieldColor(currentPassword, Color.red));
        }
        else if (newPassword.text.Trim() == string.Empty)
        {
            // New password not valid - either empty spaces or just empty
            messagePanel.SetActive(true);
            messageText.text = Constants.EmptyPassword;
        }
        else
        {
            // Changing the password
            SavePassword(newPassword.text);
        }
    }

    /// <summary>
    /// Saves the treasure hunt with the specified password and closes the password panel.
    /// </summary>
    /// <param name="password">New password for the current treasure hunt.</param>
    private void SavePassword(string password)
    {
        IsPasswordEntered = true;
        treasureHuntManager.CurrentTreasureHunt.Password = password;
        treasureHuntManager.SaveTreasureHunt();

        ClosePasswordPanel();
    }

    /// <summary>
    /// Deactivates the password panel and clears all password input fields.
    /// </summary>
    private void ClosePasswordPanel()
    {
        passwordPanel.SetActive(false);

        firstPassword.text = string.Empty;
        enterPassword.text = string.Empty;
        currentPassword.text = string.Empty;
        newPassword.text = string.Empty;
    }

    #endregion

    /// <summary>
    /// Removes the game item and returns it to the game item pool after 
    /// the player deleted a task, problem or a treasure hunt.
    /// </summary>
    /// <param name="gameItem">Game item which held a task, problem or a treasure hunt that has been deleted.</param>
    public void RemoveGameItem(GameItem gameItem)
    {
        GameItemPool.Instance.ReclaimGameItem(gameItem);

        switch (gameItem.GameItemType)
        {
            case GameItemType.TreasureHunt:
                treasureHuntManager.RemoveTreasureHunt(gameItem.TreasureHunt);
                break;
            case GameItemType.Problem:
                treasureHuntManager.RemoveProblem(gameItem.Problem);
                break;
            case GameItemType.Task:
                treasureHuntManager.RemoveTask(gameItem.Task);
                break;
            default:
                break;
        }
    }   

    #endregion

    private void Start()
    {
        treasureHuntManager = GetComponent<TreasureHuntManager>();
        locationManager = GetComponent<LocationManager>();

        currentMenu = mainMenu;

        GameMode = GameMode.PlayMode;
        GameModeChanged += OnGameModeChanged;
        
        menuStack = new Stack<Transform>();
        gameMenuStack = new Stack<GameMenuTuple>();

        AddTreasureHuntManagerHandlers();
        AddLocationManagerHandlers();        
    }

    /// <summary>
    /// Adds event handlers for treasure hunt manager's events.
    /// </summary>
    private void AddTreasureHuntManagerHandlers()
    {
        treasureHuntManager.TreasureHuntsLoaded += TreasureHuntsLoaded;
        treasureHuntManager.TreasureHuntSaveStarted += TreasureHuntSaveStarted;
        treasureHuntManager.TreasureHuntSaved += TreasureHuntSaved;

        treasureHuntManager.TreasureHuntCreated += OnTreasureHuntCreated;
        treasureHuntManager.ProblemCreated += OnProblemCreated;
        treasureHuntManager.TaskCreated += OnTaskCreated;
        treasureHuntManager.HintCreated += OnHintCreated;

        treasureHuntManager.HintRemoved += OnHintRemoved;
        treasureHuntManager.HintRevealed += OnHintRevealed;

        treasureHuntManager.TaskSolved += () => OnTaskSolved();
    }

    /// <summary>
    /// Adds event handlers for location manager's events.
    /// </summary>
    private void AddLocationManagerHandlers()
    {
        locationManager.StartingLocationService += StartingLocationService;
        locationManager.LocationDisabledByUser += LocationDisabledByUser;
        locationManager.LocationServiceNotStarted += LocationServiceNotStarted;
        locationManager.LocationServiceStarted += LocationServiceStarted;

        locationManager.TargetLocationReached += TargetLocationReached;
    }

    #region - Switch Game Modes - 

    /// <summary>
    /// Sets game mode to play mode.
    /// </summary>
    public void GoToPlayMode()
    {
        GameMode = GameMode.PlayMode;
    }

    /// <summary>
    /// Sets game mode to creation mode.
    /// </summary>
    public void GoToCreationMode()
    {
        GameMode = GameMode.CreationMode;
    }

    #endregion

    #region - TreasureHuntManager events -

    /// <summary>
    /// Displays a working spinner pannel with a message to the player that the treasure hunt is being saved.
    /// </summary>
    private void TreasureHuntSaveStarted()
    {
        workingSpinnerPanel.SetActive(true);
        workingSpinnerText.text = Constants.SavingMessage;
    }

    /// <summary>
    /// If working spinner panel was active hides it because all treasure hunts have been loaded.
    /// </summary>
    private void TreasureHuntsLoaded()
    {
        treasureHuntManager.TreasureHuntsLoaded -= TreasureHuntsLoaded;

        if (workingSpinnerPanel.activeSelf)
        {
            // Player already waiting for all treasure hunts
            workingSpinnerPanel.SetActive(false);
            UpdateGameMenu(Constants.AllTreasureHunts, treasureHuntManager.AllTreasureHunts);
        }
    }

    /// <summary>
    /// Disables the working spinner panel because the treasure hunt has been saved and the player can
    /// continue playing the game now.
    /// </summary>
    private void TreasureHuntSaved()
    {
        workingSpinnerPanel.SetActive(false);
    }

    /// <summary>
    /// Activates and deactivates some ui game objects based on the current play mode.
    /// </summary>
    private void OnGameModeChanged()
    {
        bool isInPlayMode = GameMode == GameMode.PlayMode;

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

        // Advanced answer
        advancedPlayOptions.SetActive(isInPlayMode);
        advancedCreationOptions.SetActive(!isInPlayMode);
    }

    /// <summary>
    /// Shows hint panel with the revealed hint and calls <see cref="RefreshHintNavigation"/> .
    /// </summary>
    private void OnHintRevealed()
    {
        hintPanel.gameObject.SetActive(true);
        hintText.text = treasureHuntManager.CurrentHint.Text;

        RefreshHintNavigation();
    }

    #region - OnCreated Handlers -

    /// <summary>
    /// After a treasure hunt has been created adds an "All Treasure Hunts" tuple to the game menu stack, shows the password panel
    /// and updates the game menu to display the newly created treasure hunt.
    /// </summary>
    private void OnTreasureHuntCreated()
    {
        // Adding "All Treasure Hunts" to the game menu stack so that going back from the new treasure hunt gets the player to this list
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = Constants.AllTreasureHunts;
        stackItem.ListOfItems = treasureHuntManager.AllTreasureHunts;
        stackItem.GameItemType = GameItemType.TreasureHunt;
        gameMenuStack.Push(stackItem);

        // Setting up a new password for the newly created Treasure Hunt
        IsPasswordEntered = false;
        passwordPanel.SetActive(true);
        passwordFirstSave.SetActive(true);
        passwordAlreadySet.SetActive(false);

        UpdateGameMenu(treasureHuntManager.CurrentTreasureHunt.Title, treasureHuntManager.CurrentTreasureHunt.Problems, GameItemType.Problem);
    }

    /// <summary>
    /// After a problem has been created adds the current treasure hunt to the game menu stack and updates the game menu
    /// to display the newly created problem.
    /// </summary>
    private void OnProblemCreated()
    {
        // Adding the current treasure hunt with its list of problems to the game menu stack
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = treasureHuntManager.CurrentTreasureHunt.Title;
        stackItem.ListOfItems = treasureHuntManager.CurrentTreasureHunt.Problems;
        stackItem.GameItemType = GameItemType.Problem;
        gameMenuStack.Push(stackItem);

        UpdateGameMenu(treasureHuntManager.CurrentProblem.Title, treasureHuntManager.CurrentProblem.Tasks, GameItemType.Task);        
    }

    /// <summary>
    /// After a task has been created adds the current problem to the game menu stack and shows the task panel
    /// for the newly created task.
    /// </summary>
    private void OnTaskCreated()
    {
        // Adding the current problem with its list of tasks to the game menu stack
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = treasureHuntManager.CurrentProblem.Title;
        stackItem.ListOfItems = treasureHuntManager.CurrentProblem.Tasks;
        stackItem.GameItemType = GameItemType.Task;
        gameMenuStack.Push(stackItem);

        ActivateTask();
    }

    /// <summary>
    /// After a hint has been created sets certain properties of some ui game object responsible for hints.
    /// </summary>
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

    /// <summary>
    /// After a hint has been removed sets certain properties of some ui game object responsible for hints.
    /// </summary>
    private void OnHintRemoved()
    {
        // In case the CurrentHint.Text was just an empty string
        addHint.interactable = true;

        if (treasureHuntManager.CurrentTask.AllHints.Count != 0)
        {
            treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.AllHints[0];
            hintInputField.text = treasureHuntManager.CurrentHint.Text;
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

    #region - LocationManager events -
    
    private void TargetLocationReached()
    {
        // Target location radius reached, task is solved                
        OnTaskSolved(true);
    }

    private void LocationServiceStarted()
    {
        workingSpinnerPanel.SetActive(false);
    }

    private void LocationServiceNotStarted()
    {
        useCurrentLocation.isOn = false; // In case the toggle was used for starting the service
        workingSpinnerPanel.SetActive(false);

        messagePanel.SetActive(true);
        messageText.text = Constants.LocationServiceNotStarted;

        if (GameMode == GameMode.PlayMode)
        {
            ResetPlayModeAdvancedOptions();
        }
    }

    private void LocationDisabledByUser()
    {
        useCurrentLocation.isOn = false; // In case the toggle was used for starting the service
        workingSpinnerPanel.SetActive(false);

        messagePanel.SetActive(true);
        messageText.text = Constants.LocationServiceDisabledByUser;

        if (GameMode == GameMode.PlayMode)
        {
            ResetPlayModeAdvancedOptions();
        }
    }

    private void StartingLocationService()
    {
        workingSpinnerPanel.SetActive(true);
        workingSpinnerText.text = Constants.StartingLocationService;
    }

    #endregion  
}