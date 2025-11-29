using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.Cinemachine;

public class SurfaceManager : MonoBehaviour
{
    public List<Surface> surfaces = new List<Surface>();
    public GameObject slopeEnd;
    public GameObject skierPrefab;
    public Button spawnButton;
    public CinemachineCamera followCam;

    void Start()
    {
        ConfigureSurfaces();
    }

    public void ConfigureSurfaces()
    {
        if (surfaces.Count > 1)
        {
            surfaces.First().nextSurface = surfaces[1];
            for (int i = 1; i < surfaces.Count - 1; i++)
            {
                surfaces[i].prevSurface = surfaces[i - 1];
                surfaces[i].nextSurface = surfaces[i + 1];
            }
            surfaces.Last().prevSurface = surfaces[^2];
        }
        if (surfaces.Count > 0)
        {
            surfaces.First().Origin = slopeEnd.transform.position;
            surfaces.First().first = true;
            var control = surfaces.Last().GetComponent<SurfaceControls>();
            control.skierPrefab = skierPrefab;
            spawnButton.onClick.AddListener(() => control.SpawnSkier(followCam, slopeEnd.transform.position.x));
        }
    }
}
