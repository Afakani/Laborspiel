using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using P = PlanErfuellt.EditorTools.PanelUiBuilder;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Einstellungsfenster (Phase 12): fünf Regler, Grafik Hoch/Niedrig, Vollbild/Fenster, Springen an/aus, Tastenbelegung,
    /// Ton an/aus, Zurück. Liegt über der Pause; das Tastenfenster liegt darüber.
    /// </summary>
    public static class SettingsUiBuilder
    {
        static readonly Color Gold = new Color(1f, 0.82f, 0.29f);
        static readonly Color Dim = new Color(0.04f, 0.02f, 0.015f, 0.75f);

        public static SettingsUI Build(Transform canvas)
        {
            P.Init();
            var holder = new GameObject("SettingsOverlay", typeof(RectTransform));
            var hrt = (RectTransform)holder.transform;
            hrt.SetParent(canvas, false);
            P.Stretch(hrt);

            var root = new GameObject("Screen", typeof(RectTransform), typeof(Image));
            var rrt = (RectTransform)root.transform;
            rrt.SetParent(hrt, false);
            P.Stretch(rrt);
            root.GetComponent<Image>().color = Dim;

            // deckende Fläche hinter dem (leicht durchscheinenden) Schild; Name „Screen“ = vom Schild-Stil ausgenommen
            var backing = new GameObject("Screen", typeof(RectTransform), typeof(Image));
            var brt = (RectTransform)backing.transform;
            brt.SetParent(rrt, false);
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(0.5f, 0.5f);
            brt.sizeDelta = new Vector2(740f, 800f);
            backing.GetComponent<Image>().color = new Color(0.05f, 0.045f, 0.04f, 1f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            var frt = (RectTransform)frame.transform;
            frt.SetParent(rrt, false);
            frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
            frt.sizeDelta = new Vector2(760f, 820f);
            frame.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);

            var title = P.Text(frt, "Title", 44, Gold, new Vector2(40f, -26f), new Vector2(680f, 60f), TextAlignmentOptions.Center);
            var labels = new TMP_Text[8];
            var sliders = new Slider[5];
            float Row(int i) => -110f - i * 62f;
            for (int i = 0; i < labels.Length; i++)
            {
                float y = Row(i);
                labels[i] = P.Text(frt, "Label" + i, 24, P.Cream, new Vector2(60f, y), new Vector2(300f, 44f), TextAlignmentOptions.Left);
                if (i < 5) sliders[i] = P.MakeSlider(frt, "Slider" + i, new Vector2(370f, y - 2f), new Vector2(320f, 40f));
            }
            var high = P.MakeButton(frt, "High", new Vector2(370f, Row(5)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var low = P.MakeButton(frt, "Low", new Vector2(540f, Row(5)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var full = P.MakeButton(frt, "Fullscreen", new Vector2(370f, Row(6)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var windowed = P.MakeButton(frt, "Windowed", new Vector2(540f, Row(6)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var jumpOn = P.MakeButton(frt, "JumpOn", new Vector2(370f, Row(7)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var jumpOff = P.MakeButton(frt, "JumpOff", new Vector2(540f, Row(7)), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var keys = P.MakeButton(frt, "Keys", new Vector2(60f, Row(8)), new Vector2(630f, 52f), P.ButtonBg, Gold, 22, false);
            var sound = P.MakeButton(frt, "Sound", new Vector2(60f, -730f), new Vector2(300f, 56f), P.ButtonBg, Gold, 22, false);
            var back = P.MakeButton(frt, "Back", new Vector2(400f, -730f), new Vector2(290f, 56f), new Color(0.16f, 0.32f, 0.12f), new Color(0.43f, 1f, 0.29f), 24, false);
            BuildKeys(hrt);
            HudStyle.Apply(hrt);

            var ui = holder.AddComponent<SettingsUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            var lp = so.FindProperty("labels");
            lp.arraySize = labels.Length;
            for (int i = 0; i < labels.Length; i++) lp.GetArrayElementAtIndex(i).objectReferenceValue = labels[i];
            so.FindProperty("master").objectReferenceValue = sliders[0];
            so.FindProperty("music").objectReferenceValue = sliders[1];
            so.FindProperty("sfx").objectReferenceValue = sliders[2];
            so.FindProperty("mouse").objectReferenceValue = sliders[3];
            so.FindProperty("shake").objectReferenceValue = sliders[4];
            so.FindProperty("graphicsHigh").objectReferenceValue = high;
            so.FindProperty("graphicsLow").objectReferenceValue = low;
            so.FindProperty("fullscreen").objectReferenceValue = full;
            so.FindProperty("windowed").objectReferenceValue = windowed;
            so.FindProperty("jumpOn").objectReferenceValue = jumpOn;
            so.FindProperty("jumpOff").objectReferenceValue = jumpOff;
            so.FindProperty("keys").objectReferenceValue = keys;
            so.FindProperty("sound").objectReferenceValue = sound;
            so.FindProperty("back").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ui;
        }

        /// <summary>Tastenfenster: je belegbarer Taste (settings.rebind) eine Zeile mit Name, Taste und „Ändern“.</summary>
        static void BuildKeys(RectTransform holder)
        {
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>(BalancingImporter.ConfigPath);
            int count = cfg != null ? cfg.settings.rebind.Length : 0;
            var root = new GameObject("KeysScreen", typeof(RectTransform), typeof(Image));
            var rrt = (RectTransform)root.transform;
            rrt.SetParent(holder, false);
            P.Stretch(rrt);
            root.GetComponent<Image>().color = Dim;
            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            var frt = (RectTransform)frame.transform;
            frt.SetParent(rrt, false);
            frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
            frt.sizeDelta = new Vector2(820f, 230f + count * 54f);
            frame.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.98f);

            var title = P.Text(frt, "Title", 40, Gold, new Vector2(40f, -22f), new Vector2(740f, 56f), TextAlignmentOptions.Center);
            var rows = new KeyBindingsUI.Row[count];
            for (int i = 0; i < count; i++)
            {
                var bg = P.Img(frt, "Row_" + i, new Vector2(40f, -92f - i * 54f), new Vector2(740f, 48f), new Color(0.12f, 0.1f, 0.07f, 0.9f), null, false);
                rows[i] = new KeyBindingsUI.Row
                {
                    name = P.Text(bg.rectTransform, "Name", 22, P.Cream, new Vector2(16f, -6f), new Vector2(300f, 36f), TextAlignmentOptions.Left),
                    key = P.Text(bg.rectTransform, "Key", 22, Color.white, new Vector2(320f, -6f), new Vector2(260f, 36f), TextAlignmentOptions.Center, P.mono),
                    change = P.MakeButton(bg.rectTransform, "Change", new Vector2(-8f, -5f), new Vector2(140f, 38f), P.ButtonBg, Gold, 18, true)
                };
                rows[i].key.enableAutoSizing = true;
                rows[i].key.fontSizeMin = 12f;
                rows[i].key.fontSizeMax = 22f;
            }
            float bottom = -92f - count * 54f;
            var hint = P.Text(frt, "Hint", 16, P.Muted, new Vector2(40f, bottom - 6f), new Vector2(740f, 44f), TextAlignmentOptions.Center);
            hint.textWrappingMode = TMPro.TextWrappingModes.Normal;
            var reset = P.MakeButton(frt, "Reset", new Vector2(40f, bottom - 60f), new Vector2(300f, 54f), P.ButtonBg, Gold, 22, false);
            var back = P.MakeButton(frt, "Back", new Vector2(480f, bottom - 60f), new Vector2(300f, 54f), new Color(0.16f, 0.32f, 0.12f), new Color(0.43f, 1f, 0.29f), 24, false);

            var ui = holder.gameObject.AddComponent<KeyBindingsUI>();
            var so = new SerializedObject(ui);
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("hint").objectReferenceValue = hint;
            so.FindProperty("reset").objectReferenceValue = reset;
            so.FindProperty("back").objectReferenceValue = back;
            var rp = so.FindProperty("rows");
            rp.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                var e = rp.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("name").objectReferenceValue = rows[i].name;
                e.FindPropertyRelative("key").objectReferenceValue = rows[i].key;
                e.FindPropertyRelative("change").objectReferenceValue = rows[i].change;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
