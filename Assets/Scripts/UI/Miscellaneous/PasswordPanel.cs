using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PasswordPanel : MonoBehaviour
{
    public InputField[] passwordInputFields;    

    private void OnDisable()
    {
        foreach (InputField passwordInputField in passwordInputFields)
        {
            passwordInputField.text = string.Empty;
        }
    }
}