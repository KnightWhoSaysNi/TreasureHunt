using UnityEngine;
using System.Collections;

public class CreateATreasureHunt : MonoBehaviour
{
    public TreasureHuntManager gameManager;

    private void OnEnable()
    {
        gameManager.GoToPlayMode();
    }
}