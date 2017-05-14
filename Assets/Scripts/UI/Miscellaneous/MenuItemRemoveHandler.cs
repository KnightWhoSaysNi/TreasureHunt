using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuItemRemoveHandler : MonoBehaviour
{
    public MenuItem menuItem;

    private void Start()
    {
        UIManager uiManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<UIManager>();
        Button button = menuItem.removeButton.GetComponent<Button>();
        button.onClick.AddListener(() => uiManager.RemoveMenuItem(menuItem));
    }
}