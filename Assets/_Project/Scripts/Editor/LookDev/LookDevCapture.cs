using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Vergleichsaufnahmen für den Look-Pass: rendert die Ego-Sicht (wie Bild 08) in eine PNG-Datei.
    /// Die Ego-Hände werden ausgeblendet, optional steht eine mutierte Kuh in der Kammer.
    /// </summary>
    public static class LookDevCapture
    {
        [MenuItem("Plan erfüllt/Look-Dev: Aufnahme Ego-Sicht")]
        public static void CaptureDefault() => Capture("Temp/lookdev.png", new Vector3(0.9f, 1.65f, -7.4f), new Vector3(-0.35f, 2.35f, 0f), 50f, true);

        public static void Capture(string path, Vector3 pos, Vector3 lookAt, float fov, bool showCow)
        {
            var main = Camera.main;
            var go = new GameObject("LookDevCam");
            var cam = go.AddComponent<Camera>();
            if (main != null) cam.CopyFrom(main);
            go.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 120f;
            cam.transform.position = pos;
            cam.transform.LookAt(lookAt);

            var hands = GameObject.Find("ViewModel");
            if (hands != null) hands.SetActive(false);
            var cow = GameObject.Find("Chamber/Animal/Cow");
            Vector3 cowScale = Vector3.one;
            if (showCow && cow != null)
            {
                cowScale = cow.transform.localScale;
                cow.SetActive(true);
                cow.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                cow.transform.localScale = Vector3.one * 1.35f;
            }

            const int w = 1672, h = 941;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            cam.targetTexture = null;
            RenderTexture.active = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(go);

            if (hands != null) hands.SetActive(true);
            if (showCow && cow != null)
            {
                cow.transform.localScale = cowScale;
                cow.SetActive(false);
            }
        }
    }
}
