using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.Cinemachine;

public class SurfaceManager : MonoBehaviour
{
    public List<Surface> surfaces = new List<Surface>();
    public GameObject shortPrefab;
    public GameObject mediumPrefab;
    public GameObject longPrefab;
    public GameObject slopeEnd;
    public GameObject skierPrefab;
    public Button spawnButton;
    public CinemachineCamera followCam;
    public GameObject skierInstance = null;
    public float skierLens = 5.0f;
    public float buildLens = 7.5f;

    void Start()
    {
        ConfigureSurfaces();
    }

    public void ConfigureSurfaces()
    {
        if (skierInstance != null)
        {
            skierInstance.GetComponent<Skier>().UpdateSurfaces();
        }
        if (surfaces.Count > 1)
        {
            surfaces.First().nextSurface = surfaces[1];
            for (int i = 1; i < surfaces.Count - 1; i++)
            {
                surfaces[i].prevSurface = surfaces[i - 1];
                surfaces[i].nextSurface = surfaces[i + 1];
                surfaces[i].first = false;
            }
            surfaces.Last().prevSurface = surfaces[^2];
        }
        if (surfaces.Count > 0)
        {
            surfaces.First().Origin = slopeEnd.transform.position;
            surfaces.First().first = true;
            var control = surfaces.Last().GetComponent<SurfaceControls>();
            control.skierPrefab = skierPrefab;
            spawnButton.onClick.RemoveAllListeners();
            spawnButton.onClick.AddListener(() => control.SpawnSkier(skierInstance, followCam, slopeEnd.transform.position.x));
        }
    }

    public void AddShortSurface()
    {
        AddSurface(shortPrefab);
    }

    public void AddMediumSurface()
    {
        AddSurface(mediumPrefab);
    }

    public void AddLongSurface()
    {
        AddSurface(longPrefab);
    }

    void AddSurface(GameObject prefab)
    {
        var surfObj = Instantiate(prefab);
        var surface = surfObj.GetComponent<Surface>();
        surfaces.Add(surface);
        ConfigureSurfaces();
        if (skierInstance != null)
        {
            skierInstance.GetComponent<Skier>().UpdateSurfaces();
        }
        followCam.Target.TrackingTarget = surfObj.transform;
        followCam.Lens.OrthographicSize = buildLens;
    }

    public void RemoveSurface()
    {
        if (surfaces.Count > 1)
        {
            var surface = surfaces.Last();
            surfaces.Remove(surface);
            surfaces.Last().nextSurface = null;
            followCam.Target.TrackingTarget = surfaces.Last().transform;
            followCam.Lens.OrthographicSize = buildLens;
            Destroy(surface.gameObject);
            if (skierInstance != null)
            {
                skierInstance.GetComponent<Skier>().UpdateSurfaces();
            }
        }
    }
}
