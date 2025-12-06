using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class Simulation : MonoBehaviour
{
    [SerializeField] float timestepFrequency = 60.0f;
    [SerializeField] float visualFrequency = 30.0f;
    [SerializeField] float gravitationalConstant = 1.0f;
    static float _gravConst;
    static float timestep;
    static float visualTimestep;
    [SerializeField] Body[] bodies;
    [SerializeField] int trackIndex;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] GameObject bodyPrefab;
    IEnumerator timestepCoroutine;
    IEnumerator visualCoroutine;

    void Start()
    {
        _gravConst = gravitationalConstant;
        timestep = 1.0f / timestepFrequency;
        visualTimestep = 1.0f / visualFrequency;
        timestepCoroutine = Timestep();
        visualCoroutine = UpdateVisuals();
        StartCoroutine(timestepCoroutine);
        StartCoroutine(visualCoroutine);
    }

    IEnumerator UpdateVisuals()
    {
        while (true)
        {
            foreach (var body in bodies)
            {
                if (body.body == null)
                {
                    body.body = Instantiate(bodyPrefab);
                    body.body.transform.localScale = Vector3.one * body.size;
                    body.body.GetComponent<SpriteRenderer>().color = body.color;
                }
                body.body.transform.position = body.position;
            }
            if (trackIndex < bodies.Length && bodies.Length > 0 && trackIndex >= 0) cam.Target.TrackingTarget = bodies[trackIndex].body.transform;
            else cam.Target.TrackingTarget = transform;

            yield return new WaitForSeconds(visualTimestep);
        }
    }

    IEnumerator Timestep()
    {
        while (true)
        {
            foreach (var body in bodies)
            {
                body.acceleration = Vector2.zero;
                foreach (var other in bodies)
                {
                    if (other != body)
                    {
                        AccelerateBody(body, other);
                    }
                    else continue;
                }
            }
            foreach (var body in bodies)
            {
                MoveBody(body);
            }

            yield return new WaitForSeconds(1.0f / timestepFrequency);
        }
    }

    static void AccelerateBody(Body body, Body other)
    {
        var diff = other.position - body.position;
        var r = diff.magnitude;
        var rRecip = 1.0f / r;
        var r2Recip = 1.0f / (r * r);
        var gm = _gravConst * other.mass * r2Recip;
        Vector2 a = new(gm * diff.x * rRecip, gm * diff.y * rRecip);
        body.acceleration = new(body.acceleration.x + a.x, body.acceleration.y + a.y);
    }

    static void MoveBody(Body body)
    {
        body.velocity = new(body.velocity.x + body.acceleration.x * timestep, body.velocity.y + body.acceleration.y * timestep);
        body.position = new(body.position.x + body.velocity.x * timestep, body.position.y + body.velocity.y * timestep);
    }
}

[System.Serializable]
public class Body
{
    public float mass = 1000.0f;
    public Vector2 position = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public Vector2 acceleration = Vector2.zero;
    public GameObject body;
    public float size = 1.0f;
    public Color color = Color.white;
}
