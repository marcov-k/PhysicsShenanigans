using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SurfParamControl : MonoBehaviour
{
    public Slider rotSlider;
    public TMP_InputField fricInput;
    public int index;
    public TextMeshProUGUI label;

    void Start()
    {
        string text = "Surface Section ";
        if (index == 0) text += "End";
        else text += index;
        label.text = text;
    }

    public float GetRot()
    {
        return rotSlider.value;
    }

    public float GetFric()
    {
        return (float)Convert.ChangeType(fricInput.text, typeof(float));
    }
}
