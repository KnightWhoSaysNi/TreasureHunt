using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class LocationRadiusSlider : MonoBehaviour
{
    public Text radiusText;    

    public void OnValueChanged(float value)
    {
        radiusText.text = value.ToString();
    }
}