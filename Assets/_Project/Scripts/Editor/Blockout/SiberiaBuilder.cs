using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Spielende Sibirien (D162): Rolltor als Nische vor der Vorderwand rechts unter dem Laufsteg, zwei Stasi-Männer
    /// darin, Gegenlicht, eigene Kamera und Kinobalken-Overlay; dazu SiberiaCinematic. Maße aus siberia.
    /// </summary>
    public static class SiberiaBuilder
    {
        const float Depth = 0.8f;

        public static void Build(Transform systems, GameConfig cfg)
        {
            var old = systems.Find("Siberia");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var c = cfg.siberia;
            var root = new GameObject("Siberia").transform;
            root.SetParent(systems, false);

            // Nische: Rückwand, Seiten und Decke dunkel, vorn Stahlrahmen mit Rollkasten
            var gate = BlockoutKit.Group(root, "Gate", LayoutConfig.ToUnity(c.gateX, 0f, c.gateZ + 0.12f));
            float w = c.gateWidth, h = c.gateHeight;
            var dark = BlockoutKit.Mat("LD_GateDark", LabMaterials.Hex("#0B0C0E"), 0f, 0.1f);
            var steel = LookDevMaterials.SteelFrame;
            LookDevKit.Box(gate, "Back", new Vector3(0f, h * 0.5f, 0.02f), new Vector3(w, h, 0.04f), dark);
            foreach (float x in new[] { -1f, 1f })
                LookDevKit.Box(gate, x < 0f ? "SideL" : "SideR", new Vector3(x * (w * 0.5f + 0.05f), h * 0.5f, -Depth * 0.5f), new Vector3(0.1f, h, Depth), dark);
            LookDevKit.Box(gate, "Ceiling", new Vector3(0f, h + 0.05f, -Depth * 0.5f), new Vector3(w + 0.2f, 0.1f, Depth), dark);
            foreach (float x in new[] { -1f, 1f })
                LookDevKit.Box(gate, x < 0f ? "PostL" : "PostR", new Vector3(x * (w * 0.5f + 0.12f), h * 0.5f, -Depth), new Vector3(0.24f, h, 0.24f), steel);
            LookDevKit.Box(gate, "Housing", new Vector3(0f, h + 0.18f, -Depth), new Vector3(w + 0.48f, 0.36f, 0.4f), steel);
            BlockoutKit.Sign(gate, "Sign", "ui.signs.gate", new Vector3(0f, h + 0.18f, -Depth - 0.21f), 2.2f, LabMaterials.Gold, LabMaterials.SignPlate, false);

            // Rolltor (bewegt sich, nicht statisch) mit Warnstreifen unten
            var shutterMat = LookDevKit.Pbr("LD_GateShutter", "painted_metal_shutter", new Color(0.55f, 0.52f, 0.44f), 1.0f, 0.8f);
            var shutter = BlockoutKit.Group(gate, "Shutter", new Vector3(0f, h * 0.5f, -Depth - 0.02f));
            LookDevKit.Box(shutter, "Panel", Vector3.zero, new Vector3(w, h, 0.05f), shutterMat, null, false);
            LookDevKit.Box(shutter, "Stripe", new Vector3(0f, -h * 0.5f + 0.12f, -0.03f), new Vector3(w, 0.16f, 0.01f), LookDevMaterials.RedPaint, null, false);

            // Gegenlicht aus der Nische auf den Spieler
            var lightGo = new GameObject("BackLight");
            lightGo.transform.SetParent(gate, false);
            lightGo.transform.localPosition = new Vector3(0f, h - 0.3f, -0.1f);
            lightGo.transform.localRotation = Quaternion.Euler(25f, 180f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Spot;
            light.spotAngle = 110f;
            light.range = 14f;
            light.shadows = LightShadows.Soft;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            ColorUtility.TryParseHtmlString(c.lightColor, out var lc);
            light.color = lc;
            light.intensity = 0f;
            light.enabled = false;

            // Zwei Stasi-Männer in der Nische, Blick in die Halle (−Z)
            var men = new Transform[2];
            var legsL = new Transform[2];
            var legsR = new Transform[2];
            for (int i = 0; i < 2; i++)
            {
                float x = (i - 0.5f) * c.stasiSpacing;
                men[i] = LookDevStasi.Build(gate, "Stasi" + i, new Vector3(x, 0f, -Depth * 0.5f), out legsL[i], out legsR[i], i == 0);
                men[i].localEulerAngles = new Vector3(0f, 180f, 0f);
                men[i].localScale = Vector3.one * c.stasiScale;
                men[i].gameObject.SetActive(false);
            }

            // Kamera der Zwischensequenz
            var camGo = new GameObject("SiberiaCam");
            camGo.transform.SetParent(root, false);
            var cam = camGo.AddComponent<Camera>();
            cam.depth = 10f;
            cam.nearClipPlane = 0.05f;
            cam.enabled = false;
            camGo.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;

            // Overlay: Kinobalken, Untertitel, Blende
            var canvasGo = new GameObject("SiberiaOverlay", typeof(RectTransform));
            canvasGo.transform.SetParent(root, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;
            var top = Bar(canvasGo.transform, "BarTop", new Vector2(0f, 1f - c.letterbox), Vector2.one);
            var bottom = Bar(canvasGo.transform, "BarBottom", Vector2.zero, new Vector2(1f, c.letterbox));
            var font = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/Oswald/Oswald-Bold.ttf", $"{BlockoutKit.FontDir}/Oswald/Oswald-Bold SDF.asset");
            var subGo = new GameObject("Subtitle", typeof(RectTransform));
            subGo.transform.SetParent(bottom, false);
            var srt = (RectTransform)subGo.transform;
            srt.anchorMin = Vector2.zero;
            srt.anchorMax = Vector2.one;
            srt.offsetMin = new Vector2(120f, 8f);
            srt.offsetMax = new Vector2(-120f, -8f);
            var sub = subGo.AddComponent<TextMeshProUGUI>();
            sub.font = font;
            sub.fontSize = 36f;
            sub.alignment = TextAlignmentOptions.Center;
            sub.color = new Color(0.96f, 0.93f, 0.82f);
            sub.raycastTarget = false;
            var fade = Bar(canvasGo.transform, "Fade", Vector2.zero, Vector2.one).GetComponent<Image>();
            canvasGo.SetActive(false);

            root.gameObject.AddComponent<SiberiaCinematic>().Setup(cam, shutter, light, men, legsL, legsR, canvasGo, top, bottom, fade, sub);
        }

        static RectTransform Bar(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;
            return rt;
        }
    }
}
