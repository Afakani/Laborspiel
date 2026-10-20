using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut die Panels Mischstation und Pult als uGUI (Stil wie Prototyp: dunkle Tafel, Messingrand,
    /// Creme/Gelb-Schrift). Texte setzen die Panels zur Laufzeit aus der JSON.
    /// </summary>
    public static class PanelUiBuilder
    {
        internal static readonly Color Bg = new Color(0.078f, 0.063f, 0.047f, 0.95f);
        internal static readonly Color Border = new Color(0.416f, 0.353f, 0.25f);
        internal static readonly Color Cream = new Color(0.96f, 0.93f, 0.82f);
        internal static readonly Color Muted = new Color(0.88f, 0.74f, 0.56f);
        internal static readonly Color Head = new Color(0.84f, 0.71f, 0.42f);
        internal static readonly Color ButtonBg = new Color(0.18f, 0.15f, 0.11f);
        static readonly Color LeverBorder = new Color(0.43f, 1f, 0.29f);
        static readonly Color StopBg = new Color(0.62f, 0.06f, 0.09f);
        static readonly Color StopBorder = new Color(0.95f, 0.72f, 0.02f);
        static readonly Color MixBg = new Color(0.16f, 0.23f, 0.13f);

        internal static TMP_FontAsset font, mono;
        static Sprite uiSprite, knob;

        internal static void Init()
        {
            font = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/Oswald/Oswald-Bold.ttf", $"{BlockoutKit.FontDir}/Oswald/Oswald-Bold SDF.asset");
            mono = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/JetBrainsMono/JetBrainsMono-Regular.ttf", $"{BlockoutKit.FontDir}/JetBrainsMono/JetBrainsMono-Regular SDF.asset");
            uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        // ---------- Mischstation ----------

        public static PanelMixing BuildMixing(Transform canvas)
        {
            Init();
            var host = new GameObject("PanelMixing", typeof(RectTransform));
            host.transform.SetParent(canvas, false);
            Stretch((RectTransform)host.transform);
            var root = Frame(host.transform, "Root", new Vector2(1200f, 470f));

            var title = Text(root, "Title", 30, Head, new Vector2(24f, -18f), new Vector2(400f, 40f), TextAlignmentOptions.Left);
            var rackCount = Text(root, "RackCount", 18, Muted, new Vector2(24f, -60f), new Vector2(900f, 28f), TextAlignmentOptions.Left, mono);
            var close = MakeButton(root, "Close", new Vector2(-24f, -18f), new Vector2(260f, 44f), ButtonBg, Border, 20, true);
            var dosing = Text(root, "DosingHeader", 18, Head, new Vector2(24f, -104f), new Vector2(560f, 26f), TextAlignmentOptions.Left);

            var rows = new PanelMixing.SliderRow[3];
            for (int i = 0; i < 3; i++)
            {
                float y = -142f - i * 74f;
                var row = new PanelMixing.SliderRow();
                row.dot = Img(root, "Dot" + i, new Vector2(24f, y - 12f), new Vector2(18f, 18f), Color.white, knob, true);
                row.label = Text(root, "Label" + i, 20, Cream, new Vector2(52f, y), new Vector2(220f, 40f), TextAlignmentOptions.Left);
                row.slider = MakeSlider(root, "Slider" + i, new Vector2(280f, y - 10f), new Vector2(200f, 22f));
                row.value = Text(root, "Value" + i, 24, Cream, new Vector2(490f, y), new Vector2(40f, 40f), TextAlignmentOptions.Right, mono);
                row.stock = Text(root, "Stock" + i, 16, Muted, new Vector2(540f, y), new Vector2(110f, 40f), TextAlignmentOptions.Left, mono);
                rows[i] = row;
            }

            var info = InfoBox(root, new Vector2(680f, -104f), new Vector2(496f, 260f));
            var mix = MakeButton(root, "Mix", new Vector2(680f, -380f), new Vector2(496f, 64f), MixBg, LeverBorder, 26, false);
            // Probenmikroskop (Phase 9)
            var probe = MakeButton(root, "Probe", new Vector2(24f, -330f), new Vector2(620f, 48f), ButtonBg, Border, 18, true);
            var probeText = Text(root, "ProbeText", 17, Muted, new Vector2(24f, -388f), new Vector2(620f, 64f), TextAlignmentOptions.TopLeft);
            probeText.textWrappingMode = TMPro.TextWrappingModes.Normal;

            var p = host.AddComponent<PanelMixing>();
            var so = new SerializedObject(p);
            so.FindProperty("panelId").stringValue = "mixing";
            so.FindProperty("root").objectReferenceValue = root.gameObject;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("rackCount").objectReferenceValue = rackCount;
            so.FindProperty("closeHint").objectReferenceValue = close.GetComponentInChildren<TMP_Text>();
            so.FindProperty("dosingHeader").objectReferenceValue = dosing;
            var rowsProp = so.FindProperty("rows");
            rowsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                var e = rowsProp.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("label").objectReferenceValue = rows[i].label;
                e.FindPropertyRelative("slider").objectReferenceValue = rows[i].slider;
                e.FindPropertyRelative("value").objectReferenceValue = rows[i].value;
                e.FindPropertyRelative("stock").objectReferenceValue = rows[i].stock;
                e.FindPropertyRelative("dot").objectReferenceValue = rows[i].dot;
            }
            so.FindProperty("info").objectReferenceValue = info;
            so.FindProperty("mixButton").objectReferenceValue = mix;
            so.FindProperty("mixLabel").objectReferenceValue = mix.GetComponentInChildren<TMP_Text>();
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("probeButton").objectReferenceValue = probe;
            so.FindProperty("probeLabel").objectReferenceValue = probe.GetComponentInChildren<TMP_Text>();
            so.FindProperty("probeText").objectReferenceValue = probeText;
            so.ApplyModifiedPropertiesWithoutUndo();
            return p;
        }

        // ---------- Pult ----------

        public static PanelControl BuildControl(Transform canvas)
        {
            Init();
            var host = new GameObject("PanelControl", typeof(RectTransform));
            host.transform.SetParent(canvas, false);
            Stretch((RectTransform)host.transform);
            var root = Frame(host.transform, "Root", new Vector2(1500f, 500f));

            var title = Text(root, "Title", 30, Head, new Vector2(24f, -18f), new Vector2(420f, 40f), TextAlignmentOptions.Left);
            var animal = Text(root, "AnimalStatus", 20, Head, new Vector2(460f, -22f), new Vector2(700f, 36f), TextAlignmentOptions.Left, mono);
            var close = MakeButton(root, "Close", new Vector2(-24f, -18f), new Vector2(260f, 44f), ButtonBg, Border, 20, true);

            // Spalte 1: Regal + Infobox
            var rackHeader = Text(root, "RackHeader", 18, Head, new Vector2(24f, -76f), new Vector2(600f, 26f), TextAlignmentOptions.Left);
            var rackList = Rect(root, "RackList", new Vector2(24f, -106f), new Vector2(600f, 188f));
            var grid = rackList.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(294f, 56f);
            grid.spacing = new Vector2(8f, 6f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            var rackEmpty = Text(root, "RackEmpty", 18, Muted, new Vector2(24f, -110f), new Vector2(600f, 40f), TextAlignmentOptions.Left);
            var vial = VialTemplate(root);
            var info = InfoBox(root, new Vector2(24f, -302f), new Vector2(600f, 180f));

            // Spalte 2: Hebel + Status
            var leverHeader = Text(root, "LeverHeader", 18, Head, new Vector2(660f, -76f), new Vector2(380f, 26f), TextAlignmentOptions.Left);
            var gas = MakeButton(root, "Gas", new Vector2(660f, -106f), new Vector2(380f, 50f), ButtonBg, LeverBorder, 20, false);
            var cow = MakeButton(root, "CloneCow", new Vector2(660f, -162f), new Vector2(380f, 50f), ButtonBg, LeverBorder, 20, false);
            var hen = MakeButton(root, "CloneHen", new Vector2(660f, -218f), new Vector2(380f, 50f), ButtonBg, LeverBorder, 20, false);
            var belt = MakeButton(root, "Belt", new Vector2(660f, -274f), new Vector2(380f, 50f), ButtonBg, LeverBorder, 20, false);
            var statusHeader = Text(root, "StatusHeader", 18, Head, new Vector2(660f, -336f), new Vector2(380f, 26f), TextAlignmentOptions.Left);
            var lamps = new PanelControl.Lamp[4];
            for (int i = 0; i < 4; i++)
            {
                float x = 660f + (i % 2) * 190f, y = -370f - (i / 2) * 40f;
                lamps[i] = new PanelControl.Lamp
                {
                    light = Img(root, "Lamp" + i, new Vector2(x, y - 8f), new Vector2(16f, 16f), Color.green, knob, true),
                    label = Text(root, "LampLabel" + i, 16, Cream, new Vector2(x + 24f, y), new Vector2(160f, 32f), TextAlignmentOptions.Left, mono)
                };
            }

            // Spalte 3: Aktionen
            var actionHeader = Text(root, "ActionHeader", 18, Head, new Vector2(1076f, -76f), new Vector2(400f, 26f), TextAlignmentOptions.Left);
            var antidote = MakeButton(root, "Antidote", new Vector2(1076f, -106f), new Vector2(400f, 50f), ButtonBg, Border, 20, false);
            var leash = MakeButton(root, "Leash", new Vector2(1076f, -162f), new Vector2(400f, 50f), ButtonBg, Border, 20, false);
            var clean = MakeButton(root, "Clean", new Vector2(1076f, -218f), new Vector2(400f, 50f), ButtonBg, Border, 20, false);
            var stop = MakeButton(root, "NotStop", new Vector2(1076f, -296f), new Vector2(400f, 96f), StopBg, StopBorder, 34, false);

            var p = host.AddComponent<PanelControl>();
            var so = new SerializedObject(p);
            so.FindProperty("panelId").stringValue = "pult";
            so.FindProperty("root").objectReferenceValue = root.gameObject;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("animalStatus").objectReferenceValue = animal;
            so.FindProperty("closeHint").objectReferenceValue = close.GetComponentInChildren<TMP_Text>();
            so.FindProperty("rackHeader").objectReferenceValue = rackHeader;
            so.FindProperty("leverHeader").objectReferenceValue = leverHeader;
            so.FindProperty("statusHeader").objectReferenceValue = statusHeader;
            so.FindProperty("actionHeader").objectReferenceValue = actionHeader;
            so.FindProperty("rackList").objectReferenceValue = rackList;
            so.FindProperty("vialTemplate").objectReferenceValue = vial;
            so.FindProperty("rackEmpty").objectReferenceValue = rackEmpty;
            so.FindProperty("info").objectReferenceValue = info;
            so.FindProperty("gas").objectReferenceValue = gas;
            so.FindProperty("cloneCow").objectReferenceValue = cow;
            so.FindProperty("cloneHen").objectReferenceValue = hen;
            so.FindProperty("belt").objectReferenceValue = belt;
            so.FindProperty("antidote").objectReferenceValue = antidote;
            so.FindProperty("leash").objectReferenceValue = leash;
            so.FindProperty("clean").objectReferenceValue = clean;
            so.FindProperty("stop").objectReferenceValue = stop;
            so.FindProperty("closeButton").objectReferenceValue = close;
            var lampsProp = so.FindProperty("lamps");
            lampsProp.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                var e = lampsProp.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("light").objectReferenceValue = lamps[i].light;
                e.FindPropertyRelative("label").objectReferenceValue = lamps[i].label;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return p;
        }

        // ---------- Bausteine ----------

        internal static RectTransform Frame(Transform parent, string name, Vector2 size)
        {
            var border = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)border.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 16f);
            rt.sizeDelta = size;
            border.GetComponent<Image>().color = Border;
            var inner = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)inner.transform;
            irt.SetParent(rt, false);
            Stretch(irt);
            irt.offsetMin = new Vector2(2f, 2f);
            irt.offsetMax = new Vector2(-2f, -2f);
            inner.GetComponent<Image>().color = Bg;
            return rt;
        }

        internal static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Rechteck oben links verankert (pos = Abstand von oben links; negative x = von rechts).</summary>
        internal static RectTransform Rect(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            bool right = pos.x < 0f;
            rt.anchorMin = rt.anchorMax = new Vector2(right ? 1f : 0f, 1f);
            rt.pivot = new Vector2(right ? 1f : 0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        internal static TextMeshProUGUI Text(Transform parent, string name, float size, Color color, Vector2 pos, Vector2 box, TextAlignmentOptions align, TMP_FontAsset f = null)
        {
            var rt = Rect(parent, name, pos, box);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = f != null ? f : font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.raycastTarget = false;
            t.text = "";
            return t;
        }

        internal static Image Img(Transform parent, string name, Vector2 pos, Vector2 size, Color color, Sprite sprite, bool raycast)
        {
            var rt = Rect(parent, name, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.sprite = sprite;
            img.raycastTarget = raycast && false;
            return img;
        }

        internal static Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size, Color bg, Color border, float textSize, bool small)
        {
            var rt = Rect(parent, name, pos, size);
            var frame = rt.gameObject.AddComponent<Image>();
            frame.color = border;
            var inner = new GameObject("Bg", typeof(RectTransform), typeof(Image));
            var irt = (RectTransform)inner.transform;
            irt.SetParent(rt, false);
            Stretch(irt);
            irt.offsetMin = new Vector2(2f, 2f);
            irt.offsetMax = new Vector2(-2f, -2f);
            var bgImg = inner.GetComponent<Image>();
            bgImg.color = bg;
            bgImg.raycastTarget = false;
            var label = new GameObject("Label", typeof(RectTransform));
            var lrt = (RectTransform)label.transform;
            lrt.SetParent(rt, false);
            Stretch(lrt);
            lrt.offsetMin = new Vector2(8f, 2f);
            lrt.offsetMax = new Vector2(-8f, -2f);
            var t = label.AddComponent<TextMeshProUGUI>();
            t.font = font;
            t.fontSize = textSize;
            t.color = Color.white;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.enableAutoSizing = true;
            t.fontSizeMin = 12f;
            t.fontSizeMax = textSize;
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = frame;
            var cb = b.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(1f, 1f, 0.8f);
            cb.pressedColor = new Color(0.8f, 0.8f, 0.7f);
            cb.disabledColor = new Color(1f, 1f, 1f, 0.3f);
            b.colors = cb;
            // Ausgegraute Knöpfe: ganze Fläche abdunkeln
            rt.gameObject.AddComponent<CanvasGroup>();
            rt.gameObject.AddComponent<ButtonDimmer>();
            return b;
        }

        internal static Slider MakeSlider(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var rt = Rect(parent, name, pos, size);
            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            var brt = (RectTransform)bg.transform;
            brt.SetParent(rt, false);
            Stretch(brt);
            brt.offsetMin = new Vector2(0f, 6f);
            brt.offsetMax = new Vector2(0f, -6f);
            var bgi = bg.GetComponent<Image>();
            bgi.sprite = uiSprite;
            bgi.type = Image.Type.Sliced;
            bgi.color = new Color(0.1f, 0.09f, 0.07f);

            var fillArea = new GameObject("FillArea", typeof(RectTransform));
            var frt = (RectTransform)fillArea.transform;
            frt.SetParent(rt, false);
            Stretch(frt);
            frt.offsetMin = new Vector2(4f, 6f);
            frt.offsetMax = new Vector2(-12f, -6f);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            var fillRt = (RectTransform)fill.transform;
            fillRt.SetParent(frt, false);
            fillRt.sizeDelta = new Vector2(8f, 0f);
            var fi = fill.GetComponent<Image>();
            fi.sprite = uiSprite;
            fi.type = Image.Type.Sliced;
            fi.color = Head;

            var handleArea = new GameObject("HandleArea", typeof(RectTransform));
            var hart = (RectTransform)handleArea.transform;
            hart.SetParent(rt, false);
            Stretch(hart);
            hart.offsetMin = new Vector2(10f, 0f);
            hart.offsetMax = new Vector2(-10f, 0f);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            var hrt = (RectTransform)handle.transform;
            hrt.SetParent(hart, false);
            hrt.sizeDelta = new Vector2(22f, 0f);
            var hi = handle.GetComponent<Image>();
            hi.sprite = knob;
            hi.color = Cream;

            var s = rt.gameObject.AddComponent<Slider>();
            s.fillRect = fillRt;
            s.handleRect = hrt;
            s.targetGraphic = hi;
            s.direction = Slider.Direction.LeftToRight;
            s.wholeNumbers = true;
            s.minValue = 0;
            s.maxValue = 8;
            return s;
        }

        static SerumInfoBox InfoBox(Transform parent, Vector2 pos, Vector2 size)
        {
            var rt = Rect(parent, "InfoBox", pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            // eingelassenes dunkles Feld (HudStyle)
            img.sprite = HudStyle.Sprite("HudInset.png", new Vector4(6f, 6f, 6f, 6f));
            img.type = Image.Type.Sliced;
            img.color = new Color(1f, 1f, 1f, 0.95f);
            img.raycastTarget = false;
            var title = Text(rt, "Title", 22, new Color(1f, 0.82f, 0.29f), new Vector2(12f, -8f), new Vector2(size.x - 24f, 32f), TextAlignmentOptions.Left);
            var body = Text(rt, "Body", 17, Cream, new Vector2(12f, -44f), new Vector2(size.x - 24f, size.y - 52f), TextAlignmentOptions.TopLeft);
            var box = rt.gameObject.AddComponent<SerumInfoBox>();
            var so = new SerializedObject(box);
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("body").objectReferenceValue = body;
            so.ApplyModifiedPropertiesWithoutUndo();
            return box;
        }

        static GameObject VialTemplate(Transform parent)
        {
            var rt = Rect(parent, "VialTemplate", Vector2.zero, new Vector2(294f, 56f));
            var bg = rt.gameObject.AddComponent<Image>();
            bg.color = new Color(0.42f, 0.35f, 0.25f);
            var stripe = Img(rt, "Stripe", new Vector2(10f, -10f), new Vector2(10f, 36f), Color.green, uiSprite, false);
            stripe.raycastTarget = false;
            Text(rt, "Name", 17, Cream, new Vector2(30f, -4f), new Vector2(258f, 26f), TextAlignmentOptions.Left);
            Text(rt, "Doses", 14, Muted, new Vector2(30f, -28f), new Vector2(258f, 24f), TextAlignmentOptions.Left, mono);
            var b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = bg;
            rt.gameObject.SetActive(false);
            return rt.gameObject;
        }
    }
}
