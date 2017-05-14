using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HintPanel : MonoBehaviour
{
    public Button removeHint;

    private void OnEnable()
    {
        removeHint.interactable = true;
    }

    private void OnDisable()
    {
        removeHint.interactable = false;
    }
}