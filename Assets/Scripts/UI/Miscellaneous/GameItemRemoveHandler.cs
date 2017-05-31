using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameItemRemoveHandler : MonoBehaviour
{
    public GameItem gameItem;

    private void Start()
    {
        GameManager gameManager = GameObject.FindGameObjectWithTag("Game Manager").GetComponent<GameManager>();
        Button button = gameItem.removeButton.GetComponent<Button>();
        button.onClick.AddListener(() => gameManager.RemoveGameItem(gameItem));
    }
}