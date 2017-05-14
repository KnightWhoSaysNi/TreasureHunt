using UnityEngine;
using System.Collections;

public class CreateATreasureHunt : MonoBehaviour
{
    public GameManager gameManager;

    private void OnEnable()
    {
        gameManager.GoToPlayMode();
    }
}