using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Industrie-Stil für HUD und Panels (Art Direction, UI): dunkle Flächen bekommen ein abgenutztes Metallschild
    /// als 9-Slice (Rahmen mit Lichtkante, Messingnieten, Schmutz), große Panels einen roten Zierstreifen mit Goldkante.
    /// Knöpfe bekommen eine Fase (helle Ober-, dunkle Unterkante) mit Hover-/Druck-Abstufung, graue Infofelder werden
    /// eingelassene dunkle Felder, Schieberegler Messingknöpfe. Logbuch (Papier), Sendung und Pause bleiben ohne Schild.
    /// </summary>
    public static class HudStyle
    {
        const string TexDir = "Assets/_Project/Art/Textures/LookDev";
        const float MaxDark = 0.13f, MinAlpha = 0.8f, MinWidth = 100f, MinHeight = 40f;
        static readonly string[] Skip = { "PanelLogbook", "NewsOverlay", "PauseOverlay" };
        const string FullScreenName = "Screen";

        /// <summary>Schild-Sprite auf alle großen, dunklen, ungemusterten Flächen unter root legen.</summary>
        public static int Apply(Transform root)
        {
            var plate = Sprite("HudPlate.png", new Vector4(18f, 18f, 18f, 18f));
            int n = StyleButtons(root) + StyleSliders(root) + StyleInsets(root);
            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite != null || InSkipped(img.transform, root)) continue;
                // Vollbild-Hintergründe (Start, Ende, Bestellung, Einstellungen) bleiben dunkel und deckend
                if (img.name == FullScreenName) continue;
                var c = img.color;
                if (c.a < MinAlpha || Mathf.Max(c.r, Mathf.Max(c.g, c.b)) > MaxDark) continue;
                var rt = img.rectTransform;
                bool wide = rt.anchorMin.x != rt.anchorMax.x || Mathf.Abs(rt.rect.width) >= MinWidth;
                bool tall = rt.anchorMin.y != rt.anchorMax.y || Mathf.Abs(rt.rect.height) >= MinHeight;
                if (!wide || !tall) continue;
                img.sprite = plate;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2f;
                img.color = new Color(1f, 1f, 1f, c.a);
                if (Mathf.Abs(rt.rect.width) >= AccentMinWidth && Mathf.Abs(rt.rect.height) >= AccentMinHeight) Accent(rt);
                n++;
            }
            return n;
        }

        const float AccentMinWidth = 600f, AccentMinHeight = 300f;

        /// <summary>Roter Zierstreifen mit Goldkante oben im Panel (hinter dem Inhalt).</summary>
        static void Accent(RectTransform panel)
        {
            if (panel.Find("Accent") != null) return;
            var go = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(panel, false);
            rt.SetAsFirstSibling();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(18f, -14f);
            rt.offsetMax = new Vector2(-18f, -7f);
            var img = go.GetComponent<Image>();
            img.sprite = Sprite("HudAccent.png", new Vector4(2f, 0f, 2f, 0f));
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
        }

        /// <summary>Knöpfe mit Fase; Grundfarbe bleibt (Tint), Hover hellt auf, Drücken dunkelt ab.</summary>
        public static int StyleButtons(Transform root)
        {
            var sprite = Sprite("HudButton.png", new Vector4(10f, 10f, 10f, 10f));
            int n = 0;
            foreach (var b in root.GetComponentsInChildren<Button>(true))
            {
                AddHoverFx(b, sprite);
                if (InPaper(b.transform, root) || !(b.targetGraphic is Image img) || img.sprite != null) continue;
                img.sprite = sprite;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 2f;
                // Knöpfe aus PanelUiBuilder: Rahmen + Innenfläche „Bg“ – die Fase gehört auf die Fläche
                var inner = b.transform.Find("Bg");
                if (inner != null && inner.GetComponent<Image>() is Image fill && fill.sprite == null)
                {
                    fill.sprite = sprite;
                    fill.type = Image.Type.Sliced;
                    fill.pixelsPerUnitMultiplier = 2f;
                }
                var cb = b.colors;
                cb.normalColor = new Color(0.86f, 0.86f, 0.86f, 1f);
                cb.highlightedColor = Color.white;
                cb.selectedColor = new Color(0.86f, 0.86f, 0.86f, 1f);
                cb.pressedColor = new Color(0.62f, 0.62f, 0.62f, 1f);
                cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.55f);
                cb.colorMultiplier = 1.15f;
                cb.fadeDuration = 0.08f;
                b.colors = cb;
                b.transition = Selectable.Transition.ColorTint;
                n++;
            }
            return n;
        }

        /// <summary>Hover-/Klick-Rückmeldung (ButtonHoverFx) mit eigener Leuchtfläche über dem Hintergrund, unter dem Text.</summary>
        static void AddHoverFx(Button b, Sprite sprite)
        {
            if (b.GetComponent<ButtonHoverFx>() != null) return;
            var go = new GameObject("HoverGlow", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(b.transform, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var bg = b.transform.Find("Bg");
            rt.SetSiblingIndex(bg != null ? bg.GetSiblingIndex() + 1 : 0);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 2f;
            img.color = Color.clear;
            img.raycastTarget = false;
            b.gameObject.AddComponent<ButtonHoverFx>().Setup(img);
        }

        /// <summary>Schieberegler: Messingknopf, eingelassene Spur.</summary>
        static int StyleSliders(Transform root)
        {
            var knob = Sprite("HudKnob.png");
            var inset = Sprite("HudInset.png", new Vector4(6f, 6f, 6f, 6f));
            int n = 0;
            foreach (var s in root.GetComponentsInChildren<Slider>(true))
            {
                if (s.handleRect != null && s.handleRect.GetComponent<Image>() is Image h && h.sprite != knob)
                {
                    h.sprite = knob;
                    h.color = Color.white;
                    h.preserveAspect = true;
                    var size = h.rectTransform.sizeDelta;
                    h.rectTransform.sizeDelta = new Vector2(Mathf.Max(size.x, 26f), Mathf.Max(size.y, 26f));
                    n++;
                }
                var bg = s.transform.Find("Background");
                if (bg != null && bg.GetComponent<Image>() is Image bi && bi.sprite != inset)
                {
                    bi.sprite = inset;
                    bi.type = Image.Type.Sliced;
                    bi.color = Color.white;
                }
            }
            return n;
        }

        const float InsetMinGrey = 0.13f, InsetMaxGrey = 0.62f, InsetMaxTint = 0.08f, InsetMinWidth = 150f, InsetMinHeight = 60f;

        /// <summary>Graue, ungemusterte Infofelder werden eingelassene dunkle Felder.</summary>
        static int StyleInsets(Transform root)
        {
            var inset = Sprite("HudInset.png", new Vector4(6f, 6f, 6f, 6f));
            int n = 0;
            foreach (var img in root.GetComponentsInChildren<Image>(true))
            {
                if (img.sprite != null || img.GetComponent<Selectable>() != null || InSkipped(img.transform, root) || img.name == FullScreenName) continue;
                var c = img.color;
                float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b)), min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
                if (max <= InsetMinGrey || max > InsetMaxGrey || max - min > InsetMaxTint) continue;
                var r = img.rectTransform.rect;
                if (Mathf.Abs(r.width) < InsetMinWidth || Mathf.Abs(r.height) < InsetMinHeight) continue;
                img.sprite = inset;
                img.type = Image.Type.Sliced;
                img.color = new Color(1f, 1f, 1f, Mathf.Max(0.9f, c.a));
                n++;
            }
            return n;
        }

        /// <summary>Logbuch ist Papier: keine Metallknöpfe.</summary>
        static bool InPaper(Transform t, Transform root)
        {
            for (; t != null && t != root; t = t.parent)
                if (t.name == "PanelLogbook") return true;
            return false;
        }

        static bool InSkipped(Transform t, Transform root)
        {
            for (; t != null && t != root; t = t.parent)
                foreach (var s in Skip)
                    if (t.name == s) return true;
            return false;
        }

        /// <summary>Selbst erzeugte Textur als Sprite (optional mit 9-Slice-Rand) importieren.</summary>
        public static Sprite Sprite(string file, Vector4 border = default)
        {
            string path = $"{TexDir}/{file}";
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            if (imp.textureType != TextureImporterType.Sprite || imp.spriteBorder != border)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.spriteImportMode = SpriteImportMode.Single;
                imp.alphaIsTransparency = true;
                imp.mipmapEnabled = false;
                imp.spriteBorder = border;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
