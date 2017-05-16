using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SaveReminderButtons : MonoBehaviour
{
    public Button closePanel;

    private void OnEnable()
    {
        closePanel.interactable = false;
    }

    private void OnDisable()
    {
        closePanel.interactable = true;
    }
}