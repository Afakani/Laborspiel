using TMPro;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Fenster zur Welt (GDD 7.7, Prototyp winTex): Einöde als Bild, davor Ebenen für Hunger (leerer Milchladen,
    /// Warteschlange) und Festumzug (Figuren, Fahnen, Banner, Konfetti). Pixel des Prototyp-Bildes (1024 × 512)
    /// werden auf die Fensterfläche umgerechnet. Texte aus der JSON (windowBanner, shopSign).
    /// </summary>
    public static class WindowViewBuilder
    {
        const float TexW = 1024f, TexH = 512f;

        public static void Build(Transform window, LayoutConfig L, float wallThickness)
        {
            var win = L.window;
            float viewW = win.width + 0.4f, viewH = win.height + 0.4f;
            float x = wallThickness + 0.35f;

            var view = window.Find("View");
            var viewMat = LookDevKit.Textured("LD_WindowView", "WindowView.png", Color.white, 0f, 0f, 0.9f);
            view.GetComponent<Renderer>().sharedMaterial = viewMat;

            var figMat = LookDevKit.Textured("LD_WindowFigure", "Figure.png", LabMaterials.Hex("#26262A"), 0f, 0f, 0f, true);
            var red = BlockoutKit.Emissive("LD_WindowRed", LabMaterials.Hex("#D01A1A"), 0.8f, false);
            var shopMat = BlockoutKit.Emissive("LD_WindowShop", LabMaterials.Hex("#3A3630"), 0.3f, false);
            var signMat = BlockoutKit.Emissive("LD_WindowSign", LabMaterials.Hex("#D9D2BF"), 0.8f, false);
            var poleMat = BlockoutKit.Emissive("LD_WindowPole", LabMaterials.Hex("#1E1E1E"), 0.2f, false);

            Vector3 P(float px, float py, float depth) => new Vector3(x - depth, (0.5f - py / TexH) * viewH, -(px / TexW - 0.5f) * viewW);
            float S(float px) => px / TexW * viewW;
            var figures = new System.Collections.Generic.List<Transform>();

            GameObject Quad(Transform parent, string name, float px0, float py0, float px1, float py1, Material m, float depth)
            {
                var c = P((px0 + px1) * 0.5f, (py0 + py1) * 0.5f, depth);
                var q = BlockoutKit.Quad(parent, name, c, new Vector2(S(px1 - px0), S(py1 - py0) * TexW / TexH * viewH / viewW), m, false, new Vector3(0f, 90f, 0f));
                Object.DestroyImmediate(q.GetComponent<Collider>());
                q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                return q;
            }

            Transform Figure(Transform parent, float px, float baseY, float s, float depth)
            {
                // Prototyp fig(): Kopf bei −30·s, Füße bis +9·s → Höhe ≈ 46·s Pixel
                float h = 46f * s, w = h * 0.5f;
                var q = Quad(parent, "Figure", px - w * 0.5f, baseY - 37f * s, px + w * 0.5f, baseY + 9f * s, figMat, depth);
                figures.Add(q.transform);
                return q.transform;
            }

            TMP_Text Label(Transform parent, string name, string key, float px, float py, float size, Color color, float depth)
            {
                var go = LookDevKit.Label(parent, name, key, P(px, py, depth + 0.01f), size, color, new Vector3(0f, 90f, 0f));
                return go.GetComponentInChildren<TMP_Text>();
            }

            // Hunger: leerer Milchladen und Schlange
            var hunger = BlockoutKit.Group(window, "Hunger", Vector3.zero);
            Quad(hunger, "Shop", 60f, 300f, 190f, 390f, shopMat, 0.02f);
            Quad(hunger, "ShopSign", 70f, 286f, 180f, 312f, signMat, 0.03f);
            Label(hunger, "ShopText", "shopSign", 125f, 299f, 1.1f, LabMaterials.Hex("#B3121B"), 0.04f);
            for (int i = 0; i < 26; i++)
                Figure(hunger, 205f + i * 30f + Mathf.Sin(i * 2.1f) * 5f, 392f - i % 3, 1f - i * 0.008f, 0.03f + i * 0.0005f);

            // Festumzug: Figuren, Fahnen, Banner, Konfetti
            var parade = BlockoutKit.Group(window, "Parade", Vector3.zero);
            for (int i = 0; i < 24; i++)
            {
                float px = 40f + i * 40f;
                Figure(parade, px, 392f, 1f, 0.03f);
                if (i % 3 != 0) continue;
                Quad(parade, "Pole", px + 7f, 318f, px + 10f, 388f, poleMat, 0.035f);
                Quad(parade, "Flag", px + 10f, 318f, px + 38f, 336f, red, 0.04f);
            }
            Quad(parade, "Banner", 330f, 300f, 690f, 334f, red, 0.05f);
            Label(parade, "BannerText", "windowBanner", 510f, 317f, 1.6f, LabMaterials.Hex("#FFD24A"), 0.06f);
            var confetti = LookDevChamberFx.Emitter(parade, "Confetti", P(512f, 40f, 0.05f), LookDevMaterials.Steam, Color.white,
                2.5f, 3.5f, 0.05f, 0.2f, 0.04f, 0.07f, 0.15f, ParticleSystemShapeType.Box, new Vector3(0.1f, viewW * 0.9f, 0.1f), 1f);
            var main = confetti.main;
            main.startColor = new ParticleSystem.MinMaxGradient(LabMaterials.Hex("#D01A1A"), LabMaterials.Hex("#FFD24A"));
            var em = confetti.emission;
            em.rateOverTime = 40f;

            window.gameObject.AddComponent<WindowView>().Setup(hunger.gameObject, parade.gameObject, figures.ToArray(), confetti);
        }
    }
}
