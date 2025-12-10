using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

[ExecuteInEditMode]
public class Simulation : MonoBehaviour
{
    [SerializeField] float timestepFrequency = 60.0f;
    [SerializeField] float visualFrequency = 30.0f;
    [SerializeField] float gravitationalConstant = 1.0f;
    static float _gravConst;
    static float timestep;
    static float visualTimestep;
    [SerializeField] Body[] bodies;
    Body[] _bodies;
    [SerializeField] int trackIndex;
    [SerializeField] float cameraSize = 14.0f;
    [SerializeField] CinemachineCamera cam;
    [SerializeField] GameObject bodyPrefab;
    [SerializeField] GameObject trailPrefab;
    [SerializeField] bool showTrails = true;
    [SerializeField, Range(0.0f, 1.0f)] float trailOpacity = 0.5f;
    [SerializeField] int trailLength = 2000;
    [SerializeField] bool ShowPredictions = false;
    bool showPreds = false;
    [SerializeField] float PredictionLength = 1.0f;
    float predictionLength;
    IEnumerator timestepCoroutine;
    IEnumerator visualCoroutine;
    IEnumerator showPredUpdate;
    IEnumerator cameraUpdate;
    [SerializeField] Transform predictionHolder;
    [SerializeField] ComputeShader gravityShader;

    void Start()
    {
        _gravConst = gravitationalConstant;
        timestep = 1.0f / timestepFrequency;
        visualTimestep = 1.0f / visualFrequency;
        _bodies = CreatePredictBodies(bodies);
        PredictPaths();
        if (timestepCoroutine == null)
        {
            timestepCoroutine = Timestep();
            StartCoroutine(timestepCoroutine);
        }
        if (visualCoroutine == null)
        {
            visualCoroutine = UpdateVisuals();
            StartCoroutine(visualCoroutine);
        }
        if (showPredUpdate == null)
        {
            showPredUpdate = ShowPredUpdate();
            StartCoroutine(showPredUpdate);
        }
        if (cameraUpdate == null)
        {
            cameraUpdate = CameraSizeUpdate();
            StartCoroutine(cameraUpdate);
        }
    }

    IEnumerator UpdateVisuals()
    {
        while (true)
        {
            if (Application.isPlaying)
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

                    if (showTrails && body.showTrail)
                    {
                        var trail = Instantiate(trailPrefab, body.position, Quaternion.identity);
                        var trailSize = body.size / 4.0f;
                        trail.transform.localScale = new(trailSize, trailSize, trailSize);
                        var opacity = trailOpacity * body.velocity.magnitude * visualTimestep;
                        trail.GetComponent<SpriteRenderer>().color = new(body.color.r, body.color.g, body.color.b, opacity);
                        int length = (Mathf.RoundToInt(body.velocity.magnitude) != 0) ? Mathf.RoundToInt(((body.trailLength >= 0) ? body.trailLength : trailLength) / body.velocity.magnitude / visualTimestep) : 0;
                        body.trailFIFO.Size = length;
                        body.trailFIFO.AddTrail(trail);
                    }
                    else body.trailFIFO.ClearTrail();
                }
                if (trackIndex < bodies.Length && bodies.Length > 0 && trackIndex >= 0) cam.Target.TrackingTarget = bodies[trackIndex].body.transform;
                else cam.Target.TrackingTarget = transform;
                cam.Lens.OrthographicSize = cameraSize;
            }

