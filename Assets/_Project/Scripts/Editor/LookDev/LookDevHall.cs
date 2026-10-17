using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Hallenhintergrund nach Bild 08 an der Wand hinter der Kammer: Laufstege mit gelbem Geländer,
    /// Rohrbündel mit Flanschen, hohe rote Banner mit Emblem, Wandschild, warme Wandlampen.
    /// Reine Deko ohne Kollider (der Bereich hinter dem Band ist nicht begehbar).
    /// </summary>
    public static class LookDevHall
    {
        public static void Build(Transform root, LayoutConfig layout)
        {
            var r = layout.room;
            float zWall = -r.frontWallZ;           // Innenseite der Wand hinter der Kammer (Unity-z)
            float xMax = r.sideWallX, h = r.height;
            var hall = BlockoutKit.Group(root, "LookDevHall", Vector3.zero);

            // Laufstege links und rechts der Kammer (Kammer und TV bleiben frei)
            Catwalk(hall, -xMax + 0.3f, -6.7f, 3.6f, zWall);
            Catwalk(hall, 10.7f, xMax - 0.3f, 3.6f, zWall);

            BuildPipes(hall, xMax, h, zWall);

            // Hohe Banner mit Emblem
            TallBanner(hall, new Vector3(-5.2f, 4.7f, zWall - 0.12f), "ui.signs.bannerGenetics");
            TallBanner(hall, new Vector3(12.3f, 4.7f, zWall - 0.12f), "ui.signs.bannerScienceTall");

            // Aufgemaltes Schild zwischen Kammer und Planungstafel
            var stable = BlockoutKit.Group(hall, "WallStable", new Vector3(3.7f, 2.9f, zWall - 0.03f));
            LookDevKit.Box(stable, "Plate", new Vector3(0f, 0f, 0.01f), new Vector3(1.55f, 1.1f, 0.03f), L.WallPanel);
            LookDevKit.Label(stable, "Text", "ui.signs.wallStable", new Vector3(0f, 0f, -0.01f), 1.7f, new Color(0.78f, 0.74f, 0.64f));
            LookDevKit.Bolts(stable, new Vector3(-0.7f, 0.48f, -0.006f), new Vector3(0.7f, 0.48f, -0.006f), 2, Vector3.back, L.Brass, 0.014f);
            LookDevKit.Bolts(stable, new Vector3(-0.7f, -0.48f, -0.006f), new Vector3(0.7f, -0.48f, -0.006f), 2, Vector3.back, L.Brass, 0.014f);

            // Warme Wandlampen über den Laufstegen und an der Wand
            BuildHaze(hall, xMax, zWall);

            foreach (var x in new[] { -14f, -10.5f, -7.4f, 12.2f, 15.2f })
                WallLamp(hall, new Vector3(x, 5.6f, zWall), true);
            foreach (var x in new[] { -12.2f, -8.6f, 13.6f })
                WallLamp(hall, new Vector3(x, 2.6f, zWall), false);
        }

        /// <summary>Warmer Hallendunst: wenige große, sehr transparente Partikel, die langsam treiben.</summary>
        static void BuildHaze(Transform parent, float xMax, float zWall)
        {
            var go = new GameObject("HazeWarm");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 4.2f, zWall - 2.2f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.prewarm = true;
            main.duration = 20f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(16f, 24f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startSize = new ParticleSystem.MinMaxCurve(4f, 7.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(1f, 0.8f, 0.55f, 0.5f);
            main.maxParticles = 40;
            var em = ps.emission;
            em.rateOverTime = 2f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2f * xMax - 2f, 6f, 3.5f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(1f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = L.HazeWarm;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        /// <summary>Laufsteg an der Wand: Gitterrost, Randträger, Stützen, gelbes Geländer.</summary>
        static void Catwalk(Transform parent, float x0, float x1, float y, float zWall)
        {
            const float depth = 1.3f;
            float len = x1 - x0, cx = (x0 + x1) * 0.5f, zc = zWall - depth * 0.5f, zEdge = zWall - depth;
            var c = BlockoutKit.Group(parent, "Catwalk", Vector3.zero);
            LookDevKit.Box(c, "Grate", new Vector3(cx, y, zc), new Vector3(len, 0.06f, depth), L.Grate);
            LookDevKit.Box(c, "EdgeBeam", new Vector3(cx, y - 0.12f, zEdge + 0.06f), new Vector3(len, 0.24f, 0.12f), L.SteelFrame);
            LookDevKit.Box(c, "WallBeam", new Vector3(cx, y - 0.12f, zWall - 0.06f), new Vector3(len, 0.24f, 0.12f), L.SteelFrame);

            // Stützen bis zum Boden mit Kopfstrebe
            int columns = Mathf.Max(2, Mathf.RoundToInt(len / 4.2f) + 1);
            for (int i = 0; i < columns; i++)
            {
                float x = Mathf.Lerp(x0 + 0.3f, x1 - 0.3f, i / (float)(columns - 1));
                LookDevKit.Box(c, "Column", new Vector3(x, (y - 0.24f) * 0.5f, zEdge + 0.1f), new Vector3(0.16f, y - 0.24f, 0.16f), L.SteelFrame);
                LookDevKit.Box(c, "Brace", new Vector3(x, y - 0.55f, zc + 0.05f), new Vector3(0.08f, 0.08f, 1.25f), L.SteelDark, new Vector3(-28f, 0f, 0f));
                LookDevKit.Bolts(c, new Vector3(x - 0.05f, y - 0.12f, zEdge - 0.002f), new Vector3(x + 0.05f, y - 0.12f, zEdge - 0.002f), 2, Vector3.back, L.Brass, 0.012f);
            }

            // Geländer: Pfosten, Hand- und Knieleiste
            int posts = Mathf.Max(2, Mathf.RoundToInt(len / 1.5f) + 1);
            for (int i = 0; i < posts; i++)
            {
                float x = Mathf.Lerp(x0 + 0.1f, x1 - 0.1f, i / (float)(posts - 1));
                LookDevKit.Cyl(c, "RailPost", new Vector3(x, y + 0.53f, zEdge + 0.08f), 0.028f, 1.06f, L.RailYellow, null, true, 10);
            }
            foreach (var ry in new[] { 1.05f, 0.55f })
                LookDevKit.Cyl(c, "Rail", new Vector3(cx, y + ry, zEdge + 0.08f), 0.032f, len, L.RailYellow, new Vector3(0f, 0f, 90f), true, 10);
            LookDevKit.Box(c, "ToeBoard", new Vector3(cx, y + 0.08f, zEdge + 0.08f), new Vector3(len, 0.12f, 0.02f), L.Hazard);
        }

        /// <summary>Rohrbündel unter der Decke und senkrechte Fallrohre an der Wand, mit Flanschen.</summary>
        static void BuildPipes(Transform parent, float xMax, float h, float zWall)
        {
            var p = BlockoutKit.Group(parent, "Pipes", Vector3.zero);
            var run = new[]
            {
                (y: h - 1.05f, z: zWall - 0.35f, r: 0.22f, m: L.SteelDark),
                (y: h - 1.55f, z: zWall - 0.3f, r: 0.15f, m: L.GreenPaint),
                (y: h - 1.9f, z: zWall - 0.25f, r: 0.1f, m: L.SteelPanel),
                (y: h - 0.75f, z: zWall - 1.2f, r: 0.18f, m: L.SteelFrame),
            };
            foreach (var s in run)
            {
                LookDevKit.Cyl(p, "Pipe", new Vector3(0f, s.y, s.z), s.r, 2f * xMax, s.m, new Vector3(0f, 0f, 90f), true, 16);
                for (float x = -xMax + 1.5f; x < xMax; x += 3f)
                    LookDevKit.Cyl(p, "Flange", new Vector3(x, s.y, s.z), s.r + 0.05f, 0.1f, L.SteelDark, new Vector3(0f, 0f, 90f), true, 16);
                for (float x = -xMax + 3f; x < xMax; x += 4.5f)
                    LookDevKit.Box(p, "Clamp", new Vector3(x, s.y + s.r + 0.08f, (s.z + zWall) * 0.5f), new Vector3(0.08f, 0.08f, zWall - s.z + 0.05f), L.SteelDark);
            }

            // Fallrohre (seitlich von Kammer und Tafeln)
            foreach (var x in new[] { -3.2f, 2.9f, 10.45f, -12.4f })
            {
                float top = h - 1.05f, bottom = 0.4f;
                LookDevKit.Cyl(p, "DownPipe", new Vector3(x, (top + bottom) * 0.5f, zWall - 0.25f), 0.13f, top - bottom, L.SteelPanel, null, true, 16);
                for (float y = bottom + 0.6f; y < top; y += 2.2f)
                    LookDevKit.Cyl(p, "Flange", new Vector3(x, y, zWall - 0.25f), 0.18f, 0.08f, L.SteelDark, null, true, 16);
                LookDevKit.Cyl(p, "Valve", new Vector3(x, 1.4f, zWall - 0.45f), 0.16f, 0.04f, L.RedPaint, new Vector3(90f, 0f, 0f), true, 16);
            }
        }

        /// <summary>Hohes Stoffbanner mit Emblem, Leiste oben und unten.</summary>
        static void TallBanner(Transform parent, Vector3 pos, string key)
        {
            const float w = 2.1f, hgt = 5.4f;
            var g = BlockoutKit.Group(parent, "TallBanner", pos);
            LookDevKit.Box(g, "Cloth", Vector3.zero, new Vector3(w, hgt, 0.02f), L.BannerRed);
            LookDevKit.Cyl(g, "TopBar", new Vector3(0f, hgt * 0.5f + 0.05f, -0.02f), 0.05f, w + 0.3f, L.Brass, new Vector3(0f, 0f, 90f), true, 12);
            LookDevKit.Cyl(g, "BottomBar", new Vector3(0f, -hgt * 0.5f - 0.03f, -0.02f), 0.04f, w + 0.1f, L.SteelDark, new Vector3(0f, 0f, 90f), true, 12);
            LookDevKit.Plane(g, "Emblem", new Vector3(0f, hgt * 0.5f - 0.95f, -0.015f), new Vector2(1.35f, 1.35f), L.EmblemCream);
            LookDevKit.Label(g, "Text", key, new Vector3(0f, -0.55f, -0.015f), 4.6f, LabMaterials.CreamText);
        }

        /// <summary>Käfiglampe an der Wand mit warm leuchtendem Kolben und gebackenem Licht.</summary>
        static void WallLamp(Transform parent, Vector3 pos, bool withLight)
        {
            var g = BlockoutKit.Group(parent, "WallLamp", pos);
            LookDevKit.Box(g, "Mount", new Vector3(0f, 0f, -0.04f), new Vector3(0.22f, 0.3f, 0.08f), L.SteelDark);
            LookDevKit.Cyl(g, "Arm", new Vector3(0f, 0.05f, -0.22f), 0.025f, 0.36f, L.SteelDark, new Vector3(90f, 0f, 0f), true, 8);
            BlockoutKit.MeshObj(g, "Shade", MeshFactory.Cone(0.22f, 0.18f), new Vector3(0f, 0.06f, -0.42f), L.SteelDark);
            var bulb = BlockoutKit.Sphere(g, "Bulb", new Vector3(0f, 0.0f, -0.42f), 0.08f, L.LampBulb);
            bulb.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            if (!withLight) return;

            var lightGo = new GameObject("WallLamp_Baked");
            lightGo.transform.SetParent(g, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.15f, -0.6f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.lightmapBakeType = LightmapBakeType.Baked;
            l.color = LabMaterials.Hex("#FFC27A");
            l.intensity = 4f;
            l.range = 5.5f;
            l.shadows = LightShadows.Soft;
            lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
        }
    }
}
