using UnityEngine;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(SpriteRenderer))]
public class Surface : MonoBehaviour
{
    public float Rotation {
        get { return -_rotation; }
        set { _rotation = -Mathf.Clamp(value, 0.0f, 90.0f); }
    }
    private float _rotation = 0.0f;
    public SpriteRenderer MyRenderer { get; private set; }
    public float Width { get; private set; }
    public float Height { get; private set; }
    Vector2 origin;
    public float fricCoef;
    public Vector2 spawnPoint;

    void Awake()
    {
        MyRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Width = MyRenderer.bounds.extents.x;
        Height = MyRenderer.bounds.extents.y;
        origin = new Vector2(transform.position.x + Width, transform.position.y);
    }

    void Update()
    {
        UpdatePose();
    }

    void UpdatePose()
    {
        float xPos = origin.x - Width * Mathf.Cos(_rotation * Mathf.Deg2Rad);
        float yPos = origin.y + Width * Mathf.Sin(-_rotation * Mathf.Deg2Rad);
        transform.SetPositionAndRotation(new Vector2(xPos, yPos), Quaternion.Euler(0, 0, _rotation));
        CalcSpawnPoint();
    }

    void CalcSpawnPoint()
    {
        float rot = _rotation * Mathf.Deg2Rad;
        float dist = Mathf.Sqrt(Mathf.Pow(Width, 2) + Mathf.Pow(Height, 2));
        float addAngle = Mathf.Acos(Width / dist);
        float totAngle = rot - addAngle;
        float xDiff = -dist * Mathf.Cos(totAngle);
        float yDiff = -dist * Mathf.Sin(totAngle);
        spawnPoint = new(transform.position.x + xDiff, transform.position.y + yDiff);
    }
}
