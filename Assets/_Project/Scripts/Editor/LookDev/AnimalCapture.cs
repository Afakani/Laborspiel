using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Vergleichsaufnahmen der Tiermodelle (Vorher/Nachher): Prefab abseits des Labors auf grauem Boden,
    /// eigene Lichter, Blick schräg von vorn. Optional wird vorher eine Funktion auf die Instanz angewendet (Pose, Mutation).
    /// </summary>
    public static class AnimalCapture
    {
        static readonly Vector3 Spot = new Vector3(0f, -60f, 0f);

        [MenuItem("Plan erfüllt/Look-Dev: Aufnahme Tiere")]
        public static void CaptureBoth()
        {
            Capture(AnimalBuilder.CowPath, "Temp/animal_cow.png", 1f, null);
            Capture(AnimalBuilder.HenPath, "Temp/animal_hen.png", 1f, null);
        }

        /// <summary>Rendert das Prefab; zoom &gt; 1 rückt die Kamera näher.</summary>
        public static void Capture(string prefabPath, string outPath, float zoom, System.Action<GameObject> prepare)
        {
            var root = new GameObject("AnimalCaptureRig");
            root.transform.position = Spot;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
            inst.transform.SetParent(root.transform, false);
            inst.SetActive(true);
            prepare?.Invoke(inst);

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.SetParent(root.transform, false);
            floor.transform.localScale = Vector3.one * 3f;
            var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            floorMat.SetColor("_BaseColor", new Color(0.32f, 0.31f, 0.3f));
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            var key = new GameObject("Key").AddComponent<Light>();
            key.transform.SetParent(root.transform, false);
            key.type = LightType.Spot;
            key.range = 20f;
            key.spotAngle = 70f;
            key.intensity = 60f;
            key.color = new Color(1f, 0.93f, 0.82f);
            key.shadows = LightShadows.Soft;
            key.transform.localPosition = new Vector3(2.5f, 4.5f, -3.5f);
            key.transform.LookAt(Spot + Vector3.up);
            var fill = new GameObject("Fill").AddComponent<Light>();
            fill.transform.SetParent(root.transform, false);
            fill.type = LightType.Point;
            fill.range = 12f;
            fill.intensity = 8f;
            fill.color = new Color(0.75f, 0.85f, 1f);
            fill.transform.localPosition = new Vector3(-3f, 2.5f, -2f);

            var camGo = new GameObject("Cam");
            camGo.transform.SetParent(root.transform, false);
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.13f);
            cam.fieldOfView = 32f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 40f;
            // Rahmen aus den starren Teilen (Hufe, Augen, Hörner …; Haut-Bounds sind absichtlich zu groß), zoom > 1 rückt näher
            var rs = inst.GetComponentsInChildren<MeshRenderer>();
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            var dir = new Vector3(2.4f, 1.3f, -5.4f).normalized;
            camGo.transform.position = b.center + dir * (b.size.magnitude * 1.65f / zoom);
            camGo.transform.LookAt(b.center);

            const int w = 1280, h = 860;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(outPath, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(floorMat);
            Object.DestroyImmediate(root);
        }
    }
}
