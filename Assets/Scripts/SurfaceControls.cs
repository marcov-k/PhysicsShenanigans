using UnityEngine;

[RequireComponent(typeof(Surface))]
public class SurfaceControls : MonoBehaviour
{
    public float rotation = 0.0f;
    public float fricCoef = 0.2f;
    Surface mySurface;
    [SerializeField] GameObject skierPrefab;
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
        mySurface.fricCoef = fricCoef;
    }

    public void SpawnSkier()
    {
        if (skierInstance != null)
        {
            Destroy(skierInstance);
        }
        skierInstance = Instantiate(skierPrefab);
        var skier = skierInstance.GetComponent<Skier>();
        skier.spawnSurface = mySurface;
    }
}