            yield return new WaitForSecondsRealtime(visualTimestep);
        }
    }

    IEnumerator Timestep()
    {
        while (true)
        {
            if (Application.isPlaying)
            {
                UpdateBodies(bodies, gravityShader);
            }

            yield return new WaitForSecondsRealtime(timestep);
        }
    }

    void PredictPaths()
    {
        foreach (var body in bodies)
        {
            if (body.body == null)
            {
                body.body = Instantiate(bodyPrefab, body.position, Quaternion.identity);
                body.body.transform.localScale = Vector3.one * body.size;
                body.body.GetComponent<SpriteRenderer>().color = body.color;
            }
            body.body.transform.position = body.position;
        }
        if (showPreds && !Application.isPlaying)
        {
            var predBodies = CreatePredictBodies(bodies);
            float time = 0.0f;
            ClearPredictions();
            while (time < predictionLength)
            {
                UpdateBodies(predBodies, gravityShader);
                foreach (var body in predBodies)
                {
                    DrawPrediction(body);
                }
                time += timestep;
            }
        }
        else
        {
            ClearPredictions();
        }
    }

    void DrawPrediction(Body body)
    {
        var pred = Instantiate(trailPrefab, body.position, Quaternion.identity, predictionHolder);
        var predSize = body.size / 4.0f;
        pred.transform.localScale = new(predSize, predSize, predSize);
        var opacity = trailOpacity / 2.0f * body.velocity.magnitude * timestep;
        pred.GetComponent<SpriteRenderer>().color = new(body.color.r, body.color.g, body.color.b, opacity);
    }

    IEnumerator ShowPredUpdate()
    {
        WaitForSecondsRealtime delay = new(0.1f);
        while (true)
        {
            if (ShowPredictions != showPreds)
            {
                showPreds = ShowPredictions;
                PredictPaths();
            }
            else if (PredictionLength != predictionLength)
            {
                predictionLength = PredictionLength;
                PredictPaths();
            }
            else if (timestepFrequency != timestep)
            {
                timestep = 1.0f / timestepFrequency;
                PredictPaths();
            }
            else if (gravitationalConstant != _gravConst)
            {
                _gravConst = gravitationalConstant;
                PredictPaths();
            }
            else
            {
                for (int i = 0; i < _bodies.Length; i++)
                {
                    if (_bodies[i] != bodies[i])
                    {
                        _bodies = CreatePredictBodies(bodies);
                        PredictPaths();
                        break;
                    }
                }
            }
            yield return delay;
        }
    }

    IEnumerator CameraSizeUpdate()
    {
        WaitForSecondsRealtime delay = new(0.1f);
        while (true)
        {
            if (cam.Lens.OrthographicSize != cameraSize)
            {
                cam.Lens.OrthographicSize = cameraSize;
            }
            yield return delay;
        }
    }

    void ClearPredictions()
    {
        for (int i = predictionHolder.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(predictionHolder.GetChild(i).gameObject);
        }
    }

    static void UpdateBodies(Body[] bodies, ComputeShader shader)
    {
        BodyAclData[] data = new BodyAclData[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
        {
            data[i] = new(bodies[i]);
        }

        var kernelHandle = shader.FindKernel("CalcAccel");
        var aclInput = new ComputeBuffer(data.Length, BodyAclData.size);
        var pairAcl = new ComputeBuffer(data.Length * data.Length, sizeof(float) * 2);
        var aclOutput = new ComputeBuffer(data.Length, sizeof(float) * 2);
        aclInput.SetData(data);

        shader.SetBuffer(kernelHandle, "aclInput", aclInput);
        shader.SetBuffer(kernelHandle, "pairAccelBuffer", pairAcl);
        shader.SetFloat("gravConst", _gravConst);
        shader.SetFloat("timestep", timestep);
        shader.SetInt("bodyCount", bodies.Length);

        int xThreadGroups = Mathf.CeilToInt(data.Length / 8.0f);
        int yThreadGroups = Mathf.CeilToInt(data.Length / 8.0f);
        shader.Dispatch(kernelHandle, xThreadGroups, yThreadGroups, 1);

        kernelHandle = shader.FindKernel("ReduceAccel");
        shader.SetBuffer(kernelHandle, "pairAccelBuffer", pairAcl);
        shader.SetBuffer(kernelHandle, "aclOutput", aclOutput);
        xThreadGroups = Mathf.CeilToInt(data.Length / 64.0f);
        shader.Dispatch(kernelHandle, xThreadGroups, 1, 1);

        var aclData = new float2[bodies.Length];
        aclOutput.GetData(aclData);
        for (int i = 0; i < aclData.Length; i++)
        {
            bodies[i].acceleration = aclData[i];
        }

        aclInput.Release();
        pairAcl.Release();
        aclOutput.Release();

        BodyMoveData[] moveData = new BodyMoveData[bodies.Length];
        for (int i = 0; i < bodies.Length; i++)
        {
            moveData[i] = new(bodies[i]);
        }

        kernelHandle = shader.FindKernel("Move");
        var moveIn = new ComputeBuffer(data.Length, BodyMoveData.size);
        moveIn.SetData(moveData);

        shader.SetBuffer(kernelHandle, "moveIn", moveIn);
        xThreadGroups = Mathf.CeilToInt(moveData.Length / 64.0f);
        shader.Dispatch(kernelHandle, xThreadGroups, 1, 1);
        moveIn.GetData(moveData);
        for (int i = 0; i < moveData.Length; i++)
        {
            bodies[i].UpdateData(moveData[i]);
        }
        moveIn.Release();
    }

    static Body[] CreatePredictBodies(Body[] bodies)
    {
        var output = new Body[bodies.Length];
        for (int i = 0; i < bodies.Length; i++) output[i] = bodies[i].Clone();
        return output;
    }
}

public struct BodyAclData
{
    public float mass;
    public float2 pos;
    public static int size = sizeof(float) * 3;

    public BodyAclData(Body body)
    {
        mass = body.mass;
        pos = body.position;
    }
}

public struct BodyMoveData
{
    public float2 pos;
    public float2 vel;
    public float2 acl;
    public static int size = sizeof(float) * 6;

    public BodyMoveData(Body body)
    {
        pos = body.position;
        vel = body.velocity;
        acl = body.acceleration;
    }
}

[System.Serializable]
public class Body
{
    public float mass = 1000.0f;
    public Vector2 position = Vector2.zero;
    public Vector2 velocity = Vector2.zero;
    public Vector2 acceleration = Vector2.zero;
    [System.NonSerialized] public GameObject body;
    public float size = 1.0f;
    public Color color = Color.white;
    public bool showTrail = true;
    public int trailLength = -1;
    [System.NonSerialized] public TrailFIFO trailFIFO = new(0);

    public Body Clone()
    {
        return new() { mass = mass, position = position, velocity = velocity,
            acceleration = acceleration, body = null, size = size, color = color, showTrail = showTrail, trailLength = trailLength, trailFIFO = null };
    }

    public void UpdateData(BodyMoveData data)
    {
        position = data.pos;
        velocity = data.vel;
    }

    public override bool Equals(object obj)
    {
        if (obj is Body)
        {
            var other = obj as Body;
            if (mass == other.mass && position == other.position && velocity == other.velocity && acceleration == other.acceleration && body == other.body
                && size == other.size && color == other.color && showTrail == other.showTrail && trailLength == other.trailLength && trailFIFO == other.trailFIFO)
            {
                return true;
            }
            else return false;
        }
        else return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class TrailFIFO
{
    public int Size;
    readonly List<GameObject> trails = new();

    public void AddTrail(GameObject newTrail)
    {
        trails.Add(newTrail);
        while (trails.Count > Size)
        {
            var trail = trails.First();
            trails.RemoveAt(0);
            Object.Destroy(trail);
        }
    }

    public void ClearTrail()
    {
        foreach (var trail in trails) Object.DestroyImmediate(trail);
        trails.Clear();
    }

    public TrailFIFO(int size)
    {
        Size = size;
    }
}
