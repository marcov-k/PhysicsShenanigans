using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Surface))]
public class SurfaceControls : MonoBehaviour
{
    public float rotation = 0.0f;
    public float fricCoefDyn = 0.2f;
    public float fricCoefSta = 0.3f;
    Surface mySurface;
    public GameObject skierPrefab;
    GameObject skierInstance;

    void Awake()
    {
        mySurface = GetComponent<Surface>();
    }

    void Update()
    {
        UpdateRotation();
        UpdateParams();
    }

    void UpdateRotation()
    {
        rotation = Mathf.Clamp(rotation, 0.0f, 90.0f);
        mySurface.Rotation = rotation;
    }

    void UpdateParams()
    {
        mySurface.fricCoefDyn = fricCoefDyn;
        mySurface.fricCoefSta = fricCoefSta;
    }

    public void SpawnSkier(CinemachineCamera cam, float endPos)
    {
        bool newSkier = skierInstance == null;
        if (newSkier)
        {
            skierInstance = Instantiate(skierPrefab);
            cam.Target.TrackingTarget = skierInstance.transform;
        }
        var skier = skierInstance.GetComponent<Skier>();
        skier.spawnSurface = mySurface;
        skier.endPos = endPos;
        if (!newSkier)
        {
            skier.Spawn();
        }
    }
}
