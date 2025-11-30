using System;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public float gravity = 9.81f;

    public void GravChange(string newGrav)
    {
        if (newGrav != string.Empty)
        {
            gravity = (float)Convert.ToDouble(newGrav);
        }
        else gravity = 0.0f;
    }
}
