using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SurfaceManager : MonoBehaviour
{
    public List<Surface> surfaces = new List<Surface>();

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
    }
}
