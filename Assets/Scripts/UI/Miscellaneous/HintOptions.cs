using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HintOptions : MonoBehaviour
{
    private string remainingHintPointsMessage = " hint points remaining";

    public TreasureHuntManager treasureHuntManager;

    [Space(10)]
    public Button revealHint;
    public Text remainingHintPointsText;

    // This MUST be called after GameManager.RevealHint in order to work correctly
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
        remainingHintPointsText.text = treasureHuntManager.CurrentTreasureHunt.HintPointsAvailable + remainingHintPointsMessage;

        if (treasureHuntManager.CurrentTask.UnrevealedHints.Count != 0 && treasureHuntManager.CurrentTreasureHunt.HintPointsAvailable != 0)
        {
            revealHint.interactable = true;
        }
        else
        {
            revealHint.interactable = false;
        }
    }
}