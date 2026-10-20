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
            // Rückwand mit Türen zu den Nebenzimmern; der Anbau dahinter kommt aus LabWingBuilder
            LabWingBuilder.BuildHallBackWall(shell, L);
            // Linke Wand
            Wall(shell, "Wall_Left", new Vector3(-x - t * 0.5f, h * 0.5f, zMid), new Vector3(t, h, depth), new Vector3(1f, 0f, 0f));
            // Rechte Wand mit Fensteröffnung
            BuildRightWallWithWindow(shell, L, x, t, h, zFront, zBack);

            BuildCeilingStructure(root, x, h, zFront, zBack);
            BuildDecor(root, zFront, zBack);
            LabWingBuilder.Build(root, L);
        }

        /// <summary>
        /// Stromausfall-Effekt (Phase 5): Teilstücke und herabhängende Enden des Hauptkabels (versteckt),
        /// Funken und BlackoutFx am Deckenträger.
        /// </summary>
        public static void BuildBlackoutFx(Transform root, LayoutConfig L, UnityEngine.Rendering.Volume darkVolume)
        {
            var c = root.Find("CeilingStructure");
            var main = c.Find("Cable_Main").GetComponent<Renderer>();
            float x = L.room.sideWallX;
            var g = BlockoutKit.Group(c, "CableTear", Vector3.zero);
            Transform Piece(string name, float halfLen, Vector3 euler)
            {
                var go = BlockoutKit.Cyl(g, name, main.transform.position, 0.045f, halfLen * 2f, M.Black, false, euler);
                go.SetActive(false);
                return go.transform;
            }
            var left = Piece("HalfLeft", 1f, new Vector3(0f, 0f, 90f));
            var right = Piece("HalfRight", 1f, new Vector3(0f, 0f, 90f));
            var dl = Piece("DangleLeft", 0.45f, Vector3.zero);
            var dr = Piece("DangleRight", 0.45f, Vector3.zero);

            var sparkGo = new GameObject("Sparks");
            sparkGo.transform.SetParent(g, false);
            var ps = sparkGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main2 = ps.main;
            main2.playOnAwake = false;
            main2.simulationSpace = ParticleSystemSimulationSpace.World;
            main2.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main2.startSpeed = new ParticleSystem.MinMaxCurve(2f, 6f);
            main2.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
            main2.startColor = new Color(1f, 0.85f, 0.4f);
            main2.gravityModifier = 1.5f;
            main2.maxParticles = 120;
            var em = ps.emission;
            em.rateOverTime = 0f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.08f;
            var r = sparkGo.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = LookDevMaterials.FireAdd;
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.06f;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            g.gameObject.AddComponent<BlackoutFx>().Setup(main, left, right, dl, dr, ps, darkVolume, -x, x);
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
            foreach (var bz in new[] { -3f, -9f, zBack + 2.5f })
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
            foreach (var lx in BackLampXs)
                HangingLamp(c, new Vector3(lx, 0f, BackLampZ(zBack)), h);
        }

        /// <summary>Hintere Reihe Hängelampen (vor der Rückwand mit den Türen).</summary>
        public static float BackLampZ(float zBack) => zBack + 2.6f;
        /// <summary>x der hinteren Lampenreihe (vor den Türen der Nebenzimmer).</summary>
        public static readonly float[] BackLampXs = { -12f, -6f, 0f, 6f, 12f };

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

            // Rückwand: Banner, Parole und Lampen kommen aus LookDevHall

            // Kisten stehen jetzt im Lager (LookDevRooms); in der Halle bleiben Fässer und eine Kiste in den hinteren Ecken
            LookDevRooms.Crate(d, new Vector3(-16.1f, 0f, zBack + 2.2f), 1.0f, 14f);
            foreach (var b in new[] { new Vector2(-16.2f, zBack + 0.65f), new Vector2(-15.2f, zBack + 0.6f), new Vector2(9.4f, zBack + 0.6f), new Vector2(10.3f, zBack + 0.8f) })
                LookDevRooms.Barrel(d, new Vector3(b.x, 0f, b.y), LookDevMaterials.RedPaint);
        }

    }
}
