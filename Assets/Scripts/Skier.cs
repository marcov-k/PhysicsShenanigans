using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

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
    public Surface spawnSurface;
    Vector2? collisionNormal;
    public float endPos;
    bool ended = false;
    SurfaceManager manager;
    public GameObject arrowPrefab;
    Vector2 friction = Vector2.zero;
    VectorArrow fricArrow;
    [SerializeField] Color fricColor;
    Vector2 mg = Vector2.zero;
    VectorArrow mgArrow;
    [SerializeField] Color mgColor;
    Vector2 normal = Vector2.zero;
    VectorArrow normalArrow;
    [SerializeField] Color normalColor;
    Vector2 accel = Vector2.zero;
    VectorArrow accelArrow;
    [SerializeField] Color accelColor;
    VectorArrow velocityArrow;
    [SerializeField] Color velocityColor;
    [SerializeField] float forceScale = 20.0f;
    [SerializeField] float velScale = 5.0f;
    public List<TextMeshProUGUI> toggleLabels = new(); // in the order: fric, mg, norm, accel, vel
    public TMP_InputField weightInput;

    void Awake()
    {
        myEnv = FindFirstObjectByType<Environment>();
        myRenderer = GetComponent<SpriteRenderer>();
        manager = FindFirstObjectByType<SurfaceManager>();
    }

    void Start()
    {
        Width = myRenderer.bounds.extents.x;
        Height = myRenderer.bounds.extents.y;
        InitVectorArrows();
        mass = (float)Convert.ToDouble(weightInput.text);
        transform.rotation = Quaternion.Euler(0, 0, startAngle);
        UpdateSurfaces();
        Spawn();
    }

    void Update()
    {
        CheckEnd();
        if (!ended)
        {
            CheckCollisions();
            if (collisionNormal != null)
            {
                myVelocity = UpdateVelocity(myVelocity, collisionNormal);
                collisionNormal = null;
            }
            else myVelocity = UpdateVelocity(myVelocity);
            transform.position = CalcNewPos(myVelocity);
        }
        else myVelocity = Vector2.zero;
        UpdateVectorArrows();
    }

    void CheckEnd()
    {
        if (transform.position.x - myRenderer.bounds.extents.x > endPos) ended = true;
        else ended = false;
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
        Surface newSurf = null;
        foreach (var surf in surfaces)
        {
            Vector2 surfPos = surf.transform.position;
            (float xDiff, float yDiff) = (nextPos.x - surfPos.x, nextPos.y - surfPos.y);
            float angle = Mathf.Atan2(yDiff, xDiff);
            Vector2 relPos = new(xDiff, yDiff);
            float surfAngle = ClampAngleDeg(-surf.transform.eulerAngles.z) * Mathf.Deg2Rad;
            relPos = RotatePlane(relPos, angle + surfAngle);
            if (relPos.y <= surf.Height + 0.001f && relPos.x >= -surf.Width && relPos.x <= surf.Width)
            {
                transform.rotation = surf.transform.rotation;
                if (mySurface == null)
                {
                    float velAngle = ClampAngleRad(-Mathf.Atan2(myVelocity.y, myVelocity.x));
                    float v0 = Mathf.Sqrt(Mathf.Pow(myVelocity.x, 2) + Mathf.Pow(myVelocity.y, 2));
                    float angleDiff = velAngle - surfAngle;
                    float velKept = v0 * Mathf.Cos(angleDiff);
                    float velLost = v0 * Mathf.Sin(angleDiff);
                    float velKeptX = velKept * Mathf.Cos(surfAngle);
                    float velKeptY = -velKept * Mathf.Sin(surfAngle);
                    myVelocity = new(velKeptX, velKeptY);
                    float aL = velLost / Time.deltaTime;
                    float theta = surf.Rotation * Mathf.Deg2Rad;
                    float normMag = mass * aL;
                    float normX = normMag * Mathf.Sin(theta);
                    float normY = normMag * Mathf.Cos(theta);
                    collisionNormal = new(normX, normY);
                }
                else if (mySurface != surf)
                {
                    float velMag = Mathf.Sqrt(Mathf.Pow(myVelocity.x, 2) + Mathf.Pow(myVelocity.y, 2));
                    float velX = velMag * Mathf.Cos(-surfAngle);
                    float velY = velMag * Mathf.Sin(-surfAngle);
                    myVelocity = new(velX, velY);
                }
                mySurface = surf;
                float xPos = mySurface.Height * Mathf.Sin(surfAngle) + relPos.x * Mathf.Cos(surfAngle) + Height * Mathf.Sin(surfAngle) + mySurface.transform.position.x;
                float yPos = mySurface.Height * Mathf.Cos(surfAngle) - relPos.x * Mathf.Sin(surfAngle) + Height * Mathf.Cos(surfAngle) + mySurface.transform.position.y;
                transform.position = new(xPos, yPos);
                newSurf = surf;
                break;
            }
        }
        mySurface = newSurf;
    }

    Vector2 UpdateVelocity(Vector2 velocity, Vector2? normal = null)
    {
        var force = CalcForces(normal);
        var a = CalcAccel(force);
        accel = a;
        velocity = CalcVelocity(a, velocity);
        return velocity;
    }

    Vector2 CalcForces(Vector2? normal = null)
    {
        Vector2 mg = new(0, mass * -myEnv.gravity);
        Vector2 norm = normal ?? Vector2.zero;
        Vector2 stdNorm = Vector2.zero;
        Vector2 fric = Vector2.zero;
        Vector2 sum;
        if (mySurface != null)
        {
            float theta = mySurface.Rotation * Mathf.Deg2Rad;

            float normMag = Mathf.Sqrt(Mathf.Pow(norm.x, 2) + Mathf.Pow(norm.y, 2));
            stdNorm = new(-mg.y * Mathf.Cos(theta) * Mathf.Sin(theta), -mg.y * Mathf.Cos(theta) * Mathf.Cos(theta));
            if (normal == null)
            {
                normMag = -mg.y * Mathf.Cos(theta);
                float normX = normMag * Mathf.Sin(theta);
                float normY = normMag * Mathf.Cos(theta);
                norm = new(normX, normY);
            }
            float speed = Mathf.Sqrt(Mathf.Pow(myVelocity.x, 2) + Mathf.Pow(myVelocity.y, 2));
            if (speed > 0)
            {
                float fricMag = mySurface.fricCoefDyn * normMag;

                float fricX = 0;
                if (myVelocity.x > 0) fricX = -fricMag * Mathf.Cos(theta);
                else if (myVelocity.x < 0) fricX = fricMag * Mathf.Cos(theta);

                float fricY = 0;
                if (myVelocity.y > 0) fricY = -fricMag * Mathf.Sin(theta);
                else if (myVelocity.y < 0) fricY = fricMag * Mathf.Sin(theta);

                fric = new(fricX, fricY);

                sum = mg + norm + fric;
                var a = CalcAccel(sum);
                var vel = CalcVelocity(a, myVelocity);
                if (Mathf.Sign(vel.x) != 0 && Mathf.Sign(myVelocity.x) != 0 && Mathf.Sign(vel.x) != Mathf.Sign(myVelocity.x))
                {
                    fric.x = 0;
                    myVelocity.x = 0;
                }
                if (Mathf.Sign(vel.y) != 0 && Mathf.Sign(myVelocity.y) != 0 && Mathf.Sign(vel.y) != Mathf.Sign(myVelocity.y))
                {
                    fric.y = 0;
                    myVelocity.y = 0;
                }
            }
            else
            {
                float maxFricMag = mySurface.fricCoefSta * normMag;
                sum = mg + norm;

                float fricX = 0;
                if (sum.x > 0) fricX = -maxFricMag * Mathf.Cos(theta);
                else if (sum.x < 0) fricX = maxFricMag * Mathf.Cos(theta);
                if (Mathf.Abs(fricX) > Mathf.Abs(sum.x)) fricX = Mathf.Abs(sum.x) * Mathf.Sign(fricX);

                float fricY = 0;
                if (sum.y > 0) fricY = -maxFricMag * Mathf.Sin(theta);
                else if (sum.y < 0) fricY = maxFricMag * Mathf.Sin(theta);
                if (Mathf.Abs(fricY) > Mathf.Abs(sum.y)) fricY = Mathf.Abs(sum.y) * Mathf.Sign(fricY);

                fric = new(fricX, fricY);
            }
        }
        this.mg = mg;
        this.normal = norm;
        friction = fric;
        sum = mg + stdNorm + fric;
        return sum;
    }

    Vector2 CalcAccel(Vector2 force)
    {
        Vector2 accel = force / mass;
        return accel;
    }

    Vector2 CalcVelocity(Vector2 accel, Vector2 velocity)
    {
        float t = Time.deltaTime;
        return new(velocity.x + accel.x * t, velocity.y + accel.y * t);
    }

    Vector2 CalcNewPos(Vector2 velocity)
    {
        return new(transform.position.x + velocity.x * Time.deltaTime, transform.position.y + velocity.y * Time.deltaTime);
    }

    public static float ClampAngleRad(float angle)
    {
        if (angle > 2.0f * Mathf.PI) angle -= 2.0f * Mathf.PI;
        else if (angle < 0.0f) angle += 2.0f * Mathf.PI;
        return angle;
    }

    public static float ClampAngleDeg(float angle)
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
        var spawnPos = spawnSurface.SpawnPoint;
        float xDiff = -Height * Mathf.Sin(angle) + Width * Mathf.Cos(angle);
        float yDiff = Height * Mathf.Cos(angle) + Width * Mathf.Sin(angle);
        spawnPos = new(spawnPos.x + xDiff, spawnPos.y + yDiff);
        transform.position = spawnPos;
    }

    public void UpdateSurfaces()
    {
        surfaces.Clear();
        surfaces.AddRange(manager.surfaces);
        spawnSurface = surfaces.Last();
    }

    void UpdateVectorArrows()
    {
        fricArrow.vector = friction;
        mgArrow.vector = mg;
        normalArrow.vector = normal;
        accelArrow.vector = accel;
        velocityArrow.vector = myVelocity;
    }

    void InitVectorArrows()
    {
        toggleLabels[0].color = fricColor;
        fricArrow = InitVectorArrow(fricColor, forceScale, -0.45f);
        toggleLabels[1].color = mgColor;
        mgArrow = InitVectorArrow(mgColor, forceScale, 0.0f);
        toggleLabels[2].color = normalColor;
        normalArrow = InitVectorArrow(normalColor, forceScale, 0.0f);
        toggleLabels[3].color = accelColor;
        accelArrow = InitVectorArrow(accelColor, velScale, -0.2f);
        toggleLabels[4].color = velocityColor;
        velocityArrow = InitVectorArrow(velocityColor, velScale, 0.0f);
    }

    VectorArrow InitVectorArrow(Color color, float scale, float yOffset)
    {
        var arrowObj = Instantiate(arrowPrefab, transform);
        arrowObj.transform.localPosition = new(arrowObj.transform.localPosition.x, arrowObj.transform.localPosition.y + yOffset);
        var arrow = arrowObj.GetComponent<VectorArrow>();
        arrow.origin = arrowObj.transform.localPosition;
        arrow.SetColor(color);
        arrow.scaleFactor = scale;
        arrow.skier = gameObject;
        return arrow;
    }

    public void MGToggle(bool newVal)
    {
        if (newVal) mgArrow.Show();
        else mgArrow.Hide();
    }

    public void NormToggle(bool newVal)
    {
        if (newVal) normalArrow.Show();
        else normalArrow.Hide();
    }

    public void FricToggle(bool newVal)
    {
        if (newVal) fricArrow.Show();
        else fricArrow.Hide();
    }

    public void AccelToggle(bool newVal)
    {
        if (newVal) accelArrow.Show();
        else accelArrow.Hide();
    }

    public void VelToggle(bool newVal)
    {
        if (newVal) velocityArrow.Show();
        else velocityArrow.Hide();
    }

    public void MassChange(string newMass)
    {
        if (newMass != string.Empty)
        {
            mass = (float)Convert.ToDouble(newMass);
        }
        else mass = 0.0f;
    }
}
