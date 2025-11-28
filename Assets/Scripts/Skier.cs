using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(SpriteRenderer))]
public class Skier : MonoBehaviour
{
    public float mass = 70.0f;
    public float startAngle = 0.0f;
    Environment myEnv;
    SpriteRenderer myRenderer;
    [SerializeField] Vector2 myVelocity = Vector2.zero;
    readonly List<Surface> surfaces = new();
    [SerializeField] Surface mySurface = null;
    public float Width { get; private set; }
    public float Height { get; private set; }
    public float VelocityCutoff = 0.01f;
    public Surface spawnSurface;

    void Awake()
    {
        myEnv = FindFirstObjectByType<Environment>();
        myRenderer = GetComponent<SpriteRenderer>();
        surfaces.AddRange(FindObjectsByType<Surface>(FindObjectsSortMode.None));
    }

    void Start()
    {
        Width = myRenderer.bounds.extents.x;
        Height = myRenderer.bounds.extents.y;
        transform.rotation = Quaternion.Euler(0, 0, startAngle);
        Spawn();
    }

    void Update()
    {
        CheckCollisions();
        myVelocity = UpdateVelocity(myVelocity);
        transform.position = CalcNewPos(myVelocity);
    }

    void CheckCollisions()
    {
        var velocity = UpdateVelocity(myVelocity);
        var nextPos = CalcNewPos(velocity);
        float traceAngle = (transform.eulerAngles.z - 90.0f) * Mathf.Deg2Rad;
        traceAngle = ClampAngleRad(traceAngle);
        float edgeX = nextPos.x + Height * Mathf.Cos(traceAngle);
        float edgeY = nextPos.y + Height * Mathf.Sin(traceAngle);
        nextPos = new(edgeX, edgeY);
        foreach (var surf in surfaces)
        {
            Vector2 surfPos = surf.transform.position;
            (float xDiff, float yDiff) = (nextPos.x - surfPos.x, nextPos.y - surfPos.y);
            float angle = Mathf.Atan2(yDiff, xDiff);
            Vector2 relPos = new(xDiff, yDiff);
            float surfAngle = ClampAngleDeg(-surf.transform.eulerAngles.z) * Mathf.Deg2Rad;
            relPos = RotatePlane(relPos, angle + surfAngle);
            if (relPos.y <= surf.Height && relPos.x >= -surf.Width && relPos.x <= surf.Width)
            {
                if (mySurface != surf)
                {
                    myVelocity = new(myVelocity.x, 0);
                }
                mySurface = surf;
                transform.rotation = mySurface.transform.rotation;
                break;
            }
            else mySurface = null;
        }
    }

    Vector2 UpdateVelocity(Vector2 velocity)
    {
        var force = CalcForces();
        Vector2 a = CalcAccel(force);
        float t = Time.deltaTime;
        return new(velocity.x + a.x * t, velocity.y + a.y * t);
    }

    Vector2 CalcForces()
    {
        Vector2 mg = new(0, mass * -myEnv.gravity);
        Vector2 normal = Vector2.zero;
        Vector2 fric = Vector2.zero;
        if (mySurface != null)
        {
            float theta = mySurface.Rotation * Mathf.Deg2Rad;

            float normMag = -mg.y * Mathf.Cos(theta);
            float normX = normMag * Mathf.Sin(theta);
            float normY = normMag * Mathf.Cos(theta);
            normal = new(normX, normY);

            float fricMag = mySurface.fricCoef * normMag;

            float fricX = 0;
            if (myVelocity.x > VelocityCutoff) fricX = -fricMag * Mathf.Cos(theta);
            else if (myVelocity.x < -VelocityCutoff) fricX = fricMag * Mathf.Cos(theta);
            else myVelocity = new(0, myVelocity.y);

            float fricY = 0;
            if (myVelocity.y > VelocityCutoff) fricY = -fricMag * Mathf.Sin(theta);
            else if (myVelocity.y < -VelocityCutoff) fricY = fricMag * Mathf.Sin(theta);
            else myVelocity = new(myVelocity.x, 0);

            fric = new(fricX, fricY);
        }
        Vector2 sum = mg + normal + fric;
        return sum;
    }

    Vector2 CalcAccel(Vector2 force)
    {
        Vector2 accel = new(force.x / mass, force.y / mass);
        return accel;
    }

    Vector2 CalcNewPos(Vector2 velocity)
    {
        return new(transform.position.x + velocity.x * Time.deltaTime, transform.position.y + velocity.y * Time.deltaTime);
    }

    float ClampAngleRad(float angle)
    {
        if (angle > 2.0f * Mathf.PI) angle -= 2.0f * Mathf.PI;
        else if (angle < 0.0f) angle += 2.0f * Mathf.PI;
        return angle;
    }

    float ClampAngleDeg(float angle)
    {
        if (angle > 360.0f) angle -= 360.0f;
        else if (angle < 0.0f) angle += 360.0f;
        return angle;
    }

    Vector2 RotatePlane(Vector2 point, float angle)
    {
        float d = Mathf.Sqrt(Mathf.Pow(point.x, 2) + Mathf.Pow(point.y, 2));
        float x = d * Mathf.Cos(angle);
        float y = d * Mathf.Sin(angle);
        return new(x, y);
    }

    public void Spawn()
    {
        myVelocity = Vector2.zero;
        transform.rotation = spawnSurface.transform.rotation;
        float angle = spawnSurface.transform.eulerAngles.z * Mathf.Deg2Rad;
        var spawnPos = spawnSurface.spawnPoint;
        float xDiff = -Height * Mathf.Sin(angle) + Width * Mathf.Cos(angle);
        float yDiff = Height * Mathf.Cos(angle) + Width * Mathf.Sin(angle);
        spawnPos = new(spawnPos.x + xDiff, spawnPos.y + yDiff);
        transform.position = spawnPos;
    }
}
