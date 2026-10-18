using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Industrie-Stil für HUD und Panels (Art Direction, UI): dunkle Flächen bekommen ein abgenutztes Metallschild
    /// als 9-Slice (Rahmen mit Lichtkante, Messingnieten, Schmutz). Logbuch (Papier), Sendung und Pause bleiben unverändert.
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
            int n = 0;
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
                n++;
            }
            return n;
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
