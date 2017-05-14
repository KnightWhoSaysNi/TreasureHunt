using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HintOptions : MonoBehaviour
{
    private string remainingHintPointsMessage = " hint points remaining";

    public GameManager gameManager;

    [Space(10)]
    public Button revealHint;
    public Text remainingHintPointsText;

    // This MUST be called after UIManager.RevealHint in order to work correctly
    public void OnRevealHint()
    {
        CheckHintOptions();
    }
    
    private void OnEnable()
    {
        CheckHintOptions();
    }

    private void CheckHintOptions()
    {
        remainingHintPointsText.text = gameManager.CurrentTreasureHunt.HintPointsAvailable + remainingHintPointsMessage;

        if (gameManager.CurrentTask.UnrevealedHints.Count != 0 && gameManager.CurrentTreasureHunt.HintPointsAvailable != 0)
        {
            revealHint.interactable = true;
        }
        else
        {
            revealHint.interactable = false;
        }
    }
}