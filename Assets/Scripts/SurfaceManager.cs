using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine.UI;

public class SurfaceManager : MonoBehaviour
{
    public List<Surface> surfaces = new();
    public List<SurfParamControl> surfControls = new();
    public GameObject surfControlCont;
    public GameObject surfControlPrefab;
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
        var controlObj = Instantiate(surfControlPrefab, surfControlCont.transform);
        var control = controlObj.GetComponent<SurfParamControl>();
        control.index = surfControls.Count;
        control.surface = surfObj;
        surfControls.Add(control);
        surface.GetComponent<SurfaceControls>().myParamControl = control;
        var controlContTrans = surfControlCont.GetComponent<RectTransform>();
        var height = surfControlPrefab.GetComponent<RectTransform>().sizeDelta.y;
        controlContTrans.sizeDelta = new(controlContTrans.sizeDelta.x, controlContTrans.sizeDelta.y + height);
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
            var control = surfControls.Last();
            surfControls.Remove(control);
            Destroy(control.gameObject);
            var controlContTrans = surfControlCont.GetComponent<RectTransform>();
            var height = surfControlPrefab.GetComponent<RectTransform>().sizeDelta.y;
            controlContTrans.sizeDelta = new(controlContTrans.sizeDelta.x, controlContTrans.sizeDelta.y - height);
        }
    }

    public void TrackSurface(GameObject surface)
    {
        followCam.Target.TrackingTarget = surface.transform;
        followCam.Lens.OrthographicSize = buildLens;
    }
}
