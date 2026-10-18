using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut den Baumodus in die Szene (GDD 6.10): sichtbares Raster (gesperrte Felder rot), Feldmarkierung,
    /// Draufsicht-Kamera nach buildView, Liste der im Baumodus ausgeblendeten Umgebung, BuildSystem mit den
    /// Maschinen-Prefabs und BuildMode.
    /// </summary>
    public static class BuildModeBuilder
    {
        public static void Build(Transform gridRoot, Transform envRoot, GameConfig cfg)
        {
            var g = cfg.buildMachines.grid;

            // Raster (nur im Baumodus aktiv)
            var grid = new GameObject("GridCells");
            grid.transform.SetParent(gridRoot, false);
            var cellMat = LookDevKit.Additive("LD_BuildCell", "GridCell.png", Color.white, false);
            var blockedMat = LookDevKit.Additive("LD_BuildCellBlocked", "GridCell.png", new Color(1f, 0.3f, 0.25f, 0.7f), false);
            for (int gx = 0; gx < g.cols; gx++)
            for (int gz = 0; gz < g.rows; gz++)
            {
                var c = BuildLogic.CellCenter(cfg, gx, gz);
                var mat = BuildLogic.Blocked(cfg, gx, gz) ? blockedMat : cellMat;
                var quad = BlockoutKit.Quad(grid.transform, $"Cell_{gx}_{gz}", LayoutConfig.ToUnity(c.x, 0.03f, c.y), new Vector2(g.cell * 0.97f, g.cell * 0.97f), mat, false, new Vector3(90f, 0f, 0f));
                Object.DestroyImmediate(quad.GetComponent<Collider>());
                quad.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Feldmarkierung (Farbe per Laufzeit)
            var markerMat = BlockoutKit.Transparent("LD_BuildMarker", new Color(1f, 0.82f, 0.29f, 0.4f), 0f, new Color(0.3f, 0.3f, 0.1f));
            var marker = BlockoutKit.Quad(gridRoot, "CellMarker", Vector3.zero, new Vector2(g.cell * 0.95f, g.cell * 0.95f), markerMat, false, new Vector3(90f, 0f, 0f));
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            var markerR = marker.GetComponent<Renderer>();
            markerR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Draufsicht-Kamera (Position setzt BuildMode aus buildView)
            var camGo = new GameObject("BuildCamera");
            camGo.transform.SetParent(gridRoot, false);
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 120f;
            cam.fieldOfView = cfg.buildView.fov;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = LabMaterials.Hex("#1C1612");
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            cam.enabled = false;

            // Rückwand, linke Wand und Decke für die Draufsicht ausblenden
            var v = cfg.buildView;
            var hide = new List<Renderer>();
            foreach (var r in envRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (r is ParticleSystemRenderer) continue;
                var b = r.bounds;
                if (b.center.z < -v.hideBehindZ || b.center.x < v.hideLeftX || b.center.y > v.hideAboveY) hide.Add(r);
            }

            // Maschinen und Modus
            var machinesRoot = new GameObject("Machines");
            machinesRoot.transform.SetParent(gridRoot, false);
            var host = new GameObject("BuildMode");
            host.transform.SetParent(gridRoot, false);
            var sys = host.AddComponent<BuildSystem>();
            sys.Setup(LookDevBuildMachines.BuildAll(), LookDevBuildMachines.Pipe(), machinesRoot.transform);
            var mode = host.AddComponent<BuildMode>();
            mode.Setup(cam, grid, markerR, hide.ToArray(), sys, LookDevBuildMachines.Ghost());
        }
    }
}
