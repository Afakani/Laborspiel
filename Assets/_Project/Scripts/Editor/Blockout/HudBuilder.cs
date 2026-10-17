using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Erzeugt das HUD-Prefab für Phase 3: Fadenkreuz, Maus-Hinweis, Interaktions-Prompt mit Haltebalken,
    /// Kurzmeldungen und Pause-Overlay. Stil: dunkle Industrieschilder, Schrift Creme/Gelb (Art Direction UI).
    /// </summary>
    public static class HudBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/UI/HUD.prefab";

        static TMP_FontAsset font;
        static readonly Color Panel = new Color(0.094f, 0.067f, 0.047f, 0.92f);
        static readonly Color Cream = new Color(0.96f, 0.93f, 0.82f);
        static readonly Color Gold = new Color(1f, 0.82f, 0.29f);

        public static GameObject Build()
        {
            font = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/Oswald/Oswald-Bold.ttf", $"{BlockoutKit.FontDir}/Oswald/Oswald-Bold SDF.asset");

            var root = new GameObject("HUD", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            // Fadenkreuz und Maus-Hinweis
            var crossRoot = Rect("Crosshair", root.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var dot = Img("Dot", crossRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(7f, 7f), new Color(1f, 0.93f, 0.67f, 0.9f));
            dot.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            var hint = Text("MouseHint", crossRoot, "", 22, Cream, new Vector2(0f, -60f), new Vector2(600f, 40f));
            var cross = crossRoot.gameObject.AddComponent<Crosshair>();
            PlayerBuilder.Set(cross, "dot", dot.gameObject);
            PlayerBuilder.Set(cross, "mouseHint", hint);

            // Prompt unten mittig
            var prompt = Rect("Prompt", root.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 96f), new Vector2(620f, 74f));
            var frame = prompt.gameObject.AddComponent<Image>();
            frame.color = Gold;
            var inner = Img("Inner", prompt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel);
            inner.rectTransform.offsetMin = new Vector2(3f, 3f);
            inner.rectTransform.offsetMax = new Vector2(-3f, -3f);
            var label = Text("Label", inner.rectTransform, "", 24, Cream, new Vector2(0f, 10f), new Vector2(590f, 36f));
            var barBg = Img("BarBg", inner.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(580f, 10f), new Color(0.05f, 0.07f, 0.06f));
            var fill = Img("Fill", barBg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Gold);
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            var promptUi = root.AddComponent<PromptUI>();
            PlayerBuilder.Set(promptUi, "root", prompt.gameObject);
            PlayerBuilder.Set(promptUi, "label", label);
            PlayerBuilder.Set(promptUi, "frame", frame);
            PlayerBuilder.Set(promptUi, "fill", fill);
            prompt.gameObject.SetActive(false);

            // Kurzmeldungen rechts
            var toastArea = Rect("Toasts", root.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -220f), new Vector2(520f, 400f));
            toastArea.pivot = new Vector2(1f, 1f);
            var layout = toastArea.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperRight;
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var template = BuildToastTemplate(root.transform);
            var toastUi = root.AddComponent<ToastUI>();
            PlayerBuilder.Set(toastUi, "container", toastArea);
            PlayerBuilder.Set(toastUi, "template", template);

            // Pause-Overlay
            var pause = Img("PauseOverlay", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.016f, 0.024f, 0.6f));
            var title = Text("Title", pause.rectTransform, "", 72, Gold, new Vector2(0f, 20f), new Vector2(900f, 100f));
            var sub = Text("Hint", pause.rectTransform, "", 26, Cream, new Vector2(0f, -50f), new Vector2(900f, 40f));
            var pm = root.AddComponent<PauseMenu>();
            PlayerBuilder.Set(pm, "overlay", pause.gameObject);
            PlayerBuilder.Set(pm, "title", title);
            PlayerBuilder.Set(pm, "hint", sub);
            pause.gameObject.SetActive(false);
            return root;
        }

        static GameObject BuildToastTemplate(Transform parent)
        {
            var t = Rect("ToastTemplate", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(520f, 44f));
            var frame = t.gameObject.AddComponent<Image>();
            frame.color = Cream;
            t.gameObject.AddComponent<CanvasGroup>();
            var fit = t.gameObject.AddComponent<LayoutElement>();
            fit.minHeight = 44f;
            var inner = Img("Inner", t, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Panel);
            inner.rectTransform.offsetMin = new Vector2(2f, 2f);
            inner.rectTransform.offsetMax = new Vector2(-2f, -2f);
            var txt = Text("Text", inner.rectTransform, "", 20, Cream, Vector2.zero, Vector2.zero);
            txt.rectTransform.anchorMin = Vector2.zero;
            txt.rectTransform.anchorMax = Vector2.one;
            txt.rectTransform.offsetMin = new Vector2(12f, 4f);
            txt.rectTransform.offsetMax = new Vector2(-12f, -4f);
            txt.enableAutoSizing = true;
            txt.fontSizeMin = 14f;
            txt.fontSizeMax = 20f;
            t.gameObject.SetActive(false);
            return t.gameObject;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static Image Img(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
        {
            var rt = Rect(name, parent, anchorMin, anchorMax, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color, Vector2 pos, Vector2 box)
        {
            var rt = Rect(name, parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, box);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.text = text;
            t.raycastTarget = false;
            return t;
        }
    }
}
