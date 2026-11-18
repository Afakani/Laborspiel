using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// HUD-Teile für Phase 4b (GDD 7.6): obere Leiste mit Marke, Kasse, Rate, Tag/Plan; Lager-Chips links oben;
    /// Tier-Meter links unten mit 12 Segmenten je Wert.
    /// </summary>
    public static class HudStatusBuilder
    {
        static readonly Color Panel = new Color(0.094f, 0.055f, 0.039f, 0.9f);
        static readonly Color Red = new Color(0.82f, 0.1f, 0.1f);
        static readonly Color Cream = new Color(0.96f, 0.93f, 0.82f);
        static readonly Color Muted = new Color(0.79f, 0.66f, 0.54f);
        static readonly Color Gold = new Color(1f, 0.82f, 0.29f);
        static readonly Color Line = new Color(0.48f, 0.35f, 0.23f);

        static TMP_FontAsset font, mono;

        public static void Build(Transform canvas, int segments)
        {
            font = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/Oswald/Oswald-Bold.ttf", $"{BlockoutKit.FontDir}/Oswald/Oswald-Bold SDF.asset");
            mono = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/JetBrainsMono/JetBrainsMono-Bold.ttf", $"{BlockoutKit.FontDir}/JetBrainsMono/JetBrainsMono-Bold SDF.asset");
            BuildTopBar(canvas);
        }

        static void BuildTopBar(Transform canvas)
        {
            var bar = Rect(canvas, "TopBar", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 56f));
            bar.gameObject.AddComponent<Image>().color = Panel;
            var under = Rect(bar, "RedLine", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 3f));
            under.gameObject.AddComponent<Image>().color = Red;

            var logoBg = Rect(bar, "LogoBg", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(64f, 34f));
            logoBg.gameObject.AddComponent<Image>().color = Red;
            var logo = Text(logoBg, "Logo", 22, Color.white, Vector2.zero, new Vector2(64f, 34f), TextAlignmentOptions.Center, font, new Vector2(0.5f, 0.5f));
            var brand = Text(bar, "Brand", 20, Cream, new Vector2(96f, 0f), new Vector2(520f, 40f), TextAlignmentOptions.Left, font, new Vector2(0f, 0.5f));
            brand.characterSpacing = 6f;

            var cashLabel = Text(bar, "CashLabel", 16, Muted, new Vector2(-150f, 0f), new Vector2(80f, 40f), TextAlignmentOptions.Right, font, new Vector2(0.5f, 0.5f));
            var cash = Text(bar, "Cash", 30, Gold, new Vector2(10f, 0f), new Vector2(220f, 44f), TextAlignmentOptions.Left, mono, new Vector2(0.5f, 0.5f));
            var rate = Text(bar, "Rate", 18, Gold, new Vector2(170f, 0f), new Vector2(140f, 40f), TextAlignmentOptions.Left, mono, new Vector2(0.5f, 0.5f));

            var day = Text(bar, "Day", 18, Cream, new Vector2(-170f, 0f), new Vector2(220f, 40f), TextAlignmentOptions.Right, font, new Vector2(1f, 0.5f));
            var plan = Text(bar, "Plan", 18, Cream, new Vector2(-24f, 0f), new Vector2(140f, 40f), TextAlignmentOptions.Right, font, new Vector2(1f, 0.5f));

            // Geld-Zahlen steigen rechts neben der Rate auf (MoneyPopupUI)
            var popup = Text(bar, "MoneyPopup", 24, Gold, new Vector2(330f, -14f), new Vector2(200f, 34f), TextAlignmentOptions.Left, mono, new Vector2(0.5f, 0.5f));
            var pop = bar.gameObject.AddComponent<MoneyPopupUI>();
            var pso = new SerializedObject(pop);
            pso.FindProperty("template").objectReferenceValue = popup;
            pso.ApplyModifiedPropertiesWithoutUndo();

            var hud = bar.gameObject.AddComponent<HudTopBar>();
            var so = new SerializedObject(hud);
            so.FindProperty("logo").objectReferenceValue = logo;
            so.FindProperty("brand").objectReferenceValue = brand;
            so.FindProperty("cashLabel").objectReferenceValue = cashLabel;
            so.FindProperty("cash").objectReferenceValue = cash;
            so.FindProperty("rate").objectReferenceValue = rate;
            so.FindProperty("day").objectReferenceValue = day;
            so.FindProperty("plan").objectReferenceValue = plan;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        static TextMeshProUGUI Text(Transform parent, string name, float size, Color color, Vector2 pos, Vector2 box, TextAlignmentOptions align, TMP_FontAsset f, Vector2 anchor)
        {
            var rt = Rect(parent, name, anchor, anchor, anchor, pos, box);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = f;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.text = "";
            return t;
        }
    }
}
