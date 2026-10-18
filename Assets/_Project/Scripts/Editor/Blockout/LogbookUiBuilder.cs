using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using P = PlanErfuellt.EditorTools.PanelUiBuilder;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Forschungslogbuch (GDD 12) im Stil eines Notizbuchs (Bild 08): Ledereinband, cremefarbenes Papier,
    /// Bleistiftschrift, scrollbare Kartenliste. Karten- und Abschnittsvorlagen werden zur Laufzeit vervielfältigt.
    /// </summary>
    public static class LogbookUiBuilder
    {
        const string TexDir = "Assets/_Project/Art/Textures/LookDev";
        static readonly Color Leather = new Color(0.27f, 0.16f, 0.1f);
        static readonly Color PaperC = new Color(0.93f, 0.89f, 0.78f);
        static readonly Color Ink = new Color(0.2f, 0.15f, 0.11f);
        static readonly Color InkSoft = new Color(0.38f, 0.3f, 0.22f);

        public static LogbookPanel Build(Transform canvas)
        {
            P.Init();
            var host = new GameObject("PanelLogbook", typeof(RectTransform));
            host.transform.SetParent(canvas, false);
            P.Stretch((RectTransform)host.transform);

            // Einband und Papier
            var root = new GameObject("Root", typeof(RectTransform), typeof(Image));
            var rootRt = (RectTransform)root.transform;
            rootRt.SetParent(host.transform, false);
            rootRt.anchorMin = rootRt.anchorMax = rootRt.pivot = new Vector2(0.5f, 0.5f);
            rootRt.sizeDelta = new Vector2(1380f, 920f);
            root.GetComponent<Image>().color = Leather;
            var paper = Img(rootRt, "Paper", PaperC);
            P.Stretch(paper.rectTransform);
            paper.rectTransform.offsetMin = new Vector2(18f, 18f);
            paper.rectTransform.offsetMax = new Vector2(-18f, -18f);
            var pr = paper.rectTransform;

            var title = P.Text(pr, "Title", 40, Ink, new Vector2(40f, -22f), new Vector2(900f, 52f), TextAlignmentOptions.Left);
            var intro = P.Text(pr, "Intro", 19, InkSoft, new Vector2(40f, -78f), new Vector2(1100f, 52f), TextAlignmentOptions.TopLeft);
            intro.textWrappingMode = TextWrappingModes.Normal;
            var close = P.MakeButton(pr, "Close", new Vector2(-30f, -22f), new Vector2(240f, 48f), new Color(0.82f, 0.76f, 0.62f), Leather, 20, true);
            close.GetComponentInChildren<TMP_Text>().color = Ink;
            // rote Randlinie wie im Schulheft
            var margin = Img(pr, "Margin", new Color(0.78f, 0.25f, 0.22f, 0.7f));
            margin.rectTransform.anchorMin = new Vector2(0f, 0f);
            margin.rectTransform.anchorMax = new Vector2(0f, 1f);
            margin.rectTransform.sizeDelta = new Vector2(3f, 0f);
            margin.rectTransform.anchoredPosition = new Vector2(26f, 0f);

            // Scrollbereich
            var view = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewRt = (RectTransform)view.transform;
            viewRt.SetParent(pr, false);
            viewRt.anchorMin = new Vector2(0f, 0f);
            viewRt.anchorMax = new Vector2(1f, 1f);
            viewRt.offsetMin = new Vector2(40f, 24f);
            viewRt.offsetMax = new Vector2(-30f, -140f);
            view.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var contentRt = (RectTransform)content.transform;
            contentRt.SetParent(viewRt, false);
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = Vector2.zero;
            var vl = content.GetComponent<VerticalLayoutGroup>();
            vl.spacing = 12f;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.childForceExpandHeight = false;
            vl.childForceExpandWidth = true;
            vl.padding = new RectOffset(4, 16, 4, 16);
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = view.AddComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewRt;
            scroll.horizontal = false;
            scroll.scrollSensitivity = 40f;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            // Abschnittsüberschrift (Vorlage)
            var section = new GameObject("SectionTemplate", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            section.transform.SetParent(contentRt, false);
            section.font = P.font;
            section.fontSize = 28;
            section.color = new Color(0.55f, 0.12f, 0.1f);
            section.alignment = TextAlignmentOptions.BottomLeft;
            section.textWrappingMode = TextWrappingModes.Normal;
            section.raycastTarget = false;
            section.gameObject.AddComponent<LayoutElement>().minHeight = 48f;

            var card = BuildCard(contentRt);

            var panel = host.AddComponent<LogbookPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("panelId").stringValue = "logbook";
            so.FindProperty("root").objectReferenceValue = root;
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("intro").objectReferenceValue = intro;
            so.FindProperty("closeLabel").objectReferenceValue = close.GetComponentInChildren<TMP_Text>();
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("content").objectReferenceValue = contentRt;
            so.FindProperty("cardTemplate").objectReferenceValue = card;
            so.FindProperty("sectionTemplate").objectReferenceValue = section;
            so.ApplyModifiedPropertiesWithoutUndo();
            return panel;
        }

        /// <summary>Kartenvorlage: Papierkarte, links Skizze, rechts Name, Dosis, Vor- und Nachteile.</summary>
        static LogbookCard BuildCard(RectTransform parent)
        {
            var go = new GameObject("CardTemplate", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            go.GetComponent<LayoutElement>().minHeight = 200f;
            var paper = go.GetComponent<Image>();
            paper.color = new Color(0.95f, 0.91f, 0.8f);
            // Schatten unten
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            shadow.effectDistance = new Vector2(3f, -3f);

            var sketch = new GameObject("Sketch", typeof(RectTransform));
            var sk = (RectTransform)sketch.transform;
            sk.SetParent(rt, false);
            sk.anchorMin = sk.anchorMax = new Vector2(0f, 0.5f);
            sk.pivot = new Vector2(0f, 0.5f);
            sk.anchoredPosition = new Vector2(14f, 0f);
            sk.sizeDelta = new Vector2(150f, 150f);
            var frame = SpriteImg(sk, "Frame", "SketchFrame.png", Vector2.zero, new Vector2(150f, 150f));
            var animal = SpriteImg(sk, "Animal", "SketchCow.png", Vector2.zero, new Vector2(150f, 150f));
            var udder = SpriteImg(animal.rectTransform, "Udder", "SketchUdder.png", new Vector2(4f, -24f), new Vector2(46f, 46f));

            var title = P.Text(rt, "Title", 26, Ink, new Vector2(184f, -12f), new Vector2(1000f, 34f), TextAlignmentOptions.Left);
            var sub = P.Text(rt, "Sub", 17, InkSoft, new Vector2(184f, -46f), new Vector2(1000f, 26f), TextAlignmentOptions.Left, P.mono);
            var body = P.Text(rt, "Body", 18, Ink, new Vector2(184f, -76f), new Vector2(1000f, 120f), TextAlignmentOptions.TopLeft);
            body.textWrappingMode = TextWrappingModes.Normal;

            var card = go.AddComponent<LogbookCard>();
            card.Setup(paper, title, sub, body, sk, animal, udder, frame, LoadSprite("SketchCow.png"), LoadSprite("SketchHen.png"));
            return card;
        }

        static Image Img(RectTransform parent, string name, Color c)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = c;
            img.raycastTarget = false;
            return img;
        }

        static Image SpriteImg(RectTransform parent, string name, string file, Vector2 pos, Vector2 size)
        {
            var img = Img(parent, name, Color.white);
            var r = img.rectTransform;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = pos;
            r.sizeDelta = size;
            img.sprite = LoadSprite(file);
            img.preserveAspect = true;
            return img;
        }

        /// <summary>Selbst erzeugte Skizze als Sprite importieren.</summary>
        static Sprite LoadSprite(string file)
        {
            string path = $"{TexDir}/{file}";
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp.textureType != TextureImporterType.Sprite)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.alphaIsTransparency = true;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
