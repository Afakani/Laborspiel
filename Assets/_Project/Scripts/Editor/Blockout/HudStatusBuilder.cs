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
            BuildChips(canvas);
            BuildMeter(canvas, segments);
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

        static void BuildChips(Transform canvas)
        {
            var row = Rect(canvas, "Chips", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -70f), new Vector2(900f, 54f));
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8f;
            hl.childControlWidth = false;
            hl.childControlHeight = false;
            hl.childForceExpandWidth = false;
            var chips = new HudChips.Chip[5];
            for (int i = 0; i < 5; i++)
            {
                var c = Rect(row, "Chip" + i, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(i < 4 ? 150f : 190f, 54f));
                var frame = c.gameObject.AddComponent<Image>();
                frame.color = i == 4 ? new Color(0.43f, 1f, 0.29f) : Line;
                var bg = Rect(c, "Bg", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                bg.offsetMin = new Vector2(2f, 2f);
                bg.offsetMax = new Vector2(-2f, -2f);
                bg.gameObject.AddComponent<Image>().color = Panel;
                chips[i] = new HudChips.Chip
                {
                    frame = frame,
                    symbol = Text(c, "Symbol", 30, Cream, new Vector2(10f, 0f), new Vector2(34f, 50f), TextAlignmentOptions.Center, font, new Vector2(0f, 0.5f)),
                    name = Text(c, "Name", 13, Muted, new Vector2(50f, 10f), new Vector2(i < 4 ? 96f : 136f, 22f), TextAlignmentOptions.Left, font, new Vector2(0f, 0.5f)),
                    count = Text(c, "Count", 16, Cream, new Vector2(50f, -11f), new Vector2(96f, 22f), TextAlignmentOptions.Left, mono, new Vector2(0f, 0.5f))
                };
            }
            var hc = row.gameObject.AddComponent<HudChips>();
            var so = new SerializedObject(hc);
            var p = so.FindProperty("chips");
            p.arraySize = 5;
            for (int i = 0; i < 5; i++)
            {
                var e = p.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("symbol").objectReferenceValue = chips[i].symbol;
                e.FindPropertyRelative("name").objectReferenceValue = chips[i].name;
                e.FindPropertyRelative("count").objectReferenceValue = chips[i].count;
                e.FindPropertyRelative("frame").objectReferenceValue = chips[i].frame;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildMeter(Transform canvas, int segments)
        {
            var box = Rect(canvas, "AnimalMeter", Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(20f, 20f), new Vector2(430f, 150f));
            box.gameObject.AddComponent<Image>().color = new Color(0.055f, 0.047f, 0.039f, 0.86f);
            var info = Text(box, "Info", 16, Gold, new Vector2(14f, -10f), new Vector2(400f, 26f), TextAlignmentOptions.Left, font, new Vector2(0f, 1f));
            var labels = new TMP_Text[3];
            var segs = new Image[3][];
            // Symbole wie im Pult-Bild: Flasche (Menge), Stern (Qualität), Sanduhr (Stabilität)
            string[] icons = { "IconBottle.png", "IconStar.png", "IconHourglass.png" };
            for (int r = 0; r < 3; r++)
            {
                float y = -46f - r * 34f;
                var icon = Rect(box, "Icon" + r, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, y - 3f), new Vector2(20f, 20f)).gameObject.AddComponent<Image>();
                icon.sprite = HudStyle.Sprite(icons[r]);
                icon.color = Gold;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                labels[r] = Text(box, "Label" + r, 15, Cream, new Vector2(40f, y), new Vector2(108f, 26f), TextAlignmentOptions.Left, font, new Vector2(0f, 1f));
                segs[r] = new Image[segments];
                for (int i = 0; i < segments; i++)
                {
                    var s = Rect(box, "Seg" + r + "_" + i, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150f + i * 22f, y - 6f), new Vector2(18f, 16f));
                    var img = s.gameObject.AddComponent<Image>();
                    img.color = new Color(0.16f, 0.15f, 0.13f);
                    img.raycastTarget = false;
                    segs[r][i] = img;
                }
            }
            var m = box.gameObject.AddComponent<AnimalMeter>();
            var so = new SerializedObject(m);
            so.FindProperty("info").objectReferenceValue = info;
            var lp = so.FindProperty("labels");
            lp.arraySize = 3;
            for (int i = 0; i < 3; i++) lp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            string[] names = { "amount", "quality", "stability" };
            for (int r = 0; r < 3; r++)
            {
                var p = so.FindProperty(names[r]);
                p.arraySize = segments;
                for (int i = 0; i < segments; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = segs[r][i];
            }
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
