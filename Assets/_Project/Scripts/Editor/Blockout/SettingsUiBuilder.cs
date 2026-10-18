using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using P = PlanErfuellt.EditorTools.PanelUiBuilder;

namespace PlanErfuellt.EditorTools
{
    /// <summary>Einstellungsfenster (Phase 12): fünf Regler, Grafik Hoch/Niedrig, Vollbild/Fenster, Ton an/aus, Zurück. Liegt über der Pause.</summary>
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
            brt.sizeDelta = new Vector2(740f, 670f);
            backing.GetComponent<Image>().color = new Color(0.05f, 0.045f, 0.04f, 1f);

            var frame = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            var frt = (RectTransform)frame.transform;
            frt.SetParent(rrt, false);
            frt.anchorMin = frt.anchorMax = frt.pivot = new Vector2(0.5f, 0.5f);
            frt.sizeDelta = new Vector2(760f, 690f);
            frame.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.04f, 0.97f);

            var title = P.Text(frt, "Title", 44, Gold, new Vector2(40f, -26f), new Vector2(680f, 60f), TextAlignmentOptions.Center);
            var labels = new TMP_Text[7];
            var sliders = new Slider[5];
            for (int i = 0; i < 7; i++)
            {
                float y = -116f - i * 68f;
                labels[i] = P.Text(frt, "Label" + i, 24, P.Cream, new Vector2(60f, y), new Vector2(300f, 44f), TextAlignmentOptions.Left);
                if (i < 5) sliders[i] = P.MakeSlider(frt, "Slider" + i, new Vector2(370f, y - 2f), new Vector2(320f, 40f));
            }
            var high = P.MakeButton(frt, "High", new Vector2(370f, -456f), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var low = P.MakeButton(frt, "Low", new Vector2(540f, -456f), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var full = P.MakeButton(frt, "Fullscreen", new Vector2(370f, -524f), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var windowed = P.MakeButton(frt, "Windowed", new Vector2(540f, -524f), new Vector2(150f, 48f), P.ButtonBg, Gold, 22, false);
            var sound = P.MakeButton(frt, "Sound", new Vector2(60f, -600f), new Vector2(300f, 56f), P.ButtonBg, Gold, 22, false);
            var back = P.MakeButton(frt, "Back", new Vector2(400f, -600f), new Vector2(290f, 56f), new Color(0.16f, 0.32f, 0.12f), new Color(0.43f, 1f, 0.29f), 24, false);
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
            so.FindProperty("sound").objectReferenceValue = sound;
            so.FindProperty("back").objectReferenceValue = back;
            so.ApplyModifiedPropertiesWithoutUndo();
            return ui;
        }
    }
}
