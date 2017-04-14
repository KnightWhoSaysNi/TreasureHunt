using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Menu currentMenu;
    private Transform content;
    private Text header;
    private Dictionary<Button, Menu> buttonMenus;
    private Stack<Menu> menuStack;
    
    private Button backButton;

    public Menu mainMenu;
    public RectTransform menuPanel;
    public Transform buttonGroup;
    public Transform button;

    private void Start()
    {
        //menuPanel = Instantiate(menuPanel);        
        //menuPanel.SetParent(GameObject.FindObjectOfType<Canvas>().transform, false); // TODO panel should always be there without setting the world position to false top, right and scale is completely different than the prefab
        currentMenu = mainMenu;

        content = menuPanel.FindChild("Scroll View").FindChild("Viewport").FindChild("Content"); // TODO very ugly way - change it
        header = menuPanel.FindChild("InputField").FindChild("Placeholder").GetComponent<Text>();
        buttonMenus = new Dictionary<Button, Menu>();
        menuStack = new Stack<Menu>();

        GetBackButton();

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
        header.text = newMenu.name;
        UpdateMenu();
    }

    private void UpdateMenu()
    {
        
        buttonMenus.Clear();
        ButtonPool.Instance.HideBackButton();

        UpdateButtons();
    }

    private void UpdateButtons()
    {
        ButtonPool.Instance.ReclaimButtons();

        if (content == null)
            print("No content found"); // TODO this should really throw exception

        foreach (TreasureHunt.UI.ButtonInfo buttonInfo in currentMenu.buttons)
        {
            Transform newButtonGroup = ButtonPool.Instance.GetButtonGroup(content);

            Button button = newButtonGroup.GetComponentInChildren<Button>(); // TODO check if null
            button.GetComponentInChildren<Text>().text = buttonInfo.header; // TODO this could throw exception if there is no Text component in the button
            buttonMenus.Add(button, buttonInfo.menu);
            // If buttonInfo.menu is used directly in AddListener by the time it is called it will not be available as it's apparently not being added to the button in the same way as in edior
            button.onClick.AddListener(() => OpenMenu(buttonMenus[button]));                        

            Image image = newButtonGroup.FindChild("Check mark").GetComponentInChildren<Image>(); // TODO check if null | constant 
            if (buttonInfo.isCheckmarkRequired)
            {
                image.gameObject.SetActive(true);
            }
            else
            {
                image.gameObject.SetActive(false);
            }
        }

        // Setting up back button
        if (currentMenu.name == "Main Menu")
        {
            // Back button is not needed
            ButtonPool.Instance.HideBackButton();            
        }
        else
        {
            // Back button is needed            
            ButtonPool.Instance.ShowBackButton(content);
            backButton.onClick.RemoveAllListeners();
            Menu previousMenu = menuStack.Peek();           
            backButton.onClick.AddListener(() => OpenMenu(previousMenu, true));
        }
    }

    private void GetBackButton()
    {
        backButton = ButtonPool.Instance.BackButton.GetComponentInChildren<Button>();
        backButton.GetComponentInChildren<Text>().text = "Back";
    }
}