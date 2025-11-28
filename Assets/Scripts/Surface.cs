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
    public Vector2 Origin
    {
        get { return _origin; }
        set
        {
            _origin = value;
            UpdatePose();
        }
    }
    private Vector2 _origin;
    public Vector2 NextOrigin
    {
        get { return _nextOrigin; }
        set
        {
            _nextOrigin = value;
            if (nextSurface != null)
            {
                nextSurface.Origin = _nextOrigin;
            }
        }
    }
    private Vector2 _nextOrigin;
    public float fricCoef;
    public Vector2 SpawnPoint { get; private set; }
    public Surface prevSurface;
    public Surface nextSurface;

    void Awake()
    {
        MyRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        Width = MyRenderer.bounds.extents.x;
        Height = MyRenderer.bounds.extents.y;
        NextOrigin = new(transform.position.x - Width, transform.position.y);
        if (prevSurface == null)
        {
            Origin = new Vector2(transform.position.x + Width, transform.position.y);
        }
    }

    void Update()
    {
        UpdatePose();
    }

    void UpdatePose()
    {
        float xPos = Origin.x - Width * Mathf.Cos(_rotation * Mathf.Deg2Rad);
        float yPos = Origin.y + Width * Mathf.Sin(-_rotation * Mathf.Deg2Rad);
        float nextX = Origin.x - 2.0f * Width * Mathf.Cos(_rotation * Mathf.Deg2Rad);
        float nextY = Origin.y + 2.0f * Width * Mathf.Sin(-_rotation * Mathf.Deg2Rad);
        transform.SetPositionAndRotation(new Vector2(xPos, yPos), Quaternion.Euler(0, 0, _rotation));
        NextOrigin = new(nextX, nextY);
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
        SpawnPoint = new(transform.position.x + xDiff, transform.position.y + yDiff);
    }
}
