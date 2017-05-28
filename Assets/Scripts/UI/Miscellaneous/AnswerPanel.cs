using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnswerPanel : MonoBehaviour
{
    public GameObject hintOptions;
    public GameObject hideHintOptions;
    public GameObject showHintOptions;

    private void OnDisable()
    {
        hintOptions.SetActive(false);
        hideHintOptions.SetActive(false);
        showHintOptions.SetActive(true);
    }
}