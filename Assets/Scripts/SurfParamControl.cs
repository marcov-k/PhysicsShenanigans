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
    public GameObject surface;
    SurfaceManager manager;

    void Awake()
    {
        manager = FindFirstObjectByType<SurfaceManager>();
    }

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
        float fric = 0.0f;
        if (fricInput.text != string.Empty)
        {
            fric = (float)Convert.ToDouble(fricInput.text);
        }
        return fric;
    }

    public void AlignCamera()
    {
        manager.TrackSurface(surface);
    }
}
