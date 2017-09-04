using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HintOptions : MonoBehaviour
{
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

    /// <summary>
    /// Displays remaining hint points in the appropriate Text game object and sets interactable property
    /// of the reveal hint button.
    /// </summary>
    private void CheckHintOptions()
    {
        string remainingHintPointsMessage = (treasureHuntManager.CurrentTreasureHunt.HintPointsAvailable == 1) ? 
            Constants.RemainingHintPointsMessageSingular : Constants.RemainingHintPointsMessagePlural;

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