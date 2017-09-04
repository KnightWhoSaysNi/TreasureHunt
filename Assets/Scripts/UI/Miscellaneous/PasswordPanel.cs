using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PasswordPanel : MonoBehaviour
{
    public InputField[] passwordInputFields;    

    /// <summary>
    /// Clears all input fields in the password panel.
    /// </summary>
    private void OnDisable()
    {
        foreach (InputField passwordInputField in passwordInputFields)
        {
            passwordInputField.text = string.Empty;
        }
    }
}