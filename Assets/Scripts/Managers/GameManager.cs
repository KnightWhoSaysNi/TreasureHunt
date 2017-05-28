using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TreasureHunt;

[RequireComponent(typeof(TreasureHuntManager), typeof(LocationManager))]
public class GameManager : MonoBehaviour
{
    private TreasureHuntManager treasureHuntManager;
    private LocationManager locationManager;

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

                // In CreationMode: A check to see if Task, Answer or Hint input fields are empty
                if (treasureHuntManager.GameMode == GameMode.CreationMode && !CanTaskBeSaved(false))
                {
                    messagePanel.SetActive(true);
                    messageText.text = "Your task is not yet saved. If you go back it will be deleted. Are you sure you want to go back?";
                    saveReminderButtons.SetActive(true);

                    // Stopping the 'Back' process until the player decides what to do
                    return;                    
                }

                // In case the correct answer panel was still visible
                taskSolvedPanel.SetActive(false);

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
        treasureHuntManager.RemoveTask(treasureHuntManager.CurrentTask);

        gameMenu.gameObject.SetActive(true);
        taskPanel.gameObject.SetActive(false);
        answerPanel.gameObject.SetActive(false);

        GameMenuTuple previousGameMenu = gameMenuStack.Pop();
        UpdateGameMenu(previousGameMenu.Title, previousGameMenu.ListOfItems, previousGameMenu.MenuItemType);
    }

    #endregion

    #region - Other Menus -

    public void CreateTreasureHunt()
    {
        if (treasureHuntManager.AllTreasureHunts == null)
        {
            // Treasure hunts not yet loaded
            StartCoroutine(WaitForPersistenceServiceLoaded());
        }
        else
        {
            // Treasure hunts already loaded (if there were any)
            treasureHuntManager.CreateTreasureHunt();
        }
    }

    public void ShowAllTreasureHunts()
    {
        string header = "All Treasure Hunts";
        
        if (treasureHuntManager.AllTreasureHunts == null)
        {
            SetHeader(header);            
            // Loading of saved treasure hunts not finished 
            workingSpinnerPanel.SetActive(true);
            workingSpinnerText.text = Constants.LoadingMessage;
        }
        else
        {
            UpdateGameMenu(header, treasureHuntManager.AllTreasureHunts);
        }
    }

    private void UpdateGameMenu(string header, IEnumerable listOfItems, MenuItemType menuItemType = MenuItemType.TreasureHunt)
    {
        SetHeader(header, menuItemType);
        MenuItemPool.Instance.ReclaimMenuItems();

        bool isProblemInteractable = true;

        foreach (var item in listOfItems)
        {
            MenuItem menuItem = MenuItemPool.Instance.GetMenuItem(gameMenu);
            menuItem.MenuItemType = menuItemType;

            string title = ((ITitle)item).Title;
            menuItem.text.text = title;

            if (treasureHuntManager.GameMode == GameMode.CreationMode)
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
                        treasureHuntManager.CurrentTreasureHunt = menuItem.TreasureHunt;

                        hintPoints.text = treasureHuntManager.CurrentTreasureHunt.StartingHintPoints.ToString();
                    });

                    if (treasureHuntManager.GameMode == GameMode.PlayMode)
                    {
                        menuItem.checkMark.SetActive(menuItem.TreasureHunt.IsCompleted);
                    }
                    break;
                case MenuItemType.Problem:
                    menuItem.Problem = (Problem)item;

                    if (treasureHuntManager.GameMode == GameMode.PlayMode)
                    {
                        menuItem.checkMark.SetActive(menuItem.Problem.IsSolved);
                        menuItem.GetComponent<Button>().interactable = isProblemInteractable;
                    }

                    isProblemInteractable = menuItem.Problem.IsSolved;

                    if (menuItem.Problem.Tasks.Count == 1)
                    {
                        // TODO skip showing Tasks if the game is in Play Mode and just open the one Task
                        // If the game is in Creation Mode show Task and the Add Task button
                    }
                    button.onClick.AddListener(() =>
                    {
                        gameMenuStack.Peek().Title = treasureHuntManager.CurrentTreasureHunt.Title;

                        UpdateGameMenu(title, menuItem.Problem.Tasks, MenuItemType.Task);
                        treasureHuntManager.CurrentProblem = menuItem.Problem;

                        hintPoints.text = treasureHuntManager.CurrentProblem.HintPoints.ToString();
                    });
                    break;
                case MenuItemType.Task:
                    menuItem.Task = (Task)item;
                    // TODO Add button listener which shows the Task panel with the appropriate Task
                    button.onClick.AddListener(() =>
                    {
                        gameMenuStack.Peek().Title = treasureHuntManager.CurrentProblem.Title;

                        treasureHuntManager.CurrentTask = menuItem.Task;                  
                        ActivateTask();
                    });

                    if (treasureHuntManager.GameMode == GameMode.PlayMode && menuItem.Task.IsSolved)
                    {
                        menuItem.checkMark.SetActive(true);
                    }
                    break;
                default:
                    break;
            }                     
        }

        if (treasureHuntManager.GameMode == GameMode.CreationMode)
        {
            // Adding an Add button for TreasureHunt/Problem/Task
            switch (menuItemType)
            {
                case MenuItemType.TreasureHunt:
                    hintPointsPanel.SetActive(false); // In creation mode switching from problem this might still be visible

                    MenuItemPool.Instance.GetAddTreasureHunt(gameMenu);
                    break;
                case MenuItemType.Problem:
                    hintPointsPanel.SetActive(true);
                    hintPointsText.text = Constants.TreasureHuntStartingHintPoints;

                    if (treasureHuntManager.CurrentTreasureHunt != null)
                    {
                        // Going back from a problem
                        hintPoints.text = treasureHuntManager.CurrentTreasureHunt.StartingHintPoints.ToString();
                    }

                    MenuItemPool.Instance.GetAddProblem(gameMenu);
                    break;
                case MenuItemType.Task:
                    hintPointsPanel.SetActive(true);
                    hintPointsText.text = Constants.ProblemRewardHintPoints;

                    if (treasureHuntManager.CurrentProblem != null)
                    {
                        // Going back from a task
                        hintPoints.text = treasureHuntManager.CurrentProblem.HintPoints.ToString();
                    }

                    MenuItemPool.Instance.GetAddTask(gameMenu);
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator WaitForPersistenceServiceLoaded()
    {
        SetHeader("Unnamed Treasure Hunt", MenuItemType.Problem);
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
    // TODO answerInputField Placeholder needs to be different depending on the GameMode, or changed to something more appropriate if it's the same for both

    public void CheckAnswer()
    {
        string possibleTextSolution = answerInputField.text;

        bool isAnswerCorrect = treasureHuntManager.IsSolutionCorrect(possibleTextSolution);

        if (isAnswerCorrect)
        {
            OnSolvedTask();   
        }
        else
        {
            // Wrong answer            
            StartCoroutine(ChangeInputFieldColor(answerInputField,Color.red));            
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

        // Task can be saved

        if (treasureHuntManager.CurrentHint != null)
        {    
            SaveHint();
        }
        
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

        addHint.interactable = true;

        treasureHuntManager.SaveTreasureHunt();
    }

    public void CancelCreationModeTask()
    {
        taskInputField.text = treasureHuntManager.CurrentTask.TextClue;
        if (treasureHuntManager.CurrentHint != null)
        {
            hintInputField.text = treasureHuntManager.CurrentHint.Text;
        }
        answerInputField.text = treasureHuntManager.CurrentTask.Solution.TextSolution; // TODO incorporate location into solution
    }

    private IEnumerator DisplayCorrectAnswerPanel(float seconds, bool isLocationReached = false)
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

        while (seconds > 0)
        {
            seconds -= Time.deltaTime;
            yield return null;
        }

        taskSolvedPanel.SetActive(false);
    }

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

    private IEnumerator ShowFireworks(float time)
    {
        // TODO particle effect set active to true
        while (time > 0)
        {


            time -= Time.deltaTime;
            yield return null;
        }
        // TODO particle effect set active to false
    }

    private void ActivateTask()
    {
        // In creation mode this was active so it needs to be set to false
        hintPointsPanel.SetActive(false); 

        SetHeader(treasureHuntManager.CurrentTask.Title, MenuItemType.Task);

        // Show Task and Answer panels and hide Game Menu
        gameMenu.gameObject.SetActive(false);
        taskPanel.gameObject.SetActive(true);
        answerPanel.gameObject.SetActive(true);

        if (treasureHuntManager.GameMode == GameMode.PlayMode)
        {
            taskText.text = treasureHuntManager.CurrentTask.TextClue;

            if (treasureHuntManager.CurrentTask.RevealedHints.Count == 0)
            {
                // There are no revealed hints for this taks 
                hintPanel.gameObject.SetActive(false);
            }
            else
            {
                // There are revealed hints for this Task
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
        else // GameMode is Creation Mode
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
    }

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

    private void OnSolvedTask(bool isLocationReached = false)
    {
        treasureHuntManager.CurrentTask.IsSolved = true;
        StartCoroutine(ChangeInputFieldColor(answerInputField, Color.green));
        StartCoroutine(DisplayCorrectAnswerPanel(5, isLocationReached));
        DisplaySolvedTask();

        if (treasureHuntManager.CurrentTreasureHunt.IsCompleted)
        {
            // TODO Signal to the player that the treasure hunt is completed
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
            messagePanel.SetActive(true);
            saveMessage = "You cannot save a task without an answer.";
            canTaskBeSaved = false;
        }
        else if (treasureHuntManager.CurrentHint != null && !CanHintBeSaved(false))
        {
            // Hint InputField is empty
            saveMessage = "You cannot save an empty hint. Please either add some text or remove the hint.";
            canTaskBeSaved = false;
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

    public void RevealHint()
    {
        // This can only be called if there are some Unrevealed hints and there are Hint points available
        Hint hint = treasureHuntManager.CurrentTask.UnrevealedHints[0];
        treasureHuntManager.CurrentTask.UnrevealedHints.RemoveAt(0);
        treasureHuntManager.CurrentTask.RevealedHints.Add(hint);
        treasureHuntManager.CurrentHint = hint;
        treasureHuntManager.CurrentTreasureHunt.HintPointsAvailable--;

        hintPanel.gameObject.SetActive(true);
        hintText.text = hint.Text;

        RefreshHintNavigation();
    }

    public void ShowPreviousHint()
    {
        if (treasureHuntManager.GameMode == GameMode.PlayMode)
        {
            ShowHint(-1);
        }
        else if (CanHintBeSaved()) // Creation Mode
        {
            if (treasureHuntManager.CurrentHint.Text == string.Empty)
            {
                // Current Hint is empty, but hintInputField.text is not
                SaveHint();
            }
            ShowHint(-1);
        }
    }

    public void ShowNextHint()
    {
        if (treasureHuntManager.GameMode == GameMode.PlayMode)
        {
            ShowHint(1);
        }
        else if (CanHintBeSaved()) // Creation Mode 
        {
            if (treasureHuntManager.CurrentHint.Text == string.Empty)
            {
                // Current Hint is empty, but hintInputField.text is not
                SaveHint();
            }
            ShowHint(1);
        }
    }

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

    private void SaveHint()
    {
        treasureHuntManager.CurrentHint.Text = hintInputField.text;
        treasureHuntManager.SaveTreasureHunt();
    }

    private bool CanHintBeSaved(bool isReminderNeeded = true)
    {
        bool canHintBeSaved = true;

        if (hintInputField.text == string.Empty)
        {
            canHintBeSaved = false;

            if (isReminderNeeded)
            {
                messagePanel.SetActive(true);
                messageText.text = "Current hint is empty. Please either add some text or remove the hint.";
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
        if (treasureHuntManager.GameMode == GameMode.PlayMode)
        {
            // RevealedHints are used in PlayMode
            currentHintIndex = treasureHuntManager.CurrentTask.RevealedHints.IndexOf(treasureHuntManager.CurrentHint);
            currentHintIndex = currentHintIndex + direction;
            if (currentHintIndex < 0)
            {
                currentHintIndex = treasureHuntManager.CurrentTask.RevealedHints.Count - 1;
            }
            currentHintIndex %= treasureHuntManager.CurrentTask.RevealedHints.Count;
            treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.RevealedHints[currentHintIndex];

            hintText.text = treasureHuntManager.CurrentHint.Text;
        }
        else // Creation Mode
        {
            // AllHints are used in CreationMode
            currentHintIndex = treasureHuntManager.CurrentTask.AllHints.IndexOf(treasureHuntManager.CurrentHint);
            currentHintIndex = currentHintIndex + direction;
            if (currentHintIndex < 0)
            {
                currentHintIndex = treasureHuntManager.CurrentTask.AllHints.Count - 1;
            }
            currentHintIndex %= treasureHuntManager.CurrentTask.AllHints.Count;
            treasureHuntManager.CurrentHint = treasureHuntManager.CurrentTask.AllHints[currentHintIndex];

            hintInputField.text = treasureHuntManager.CurrentHint.Text;
        }
    }

    private void RefreshHintNavigation()
    {
        if (treasureHuntManager.GameMode == GameMode.PlayMode)
        {
            // RevealedHints are used in PlayMode
            if (treasureHuntManager.CurrentTask.RevealedHints.Count > 1)
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
            if (treasureHuntManager.CurrentTask.AllHints.Count > 1)
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

    #region - Hints points related methods -

    public void ChangeHintPoints(int changeBy)
    {
        int points;

        if (int.TryParse(hintPoints.text, out points))
        {
            points += changeBy;
            points = Mathf.Clamp(points, Constants.MinHintPoints, Constants.MaxHintPoints);
            hintPoints.text = points.ToString();

            MenuItemType menuItemType = gameMenuStack.Peek().MenuItemType;
            if (menuItemType == MenuItemType.TreasureHunt)
            {
                treasureHuntManager.ChangeTreasureHuntHintPoints(points);
            }
            else if(menuItemType == MenuItemType.Problem)
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

        MenuItemType currentMenuItemType = gameMenuStack.Peek().MenuItemType;
        string oldTitle = treasureHuntManager.CurrentTreasureHunt.Title;

        switch (currentMenuItemType)
        {
            case MenuItemType.TreasureHunt:
                if (treasureHuntManager.ContainsTreasureHuntTitle(newTitle))
                {
                    messagePanel.SetActive(true);
                    messageText.text = "A treasure hunt with the same name already exists.";
                    SetHeader(oldTitle, MenuItemType.Problem);
                    return;
                }
                treasureHuntManager.CurrentTreasureHunt.ChangeTitle(newTitle);
                break;
            case MenuItemType.Problem:
                treasureHuntManager.CurrentProblem.ChangeTitle(newTitle);
                break;
            case MenuItemType.Task:
                treasureHuntManager.CurrentTask.ChangeTitle(newTitle);
                break;            
            default:                
                break;
        }

        if (currentMenuItemType == MenuItemType.TreasureHunt && (newTitle != oldTitle))
        {
            // Treasure Hunt's title is changed and it's different than the old one
            treasureHuntManager.SaveTreasureHunt(oldTitle);
        }
        else
        {
            treasureHuntManager.SaveTreasureHunt();
        }        
    }

    private void SetHeader(string header, MenuItemType menuItemType = MenuItemType.TreasureHunt)
    {
        if (treasureHuntManager.GameMode == GameMode.PlayMode)
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

    #region - Advanced options related methods - 

    public void ShowAdvancedOptions()
    {
        if (treasureHuntManager.GameMode == GameMode.CreationMode && treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            latitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Latitude.ToString();
            longitude.text = treasureHuntManager.CurrentTask.Solution.LocationSolution.Longitude.ToString();
            radiusSlider.value = treasureHuntManager.CurrentTask.Solution.LocationSolution.Radius;
        }
        else // Play mode
        {
            if (!treasureHuntManager.CurrentTask.IsSolved)
            {
                IgnoreLocation();
                                
                checkCoordinates.interactable = true;
                checkPosition.interactable = true;
            }
        }
    }

    public void CloseAdvancedOptions()
    {
        // In case it was used on the first openning of the advanced options
        useCurrentLocation.isOn = false;

        locationManager.StopLocationService();
    }

    #endregion

    #region - Location related methods - 

    public void UseCurrentLocation(bool useCurrentLocation)
    {
        locationManager.UseCurrentLocation(useCurrentLocation);
    }

    public void IgnoreLocation()
    {
        latitude.text = string.Empty;
        longitude.text = string.Empty;
        radiusSlider.value = Constants.DefaultLocationRadius;
    }

    public void CheckCoordinates()
    {
        if (!treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            // The current task has no location solution
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

    public void CheckPosition()
    {
        if (!treasureHuntManager.CurrentTask.Solution.HasLocationSolution)
        {
            // The current task has no location solution
            return;
        }

        locationManager.CheckPosition(treasureHuntManager.CurrentTask.Solution.LocationSolution);
    }

    #endregion

    public void RemoveMenuItem(MenuItem menuItem)
    {
        MenuItemPool.Instance.ReclaimMenuItem(menuItem);

        switch (menuItem.MenuItemType)
        {
            case MenuItemType.TreasureHunt:
                treasureHuntManager.RemoveTreasureHunt(menuItem.TreasureHunt);
                break;
            case MenuItemType.Problem:
                treasureHuntManager.RemoveProblem(menuItem.Problem);
                break;
            case MenuItemType.Task:
                treasureHuntManager.RemoveTask(menuItem.Task);
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

        menuStack = new Stack<Transform>();
        gameMenuStack = new Stack<GameMenuTuple>();

        treasureHuntManager.TreasureHuntsLoaded += TreasureHuntsLoaded;
        treasureHuntManager.TreasureHuntSaveStarted += TreasureHuntSaveStarted;
        treasureHuntManager.TreasureHuntSaved += TreasureHuntSaved;
        
        treasureHuntManager.GameModeChanged += OnGameModeChanged;

        treasureHuntManager.TreasureHuntCreated += OnTreasureHuntCreated;
        treasureHuntManager.ProblemCreated += OnProblemCreated;
        treasureHuntManager.TaskCreated += OnTaskCreated;
        treasureHuntManager.HintCreated += OnHintCreated;

        treasureHuntManager.HintRemoved += OnHintRemoved;

        // Location Manager event subscription
        locationManager.StartingLocationService += LocationManager_StartingLocationService;
        locationManager.LocationDisabledByUser += LocationManager_LocationDisabledByUser;
        locationManager.LocationServiceNotStarted += LocationManager_LocationServiceNotStarted;
        locationManager.LocationServiceStarted += LocationManager_LocationServiceStarted;

        locationManager.TargetLocationReached += LocationManager_TargetLocationReached;
    }

    

    #region - TreasureHuntManager events -

    private void TreasureHuntSaveStarted()
    {
        workingSpinnerPanel.SetActive(true);
        workingSpinnerText.text = Constants.SavingMessage;
    }

    private void TreasureHuntsLoaded()
    {
        treasureHuntManager.TreasureHuntsLoaded -= TreasureHuntsLoaded;

        if (workingSpinnerPanel.activeSelf)
        {
            // Player already waiting for all treasure hunts
            workingSpinnerPanel.SetActive(false);
            UpdateGameMenu("All Treasure Hunts", treasureHuntManager.AllTreasureHunts);
        }
    }

    private void TreasureHuntSaved()
    {
        workingSpinnerPanel.SetActive(false);
    }

    private void OnGameModeChanged()
    {
        bool isInPlayMode = treasureHuntManager.GameMode == GameMode.PlayMode;

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

    #region - OnCreated Handlers -

    private void OnTreasureHuntCreated()
    {
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = "All Treasure Hunts";
        stackItem.ListOfItems = treasureHuntManager.AllTreasureHunts;
        stackItem.MenuItemType = MenuItemType.TreasureHunt;
        gameMenuStack.Push(stackItem);

        UpdateGameMenu(treasureHuntManager.CurrentTreasureHunt.Title, treasureHuntManager.CurrentTreasureHunt.Problems, MenuItemType.Problem);
    }

    private void OnProblemCreated()
    {
        // Game is already in Creation Mode
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = treasureHuntManager.CurrentTreasureHunt.Title;
        stackItem.ListOfItems = treasureHuntManager.CurrentTreasureHunt.Problems;
        stackItem.MenuItemType = MenuItemType.Problem;
        gameMenuStack.Push(stackItem);

        UpdateGameMenu(treasureHuntManager.CurrentProblem.Title, treasureHuntManager.CurrentProblem.Tasks, MenuItemType.Task);        
    }

    private void OnTaskCreated()
    {
        // Game is in Creation Mode
        GameMenuTuple stackItem = new GameMenuTuple();
        stackItem.Title = treasureHuntManager.CurrentProblem.Title;
        stackItem.ListOfItems = treasureHuntManager.CurrentProblem.Tasks;
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

    private void LocationManager_TargetLocationReached()
    {
        // Target location radius reached, Task is solved        
        //advancedOptionsPanel.SetActive(false);
        OnSolvedTask();
    }

    private void LocationManager_LocationServiceStarted()
    {
        workingSpinnerPanel.SetActive(false);
    }

    private void LocationManager_LocationServiceNotStarted()
    {
        useCurrentLocation.isOn = false; // In case the toggle was used for starting the service
        workingSpinnerPanel.SetActive(false);

        messagePanel.SetActive(true);
        messageText.text = Constants.LocationServiceNotStarted;
    }

    private void LocationManager_LocationDisabledByUser()
    {
        useCurrentLocation.isOn = false; // In case the toggle was used for starting the service
        workingSpinnerPanel.SetActive(false);

        messagePanel.SetActive(true);
        messageText.text = Constants.LocationServiceDisabledByUser;
    }

    private void LocationManager_StartingLocationService()
    {
        workingSpinnerPanel.SetActive(true);
        workingSpinnerText.text = Constants.StartingLocationServiceMessage;
    }

    #endregion  
}

public class GameMenuTuple
{
    public string Title { get; set; }
    public IEnumerable ListOfItems { get; set; }
    public MenuItemType MenuItemType { get; set; }
}

