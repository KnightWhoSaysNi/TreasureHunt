using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuItemRemoveHandler : MonoBehaviour
{
    public MenuItem menuItem;

    private void Start()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        Button button = menuItem.removeButton.GetComponent<Button>();
        button.onClick.AddListener(() => gameManager.RemoveMenuItem(menuItem));
    }
}