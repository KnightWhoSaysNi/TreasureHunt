using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// A script that adds a remove handler to the remove button of a game item.
/// </summary>
public class GameItemRemoveHandler : MonoBehaviour
{
    public GameItem gameItem;
        
    private void Start()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag(Constants.GameManagerTag).GetComponent<GameManager>();
        Button button = gameItem.removeButton.GetComponent<Button>();
        button.onClick.AddListener(() => gameManager.RemoveGameItem(gameItem));
    }
}