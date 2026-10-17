using UnityEngine;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Baut die Halle: Boden, Wände mit Fensteröffnung, Decke mit Trägern, Rohre, Lampen, Banner und Deko.
    /// Raummaße aus GameConfig.layout, Deko-Positionen wie im Prototyp (in Unity-Koordinaten).
    /// </summary>
    public static class LabEnvironmentBuilder
    {
        public static void Build(Transform root, LayoutConfig L)
        {
            var r = L.room;
            float x = r.sideWallX, t = r.wallThickness, h = r.height;
            // Unity-z: Frontwand (hinter der Kammer) positiv, Rückwand (hinter dem Spieler) negativ
            float zFront = -r.frontWallZ, zBack = -r.backWallZ;
            float depth = zFront - zBack, zMid = (zFront + zBack) * 0.5f;

            var shell = BlockoutKit.Group(root, "Shell", Vector3.zero);
            var floorSize = new Vector3(2f * x + t, 0.2f, depth + t);
            Retexture(BlockoutKit.Box(shell, "Floor", new Vector3(0f, -0.1f, zMid), floorSize, M.Concrete), floorSize, LookDevMaterials.FloorConcrete);
            var ceil = BlockoutKit.Box(shell, "Ceiling", new Vector3(0f, h + 0.1f, zMid), new Vector3(2f * x + t, 0.2f, depth + t), M.Ceiling);
            BlockoutKit.CastShadows(ceil, false);

            // Frontwand und Rückwand
            Wall(shell, "Wall_Front", new Vector3(0f, h * 0.5f, zFront + t * 0.5f), new Vector3(2f * x, h, t), new Vector3(0f, 0f, -1f));
            Wall(shell, "Wall_Back", new Vector3(0f, h * 0.5f, zBack - t * 0.5f), new Vector3(2f * x, h, t), new Vector3(0f, 0f, 1f));
            // Linke Wand
            Wall(shell, "Wall_Left", new Vector3(-x - t * 0.5f, h * 0.5f, zMid), new Vector3(t, h, depth), new Vector3(1f, 0f, 0f));
            // Rechte Wand mit Fensteröffnung
            BuildRightWallWithWindow(shell, L, x, t, h, zFront, zBack);

            // Tür in der Rückwand
            BlockoutKit.Box(shell, "Door", new Vector3(0f, 1.8f, zBack + 0.1f), new Vector3(2.4f, 3.6f, 0.2f), M.SteelDark);

            BuildCeilingStructure(root, x, h, zFront, zBack);
            BuildDecor(root, zFront, zBack);
        }

        /// <summary>Wand mit dunklerem Sockel (Fliesen) auf der Raumseite.</summary>
        static void Wall(Transform parent, string name, Vector3 center, Vector3 size, Vector3 inward)
        {
            var w = Retexture(BlockoutKit.Box(parent, name, center, size, M.WallUpper), size, LookDevMaterials.WallConcrete);
            w.isStatic = true;
            BlockoutKit.CastShadows(w, true);
            const float wainscot = 2.2f, inset = 0.03f;
            var s = size;
            var lowerSize = new Vector3(Mathf.Abs(inward.x) > 0 ? inset : s.x, wainscot, Mathf.Abs(inward.z) > 0 ? inset : s.z);
            var halfThick = Vector3.Scale(size * 0.5f, new Vector3(Mathf.Abs(inward.x), 0f, Mathf.Abs(inward.z)));
            var lowerCenter = new Vector3(center.x, wainscot * 0.5f, center.z) + Vector3.Scale(inward, halfThick) + inward * inset * 0.5f;
            Retexture(BlockoutKit.Box(parent, name + "_Sockel", lowerCenter, lowerSize, M.WallLower), lowerSize, LookDevMaterials.WallPanel);
        }

        /// <summary>Ersetzt den Einheitswürfel durch ein Mesh mit Welt-UVs (Texturen kacheln in Metern), Kollider bleibt gleich groß.</summary>
        static GameObject Retexture(GameObject go, Vector3 size, Material mat)
        {
            go.transform.localScale = Vector3.one;
            go.GetComponent<MeshFilter>().sharedMesh = LookDevKit.Box(size);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            var col = go.GetComponent<BoxCollider>();
            if (col != null) col.size = size;
            return go;
        }

        static void BuildRightWallWithWindow(Transform parent, LayoutConfig L, float x, float t, float h, float zFront, float zBack)
        {
            var win = L.window;
            float wz = -win.z, halfW = win.width * 0.5f, halfH = win.height * 0.5f;
            float z0 = wz - halfW, z1 = wz + halfW, y0 = win.y - halfH, y1 = win.y + halfH;
            float cx = x + t * 0.5f;
            // Wandstücke um die Öffnung
            Wall(parent, "Wall_Right_Back", new Vector3(cx, h * 0.5f, (zBack + z0) * 0.5f), new Vector3(t, h, z0 - zBack), Vector3.left);
            Wall(parent, "Wall_Right_Front", new Vector3(cx, h * 0.5f, (z1 + zFront) * 0.5f), new Vector3(t, h, zFront - z1), Vector3.left);
            Retexture(BlockoutKit.Box(parent, "Wall_Right_Below", new Vector3(cx, y0 * 0.5f, wz), new Vector3(t, y0, win.width), M.WallUpper), new Vector3(t, y0, win.width), LookDevMaterials.WallConcrete);
            Retexture(BlockoutKit.Box(parent, "Wall_Right_Above", new Vector3(cx, (y1 + h) * 0.5f, wz), new Vector3(t, h - y1, win.width), M.WallUpper), new Vector3(t, h - y1, win.width), LookDevMaterials.WallConcrete);

            // Fenster: Himmel/Einöde als leuchtende Fläche hinter der Öffnung, davor Rahmen und Sprossen
            var w = BlockoutKit.Group(parent, "Window", new Vector3(x, win.y, wz));
            BlockoutKit.Quad(w, "View", new Vector3(t + 0.4f, 0f, 0f), new Vector2(win.width + 0.4f, win.height + 0.4f), M.WindowSky, true, new Vector3(0f, 90f, 0f));
            BlockoutKit.Box(w, "Frame_Top", new Vector3(0f, halfH, 0f), new Vector3(0.15f, 0.18f, win.width + 0.3f), M.SteelDark);
            BlockoutKit.Box(w, "Frame_Bottom", new Vector3(0f, -halfH, 0f), new Vector3(0.15f, 0.18f, win.width + 0.3f), M.SteelDark);
            BlockoutKit.Box(w, "Frame_Mid", new Vector3(0f, 0f, 0f), new Vector3(0.15f, 0.12f, win.width), M.SteelDark);
            foreach (var dz in new[] { -4.6f, -1.5f, 1.5f, 4.6f })
                BlockoutKit.Box(w, "Mullion", new Vector3(0f, 0f, dz), new Vector3(0.15f, win.height + 0.3f, 0.16f), M.SteelDark);
        }

        static void BuildCeilingStructure(Transform root, float x, float h, float zFront, float zBack)
        {
            var c = BlockoutKit.Group(root, "CeilingStructure", Vector3.zero);
            float depth = zFront - zBack, zMid = (zFront + zBack) * 0.5f;
            for (float bx = -16f; bx <= 16f; bx += 4f)
            {
                var beam = BlockoutKit.Box(c, "Beam", new Vector3(bx, h - 0.3f, zMid), new Vector3(0.3f, 0.45f, depth), M.SteelDark);
                BlockoutKit.CastShadows(beam, false);
            }
            foreach (var bz in new[] { -3f, -9f })
            {
                var beam = BlockoutKit.Box(c, "CrossBeam", new Vector3(0f, h - 0.25f, bz), new Vector3(2f * x, 0.4f, 0.3f), M.SteelDark);
                BlockoutKit.CastShadows(beam, false);
            }

            // Rohr und Kabel an der Frontwand (Prototyp z -2.5 / -1.0)
            BlockoutKit.Cyl(c, "Pipe_Front", new Vector3(0f, 7.7f, 2.5f), 0.16f, 2f * x, M.Steel, true, new Vector3(0f, 0f, 90f));
            BlockoutKit.Cyl(c, "Cable_Main", new Vector3(0f, 6.9f, 1.0f), 0.045f, 2f * x, M.Black, true, new Vector3(0f, 0f, 90f));

            // Lampenleisten an der Frontwand und Hängelampen
            foreach (var lx in new[] { -9f, -3f, 3f, 9f })
            {
                BlockoutKit.Box(c, "LampStrip", new Vector3(lx, 7.2f, 1.4f), new Vector3(1.6f, 0.18f, 0.5f), M.LampWarm);
                HangingLamp(c, new Vector3(lx, 0f, -1.4f), h);
            }
            foreach (var lx in new[] { -6f, 6f })
                HangingLamp(c, new Vector3(lx, 0f, -8.5f), h);
        }

        static void HangingLamp(Transform parent, Vector3 pos, float h)
        {
            var g = BlockoutKit.Group(parent, "HangingLamp", pos);
            BlockoutKit.Cyl(g, "Rod", new Vector3(0f, h - 0.5f, 0f), 0.02f, 0.9f, M.Steel);
            BlockoutKit.MeshObj(g, "Shade", MeshFactory.Cone(0.55f, 0.45f), new Vector3(0f, 7.3f, 0f), M.SteelDark);
            BlockoutKit.Sphere(g, "Bulb", new Vector3(0f, 7.32f, 0f), 0.12f, M.LampWarm);
        }

        static void BuildDecor(Transform root, float zFront, float zBack)
        {
            var d = BlockoutKit.Group(root, "Decor", Vector3.zero);

            // Frontwand (hinter der Kammer): Banner, Laufstege und Rohre kommen aus LookDevHall

            // Rückwand (zeigt zur Kammer)
            BlockoutKit.Sign(d, "Slogan_Back", "ui.signs.wallSlogan", new Vector3(0f, 5.4f, zBack + 0.05f), 5f, M.Gold, M.PropRedBright, false, 180f);
            for (int i = 0; i < 4; i++)
                Banner(d, new Vector3(new[] { -9f, -4.5f, 4.5f, 9f }[i], 4.0f, zBack + 0.06f), i % 2 == 0 ? "ui.signs.bannerPlan" : "ui.signs.bannerScience", 180f);

            // Kisten und Fässer (Prototyp-Positionen, z gespiegelt)
            foreach (var c in new[] { new Vector3(-12f, 1.3f, -10.6f), new Vector3(-10.4f, 1.0f, -10.9f), new Vector3(-11.4f, 0.8f, -10.6f),
                         new Vector3(12f, 1.4f, -10.4f), new Vector3(10.2f, 1.0f, -10.8f) })
                BlockoutKit.Box(d, "Crate", new Vector3(c.x, c.y * 0.5f, c.z), Vector3.one * c.y, M.Crate, true, new Vector3(0f, c.x * 0.3f * Mathf.Rad2Deg, 0f))
                    .AddComponent<BoxCollider>();
            foreach (var b in new[] { new Vector2(-14.5f, -9.8f), new Vector2(14.6f, -9.6f), new Vector2(14.2f, -8.4f) })
                BlockoutKit.Cyl(d, "Barrel", new Vector3(b.x, 0.55f, b.y), 0.45f, 1.1f, M.Barrel).AddComponent<CapsuleCollider>();
        }

        static void Banner(Transform parent, Vector3 pos, string key, float yaw)
        {
            var g = BlockoutKit.Group(parent, "Banner", pos, yaw);
            BlockoutKit.Quad(g, "Cloth", Vector3.zero, new Vector2(1.4f, 2.6f), M.PropRedBright);
            BlockoutKit.Sign(g, "Text", key, new Vector3(0f, -0.2f, -0.01f), 2.2f, M.Gold, null, false);
        }
    }
}
