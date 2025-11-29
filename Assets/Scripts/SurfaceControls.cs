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
    SurfaceManager manager;

    void Awake()
    {
        mySurface = GetComponent<Surface>();
        manager = FindFirstObjectByType<SurfaceManager>();
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

    public void SpawnSkier(GameObject instance, CinemachineCamera cam, float endPos)
    {
        if (instance != null)
        {
            skierInstance = instance;
        }
        bool newSkier = skierInstance == null;
        if (newSkier)
        {
            skierInstance = Instantiate(skierPrefab);
            manager.skierInstance = skierInstance;
        }
        var skier = skierInstance.GetComponent<Skier>();
        skier.endPos = endPos;
        cam.Target.TrackingTarget = skierInstance.transform;
        cam.Lens.OrthographicSize = manager.skierLens;
        if (!newSkier)
        {
            skier.Spawn();
        }
    }
}
