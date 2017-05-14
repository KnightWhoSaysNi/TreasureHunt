using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AnswerPanel : MonoBehaviour
{
    public GameObject hintOptions;
    public GameObject hideHintOptions;
    public GameObject showHintOptions;

    [Space(10)]
    public GameObject locationOptions;
    public GameObject hideLocationOptions;
    public GameObject showLocationOptions;

    [Space(5)]
    public InputField latitude;
    public InputField longitude;

    private void OnDisable()
    {
        hintOptions.SetActive(false);
        hideHintOptions.SetActive(false);
        showHintOptions.SetActive(true);

        locationOptions.SetActive(false);
        hideLocationOptions.SetActive(false);
        showLocationOptions.SetActive(true);

        latitude.text = string.Empty;
        longitude.text = string.Empty;
    }
}