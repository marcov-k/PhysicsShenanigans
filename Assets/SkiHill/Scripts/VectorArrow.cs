using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VectorArrow : MonoBehaviour
{
    public Color myColor;
    Color hiddenColor;
    public float scaleFactor = 10.0f;
    bool shown = false;
    public Vector2 vector;
    public SpriteRenderer myRenderer;
    public Transform bodyTransform;
    public SpriteRenderer head;
    public Vector2 origin;
    public GameObject skier;

    void Start()
    {
        myRenderer.color = new(0, 0, 0, 0);
        head.color = myRenderer.color;
    }

    void Update()
    {
        UpdateOrientation();
    }

    void UpdateOrientation()
    {
        if (shown && vector != null)
        {
            myRenderer.color = myColor;
            head.color = myColor;
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg - 90.0f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            float length = Mathf.Sqrt(Mathf.Pow(vector.x, 2) + Mathf.Pow(vector.y, 2)) / scaleFactor;
            bodyTransform.localScale = new(bodyTransform.localScale.x, length, bodyTransform.localScale.z);
            float offsetMag = length / 2;
            angle = (transform.localEulerAngles.z - 90.0f) * Mathf.Deg2Rad;
            float xOff = -offsetMag * Mathf.Cos(angle);
            float yOff = -offsetMag * Mathf.Sin(angle);
            transform.localPosition = new(origin.x + xOff, origin.y + yOff);
        }
        else
        {
            myRenderer.color = hiddenColor;
            head.color = hiddenColor;
        }
    }

    public void Show()
    {
        shown = true;
    }

    public void Hide()
    {
        shown = false;
    }
    
    public void SetColor(Color color)
    {
        myColor = color;
        hiddenColor = new(color.r, color.g, color.b, 0.0f);
    }
}
