using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UseCurrentLocation : MonoBehaviour
{
    private Toggle toggle;
    private ColorBlock oldColorBlock;

    public ColorBlock newColorBlock;

    public void OnValueChanged(bool isOn)
    {
        if (isOn)
        {
            toggle.colors = newColorBlock;
        }
        else
        {
            toggle.colors = oldColorBlock;
        }
    }

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        oldColorBlock = toggle.colors;
    }
}