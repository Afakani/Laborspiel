using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Deko-Dichte nach Bild 07 und Prototyp (Dekoration): Wandplakate und Warnschilder mit Texten aus der JSON (posters),
    /// Glasregale, Spinde, Büroecke unter dem Fenster (Schreibtisch mit Lampe, Röhrenmonitor, Schreibmaschine, Bücher,
    /// Tasse, Reagenzgläser, Stuhl), roter Teppich mit Emblem, Aktenschränke, Feuerlöscher, Pflanze, Wanduhr, Wandrohre.
    /// Alles eigene Meshes; Lage in Prototyp-Koordinaten (z gespiegelt), an die Raummaße angepasst.
    /// </summary>
    public static class LookDevDecor
    {
        const string PosterKey = "posters";

        public static void Build(Transform root, LayoutConfig layout)
        {
            var r = layout.room;
            float xWall = r.sideWallX, zBack = -r.backWallZ, zFront = -r.frontWallZ;
            var d = BlockoutKit.Group(root, "LookDevDecor", Vector3.zero);

            // Plakate: linke Wand (Blick nach +x) und Rückwand (Blick nach +z)
            Poster(d, 0, new Vector3(-xWall + 0.02f, 4.5f, -2.6f), -90f, "#D01A1A", "#F4EFE3");
            Poster(d, 1, new Vector3(-xWall + 0.02f, 4.5f, -5.6f), -90f, "#2A2622", "#FFD24A");
            Poster(d, 2, new Vector3(-xWall + 0.02f, 4.5f, -8.4f), -90f, "#B3121B", "#F4EFE3");
            Poster(d, 3, new Vector3(-5.9f, 4.5f, zBack + 0.02f), 180f, "#1D3A8A", "#F4EFE3");
            Poster(d, 4, new Vector3(5.9f, 4.5f, zBack + 0.02f), 180f, "#E9E1CC", "#2A2622");

            // Warnschilder neben der Mischstation und hinter der Zentrifuge (Prototyp warnSign; neben statt hinter den Tanks, sonst verdeckt)
            WarnSign(d, new Vector3(-3.55f, 2.2f, -3.4f), new Vector2(1.0f, 0.5f));
            WarnSign(d, new Vector3(6.6f, 3.3f, -1.4f), new Vector2(1.0f, 0.5f));

            // Regale mit Gläsern an der linken Wand
            foreach (var z in new[] { -3.4f, -7.0f })
                JarShelf(d, new Vector3(-xWall + 0.4f, 0f, z), z < -5f ? 7 : 3);

            // Spinde hinten links
            for (int i = 0; i < 4; i++)
                Locker(d, new Vector3(-15.5f + i * 0.78f, 0f, zBack + 0.32f), i);
            Solid(d, "LockerBlock", new Vector3(-14.33f, 1.05f, zBack + 0.32f), new Vector3(3.1f, 2.1f, 0.6f));

            // Büroecke unter dem Fenster mit Teppich und Aktenschränken
            Desk(d, new Vector3(xWall - 0.52f, 0f, -9.6f));
            Carpet(d, new Vector3(13.6f, 0f, -9.3f), new Vector2(2.4f, 3.4f));
            foreach (var z in new[] { -6.9f, -7.72f })
                FileCabinet(d, new Vector3(xWall - 0.37f, 0f, z));

            Extinguisher(d, new Vector3(-xWall + 0.15f, 0f, -0.9f));
            Plant(d, new Vector3(xWall - 0.7f, 0f, zBack + 0.6f));
            Clock(d, new Vector3(0f, 6.6f, zBack + 0.03f));

            // Rohre an den Seitenwänden (Messing und Stahl)
            float len = zFront - zBack, zMid = (zFront + zBack) * 0.5f;
            foreach (float side in new[] { -1f, 1f })
            {
                LookDevKit.Cyl(d, "SidePipe", new Vector3(side * (xWall - 0.15f), 6.75f, zMid), 0.09f, len, L.Brass, new Vector3(90f, 0f, 0f), true, 12);
                LookDevKit.Cyl(d, "SidePipe", new Vector3(side * (xWall - 0.15f), 7.15f, zMid), 0.09f, len, L.SteelPanel, new Vector3(90f, 0f, 0f), true, 12);
                for (float z = zBack + 1.5f; z < zFront; z += 3f)
                    LookDevKit.Box(d, "PipeClamp", new Vector3(side * (xWall - 0.08f), 6.95f, z), new Vector3(0.16f, 0.62f, 0.06f), L.SteelDark);
            }
        }

        // ---------- Plakate und Schilder ----------

        /// <summary>Papierplakat mit Rahmenlinie, Emblem, Schlagzeile und Unterzeile (posters[i]), an Messingnägeln.</summary>
        static void Poster(Transform parent, int index, Vector3 pos, float yaw, string bgHex, string fgHex)
        {
            var bg = LabMaterials.Hex(bgHex);
            var fg = LabMaterials.Hex(fgHex);
            var g = BlockoutKit.Group(parent, "Poster", pos, yaw);
            var paper = LookDevKit.Textured("LD_Poster_" + bgHex.TrimStart('#'), "Paper.png", Color.Lerp(bg, bg * 1.25f, 0.5f), 0f, 0.15f);
            var line = BlockoutKit.Mat("LD_PosterLine_" + fgHex.TrimStart('#'), fg, 0f, 0.15f);
            const float w = 1.3f, h = 1.8f;
            LookDevKit.Box(g, "Paper", Vector3.zero, new Vector3(w, h, 0.01f), paper);
            Frame(g, w - 0.12f, h - 0.12f, 0.025f, -0.007f, line);
            var emblem = LookDevKit.Textured("LD_PosterEmblem_" + fgHex.TrimStart('#'), "EmblemCream.png", fg * 1.1f, 0f, 0.2f, 0f, true);
            LookDevKit.Plane(g, "Emblem", new Vector3(0f, 0.5f, -0.008f), new Vector2(0.62f, 0.62f), emblem);
            var head = Text(g, "Head", new Vector3(0f, -0.2f, -0.009f), new Vector2(w - 0.24f, 0.8f), 2.4f, fg);
            var sub = Text(g, "Sub", new Vector3(0f, -0.73f, -0.009f), new Vector2(w - 0.24f, 0.16f), 0.75f, fg);
            g.gameObject.AddComponent<PosterText>().Setup($"{PosterKey}[{index}]", head, sub, BlockoutKit.Texts.Get($"{PosterKey}[{index}]"), false);
            foreach (var c in new[] { new Vector3(-w * 0.5f + 0.05f, h * 0.5f - 0.05f, -0.01f), new Vector3(w * 0.5f - 0.05f, h * 0.5f - 0.05f, -0.01f) })
                LookDevKit.Cyl(g, "Nail", c, 0.012f, 0.01f, L.Brass, new Vector3(90f, 0f, 0f), true, 8);
        }

        /// <summary>Gelb-schwarzes Warnschild (letzter Eintrag in posters) auf einem Ständer.</summary>
        static void WarnSign(Transform parent, Vector3 pos, Vector2 size)
        {
            var g = BlockoutKit.Group(parent, "WarnSign", pos);
            var yellow = BlockoutKit.Mat("LD_WarnYellow", LabMaterials.Hex("#F2B705"), 0.1f, 0.35f);
            var black = BlockoutKit.Mat("LD_WarnBlack", LabMaterials.Hex("#111111"), 0f, 0.3f);
            LookDevKit.Box(g, "Plate", Vector3.zero, new Vector3(size.x, size.y, 0.02f), yellow);
            Frame(g, size.x - 0.06f, size.y - 0.06f, 0.025f, -0.012f, black);
            var head = Text(g, "Head", new Vector3(0f, size.y * 0.2f, -0.013f), new Vector2(size.x - 0.12f, size.y * 0.35f), 1.6f, LabMaterials.Hex("#111111"));
            var sub = Text(g, "Sub", new Vector3(0f, -size.y * 0.2f, -0.013f), new Vector2(size.x - 0.12f, size.y * 0.38f), 0.9f, LabMaterials.Hex("#111111"));
            int last = BlockoutKit.Texts.Count(PosterKey) - 1;
            g.gameObject.AddComponent<PosterText>().Setup($"{PosterKey}[{last}]", head, sub, BlockoutKit.Texts.Get($"{PosterKey}[{last}]"), true);
            LookDevKit.Cyl(parent, "SignPost", new Vector3(pos.x, (pos.y - size.y * 0.5f) * 0.5f, pos.z + 0.04f), 0.03f, pos.y - size.y * 0.5f, L.SteelDark, null, true, 10);
            LookDevKit.Bolts(g, new Vector3(-size.x * 0.5f + 0.04f, 0f, -0.012f), new Vector3(size.x * 0.5f - 0.04f, 0f, -0.012f), 2, Vector3.back, L.SteelDark, 0.012f);
        }

        /// <summary>Rechteckige Linie aus vier Leisten (Plakatrand).</summary>
        static void Frame(Transform g, float w, float h, float t, float z, Material m)
        {
            LookDevKit.Box(g, "Line", new Vector3(0f, h * 0.5f, z), new Vector3(w, t, 0.002f), m);
            LookDevKit.Box(g, "Line", new Vector3(0f, -h * 0.5f, z), new Vector3(w, t, 0.002f), m);
            LookDevKit.Box(g, "Line", new Vector3(-w * 0.5f, 0f, z), new Vector3(t, h, 0.002f), m);
            LookDevKit.Box(g, "Line", new Vector3(w * 0.5f, 0f, z), new Vector3(t, h, 0.002f), m);
        }

        /// <summary>3D-Text mit fester Fläche und automatischer Größe (lesbar von -Z).</summary>
        static TextMeshPro Text(Transform parent, string name, Vector3 pos, Vector2 size, float maxSize, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var t = go.AddComponent<TextMeshPro>();
            t.font = BlockoutKit.SignFont;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableAutoSizing = true;
            t.fontSizeMin = 0.1f;
            t.fontSizeMax = maxSize;
            t.lineSpacing = -18f;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.rectTransform.sizeDelta = size;
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return t;
        }

        // ---------- Möbel ----------

        /// <summary>Holzregal mit vier Böden und 18 leuchtenden Gläsern (Farben wie im Prototyp jarCols).</summary>
        static void JarShelf(Transform parent, Vector3 pos, int seed)
        {
            var g = BlockoutKit.Group(parent, "JarShelf", pos, -90f);
            foreach (var y in new[] { 0.25f, 1.05f, 1.85f, 2.65f })
                LookDevKit.Box(g, "Board", new Vector3(0f, y, 0f), new Vector3(2.6f, 0.06f, 0.8f), L.Wood);
            foreach (var x in new[] { -1.27f, 1.27f })
                LookDevKit.Box(g, "Side", new Vector3(x, 1.4f, 0f), new Vector3(0.07f, 2.8f, 0.8f), L.Wood);
            LookDevKit.Box(g, "Back", new Vector3(0f, 1.4f, 0.39f), new Vector3(2.6f, 2.8f, 0.02f), L.SteelDark);
            var glass = LookDevKit.Glass("LD_JarGlass", new Color(0.9f, 0.97f, 1f, 0.22f), 0.95f);
            var cols = new[] { "#46D14F", "#3AA0FF", "#FFCC2A", "#FF6BD5", "#9FE8FF", "#D8C8A0" };
            var jar = LookDevKit.Lathe("Jar", new[] { new Vector2(0f, 0f), new Vector2(0.1f, 0f), new Vector2(0.11f, 0.02f), new Vector2(0.11f, 0.27f), new Vector2(0.08f, 0.31f), new Vector2(0.08f, 0.33f), new Vector2(0f, 0.33f) }, 14);
            for (int j = 0; j < 18; j++)
            {
                var hex = cols[(j * 7 + seed) % cols.Length];
                var liquid = BlockoutKit.Emissive("LD_Jar_" + hex.TrimStart('#'), LabMaterials.Hex(hex), 0.6f, false);
                var p = new Vector3(-1.0f + (j % 6) * 0.4f, 0.28f + (j / 6) * 0.8f, j % 2 == 1 ? -0.15f : 0.15f);
                LookDevKit.Obj(g, "Jar", jar, p, glass, null, true, false);
                LookDevKit.Cyl(g, "Liquid", p + new Vector3(0f, 0.12f, 0f), 0.09f, 0.2f + (j % 3) * 0.02f, liquid, null, true, 12);
                LookDevKit.Cyl(g, "Lid", p + new Vector3(0f, 0.345f, 0f), 0.085f, 0.03f, L.Brass, null, true, 12);
            }
            Solid(g, "Collider", new Vector3(0f, 1.4f, 0f), new Vector3(2.6f, 2.8f, 0.8f));
        }

        /// <summary>Grüner Stahlspind mit Lüftungsschlitzen, Griff und Nummernschild.</summary>
        static void Locker(Transform parent, Vector3 pos, int i)
        {
            var g = BlockoutKit.Group(parent, "Locker", pos, 180f);
            LookDevKit.Box(g, "Body", new Vector3(0f, 1.05f, 0f), new Vector3(0.74f, 2.1f, 0.6f), L.GreenPaint);
            LookDevKit.Box(g, "Door", new Vector3(0f, 1.05f, -0.305f), new Vector3(0.66f, 2.0f, 0.01f), L.GreenPaint);
            for (int s = 0; s < 4; s++)
                LookDevKit.Box(g, "Vent", new Vector3(0f, 1.85f - s * 0.06f, -0.312f), new Vector3(0.4f, 0.02f, 0.004f), L.BlackPaint);
            LookDevKit.Box(g, "Handle", new Vector3(0.24f, 1.1f, -0.33f), new Vector3(0.03f, 0.16f, 0.03f), L.Chrome);
            LookDevKit.Box(g, "NumberPlate", new Vector3(0f, 1.55f, -0.312f), new Vector3(0.14f, 0.08f, 0.004f), L.CreamPlate);
            LookDevKit.Box(g, "Kick", new Vector3(0f, 0.05f, -0.29f), new Vector3(0.72f, 0.1f, 0.02f), L.SteelDark, new Vector3(0f, 0f, i % 2 == 0 ? 0f : 0.6f));
        }

        /// <summary>Schreibtisch mit Lampe, Röhrenmonitor, Schreibmaschine, Bücherstapel (Titel aus der JSON), Tasse, Papieren, Reagenzgläsern und Stuhl.</summary>
        static void Desk(Transform parent, Vector3 pos)
        {
            // Vorderseite (−Z lokal) zeigt in den Raum (−x Welt); lokal x = Längsrichtung
            var g = BlockoutKit.Group(parent, "Desk", pos, 90f);
            const float top = 0.82f;
            LookDevKit.Box(g, "Top", new Vector3(0f, top - 0.03f, 0f), new Vector3(2.2f, 0.06f, 1.0f), L.Wood);
            foreach (var x in new[] { -1.03f, 1.03f })
                foreach (var z in new[] { -0.42f, 0.42f })
                    LookDevKit.Box(g, "Leg", new Vector3(x, (top - 0.06f) * 0.5f, z), new Vector3(0.06f, top - 0.06f, 0.06f), L.SteelDark);
            LookDevKit.Box(g, "Drawers", new Vector3(0.72f, 0.4f, 0f), new Vector3(0.55f, 0.72f, 0.9f), L.SteelPanel);
            for (int k = 0; k < 3; k++)
                LookDevKit.Box(g, "DrawerHandle", new Vector3(0.72f, 0.64f - k * 0.22f, -0.46f), new Vector3(0.16f, 0.02f, 0.02f), L.Chrome);

            // Röhrenmonitor
            var crt = BlockoutKit.Group(g, "CRT", new Vector3(-0.72f, top, 0.12f));
            var beige = BlockoutKit.Mat("LD_CrtBeige", LabMaterials.Hex("#BDB294"), 0f, 0.35f);
            LookDevKit.Box(crt, "Body", new Vector3(0f, 0.2f, 0.05f), new Vector3(0.42f, 0.38f, 0.42f), beige);
            LookDevKit.Box(crt, "Bezel", new Vector3(0f, 0.21f, -0.165f), new Vector3(0.38f, 0.32f, 0.02f), L.BlackPaint);
            var screen = BlockoutKit.Emissive("LD_CrtScreen", LabMaterials.Hex("#2F8A3E"), 1.3f, false);
            LookDevKit.Plane(crt, "Screen", new Vector3(0f, 0.21f, -0.177f), new Vector2(0.3f, 0.23f), screen);
            LookDevKit.Box(crt, "Stand", new Vector3(0f, 0.01f, 0.05f), new Vector3(0.26f, 0.02f, 0.26f), beige);

            // Schreibmaschine mit eingespanntem Blatt
            var tw = BlockoutKit.Group(g, "Typewriter", new Vector3(-0.08f, top, -0.05f));
            LookDevKit.Box(tw, "Base", new Vector3(0f, 0.06f, 0f), new Vector3(0.45f, 0.12f, 0.36f), L.BlackPaint, new Vector3(-6f, 0f, 0f));
            LookDevKit.Cyl(tw, "Roller", new Vector3(0f, 0.15f, 0.12f), 0.035f, 0.5f, L.Rubber, new Vector3(0f, 0f, 90f), true, 12);
            LookDevKit.Box(tw, "Sheet", new Vector3(0f, 0.25f, 0.15f), new Vector3(0.3f, 0.21f, 0.004f), L.Paper, new Vector3(-12f, 0f, 0f));
            for (int k = 0; k < 3; k++)
                LookDevKit.Box(tw, "Keys", new Vector3(0f, 0.125f - k * 0.012f, -0.06f - k * 0.05f), new Vector3(0.36f, 0.015f, 0.035f), L.Chrome, new Vector3(-6f, 0f, 0f));

            // Bücherstapel (Rücken zum Raum, Titel aus ui.signs.books)
            var books = BlockoutKit.Group(g, "Books", new Vector3(0.5f, top, 0.18f));
            var bookCols = new[] { "#7A1A1C", "#1D3A8A", "#3A5A2A", "#5B3A22" };
            int nBooks = BlockoutKit.Texts.Count("ui.signs.books");
            for (int b = 0; b < nBooks; b++)
            {
                var m = BlockoutKit.Mat("LD_Book_" + bookCols[b % bookCols.Length].TrimStart('#'), LabMaterials.Hex(bookCols[b % bookCols.Length]), 0f, 0.25f);
                var bk = BlockoutKit.Group(books, "Book", new Vector3(Mathf.Sin(b * 2.3f) * 0.02f, 0.035f + b * 0.07f, 0f), Mathf.Sin(b * 1.7f) * 6f);
                LookDevKit.Box(bk, "Cover", Vector3.zero, new Vector3(0.36f, 0.065f, 0.26f), m);
                LookDevKit.Box(bk, "Pages", new Vector3(0f, 0f, 0.008f), new Vector3(0.34f, 0.052f, 0.25f), L.CreamPlate);
                LookDevKit.Label(bk, "Title", $"ui.signs.books[{b}]", new Vector3(0f, 0f, -0.132f), 0.22f, LabMaterials.Hex("#E3C77A"));
            }

            // Schreibtischlampe (gebackenes warmes Licht)
            var lamp = BlockoutKit.Group(g, "DeskLamp", new Vector3(0.9f, top, 0.3f));
            var lampGreen = BlockoutKit.Mat("LD_LampGreen", LabMaterials.Hex("#2E5A3A"), 0.3f, 0.5f);
            LookDevKit.Cyl(lamp, "Base", new Vector3(0f, 0.02f, 0f), 0.1f, 0.04f, L.SteelDark, null, true, 16);
            LookDevKit.Cyl(lamp, "Arm", new Vector3(0f, 0.25f, -0.05f), 0.012f, 0.46f, L.SteelDark, new Vector3(-12f, 0f, 0f), true, 8);
            BlockoutKit.MeshObj(lamp, "Shade", MeshFactory.Cone(0.14f, 0.18f), new Vector3(0f, 0.46f, -0.16f), lampGreen, true, new Vector3(-35f, 0f, 0f));
            var bulb = BlockoutKit.Sphere(lamp, "Bulb", new Vector3(0f, 0.41f, -0.19f), 0.04f, L.LampBulb);
            bulb.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            var lightGo = new GameObject("DeskLamp_Baked");
            lightGo.transform.SetParent(lamp, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.32f, -0.3f);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.lightmapBakeType = LightmapBakeType.Baked;
            l.color = LabMaterials.Hex("#FFC27A");
            l.intensity = 2.2f;
            l.range = 3f;
            lightGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();

            // Emaille-Tasse mit rotem Streifen
            var mug = BlockoutKit.Group(g, "Mug", new Vector3(0.25f, top, -0.3f));
            LookDevKit.Obj(mug, "Cup", LookDevKit.Lathe("Mug", new[] { new Vector2(0f, 0f), new Vector2(0.045f, 0f), new Vector2(0.048f, 0.11f), new Vector2(0.042f, 0.11f), new Vector2(0.04f, 0.01f), new Vector2(0f, 0.01f) }, 18, true), Vector3.zero, L.EnamelCream);
            LookDevKit.Obj(mug, "Band", LookDevKit.Ring(0.047f, 0.05f, 0.02f, 18), new Vector3(0f, 0.07f, 0f), L.RedPaint);
            LookDevKit.Obj(mug, "Handle", LookDevKit.Ring(0.022f, 0.032f, 0.012f, 12), new Vector3(0.055f, 0.055f, 0f), L.EnamelCream, new Vector3(90f, 0f, 0f));

            // Papiere
            LookDevKit.Plane(g, "Papers", new Vector3(-0.4f, top + 0.002f, -0.28f), new Vector2(0.24f, 0.32f), L.Paper, new Vector3(90f, 18f, 0f));
            LookDevKit.Plane(g, "Papers", new Vector3(-0.32f, top + 0.004f, -0.25f), new Vector2(0.24f, 0.32f), L.Checklist, new Vector3(90f, -9f, 0f));

            // Reagenzglas-Ständer
            var rack = BlockoutKit.Group(g, "TubeRack", new Vector3(0.12f, top, 0.32f));
            LookDevKit.Box(rack, "Base", new Vector3(0f, 0.01f, 0f), new Vector3(0.32f, 0.02f, 0.1f), L.Wood);
            LookDevKit.Box(rack, "Bar", new Vector3(0f, 0.1f, 0f), new Vector3(0.32f, 0.02f, 0.1f), L.Wood);
            var tubeGlass = LookDevKit.Glass("LD_TubeGlass", new Color(0.9f, 0.97f, 1f, 0.25f), 0.95f);
            var tubeCols = new[] { "#46D14F", "#3AA0FF", "#FFCC2A", "#6DFF4A", "#FF6BD5" };
            for (int k = 0; k < tubeCols.Length; k++)
            {
                float x = -0.12f + k * 0.06f;
                var liq = BlockoutKit.Emissive("LD_Tube_" + tubeCols[k].TrimStart('#'), LabMaterials.Hex(tubeCols[k]), 0.9f, false);
                LookDevKit.Cyl(rack, "Tube", new Vector3(x, 0.1f, 0f), 0.016f, 0.17f, tubeGlass, null, true, 10);
                LookDevKit.Cyl(rack, "Liquid", new Vector3(x, 0.06f, 0f), 0.013f, 0.08f, liq, null, true, 10);
            }
            Solid(g, "Collider", new Vector3(0f, 0.45f, 0f), new Vector3(2.2f, 0.9f, 1.0f));

            // Bürostuhl vor dem Tisch
            var chair = BlockoutKit.Group(g, "Chair", new Vector3(0.1f, 0f, -0.75f), 8f);
            var seatRed = BlockoutKit.Mat("LD_ChairRed", LabMaterials.Hex("#8E1B1B"), 0f, 0.3f);
            LookDevKit.Box(chair, "Seat", new Vector3(0f, 0.47f, 0f), new Vector3(0.46f, 0.07f, 0.44f), seatRed);
            LookDevKit.Box(chair, "Back", new Vector3(0f, 0.78f, -0.22f), new Vector3(0.42f, 0.4f, 0.06f), seatRed, new Vector3(-8f, 0f, 0f));
            LookDevKit.Cyl(chair, "Pole", new Vector3(0f, 0.25f, 0f), 0.025f, 0.42f, L.Chrome, null, true, 8);
            for (int k = 0; k < 5; k++)
                LookDevKit.Box(chair, "Foot", new Vector3(0f, 0.04f, 0f), new Vector3(0.05f, 0.04f, 0.56f), L.SteelDark, new Vector3(0f, k * 72f, 0f));
            Solid(chair, "Collider", new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 1f, 0.5f));
        }

        /// <summary>Roter Teppich mit Emblem (Bild 07), flach auf dem Boden.</summary>
        static void Carpet(Transform parent, Vector3 pos, Vector2 size)
        {
            var m = LookDevKit.Textured("LD_Carpet", "Carpet.png", Color.white, 0f, 0.05f);
            var c = LookDevKit.Plane(parent, "Carpet", pos + new Vector3(0f, 0.012f, 0f), size, m, new Vector3(90f, 0f, 0f));
            c.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>Aktenschrank mit vier Schubladen, Griffen und Etiketten.</summary>
        static void FileCabinet(Transform parent, Vector3 pos)
        {
            var g = BlockoutKit.Group(parent, "FileCabinet", pos, 90f);
            LookDevKit.Box(g, "Body", new Vector3(0f, 0.7f, 0f), new Vector3(0.7f, 1.4f, 0.7f), L.SteelPanel);
            for (int k = 0; k < 4; k++)
            {
                float y = 1.22f - k * 0.33f;
                LookDevKit.Box(g, "Drawer", new Vector3(0f, y, -0.355f), new Vector3(0.62f, 0.29f, 0.012f), L.SteelPanel);
                LookDevKit.Box(g, "Handle", new Vector3(0f, y - 0.05f, -0.37f), new Vector3(0.18f, 0.025f, 0.025f), L.Chrome);
                LookDevKit.Box(g, "Label", new Vector3(0f, y + 0.06f, -0.362f), new Vector3(0.12f, 0.05f, 0.003f), L.Paper);
            }
            Solid(g, "Collider", new Vector3(0f, 0.7f, 0f), new Vector3(0.7f, 1.4f, 0.7f));
        }

        static void Extinguisher(Transform parent, Vector3 pos)
        {
            var g = BlockoutKit.Group(parent, "Extinguisher", pos);
            LookDevKit.Box(g, "Bracket", new Vector3(-0.02f, 1.0f, 0f), new Vector3(0.03f, 0.3f, 0.12f), L.SteelDark);
            LookDevKit.Obj(g, "Bottle", LookDevKit.Lathe("Extinguisher", new[] { new Vector2(0f, 0f), new Vector2(0.12f, 0f), new Vector2(0.13f, 0.03f), new Vector2(0.13f, 0.52f), new Vector2(0.09f, 0.6f), new Vector2(0.03f, 0.63f), new Vector2(0f, 0.63f) }, 18),
                new Vector3(0.13f, 0.7f, 0f), L.RedPaint);
            LookDevKit.Box(g, "Head", new Vector3(0.13f, 1.36f, 0f), new Vector3(0.06f, 0.1f, 0.2f), L.BlackPaint);
            LookDevKit.Cyl(g, "Hose", new Vector3(0.24f, 1.05f, 0.08f), 0.015f, 0.6f, L.Rubber, new Vector3(0f, 0f, -8f), true, 8);
        }

        /// <summary>Topfpflanze (Bild 07): Tontopf mit schmalen Blättern.</summary>
        static void Plant(Transform parent, Vector3 pos)
        {
            var g = BlockoutKit.Group(parent, "Plant", pos);
            var clay = BlockoutKit.Mat("LD_Clay", LabMaterials.Hex("#B5653A"), 0f, 0.2f);
            var leaf = BlockoutKit.Mat("LD_Leaf", LabMaterials.Hex("#2F7A3A"), 0f, 0.35f);
            LookDevKit.Obj(g, "Pot", LookDevKit.Lathe("PlantPot", new[] { new Vector2(0f, 0f), new Vector2(0.24f, 0f), new Vector2(0.3f, 0.46f), new Vector2(0.33f, 0.5f), new Vector2(0.29f, 0.5f), new Vector2(0f, 0.46f) }, 18), Vector3.zero, clay);
            var leafMesh = LookDevKit.Lathe("PlantLeaf", new[] { new Vector2(0f, 0f), new Vector2(0.06f, 0.15f), new Vector2(0.07f, 0.4f), new Vector2(0.04f, 0.6f), new Vector2(0f, 0.75f) }, 8);
            for (int k = 0; k < 9; k++)
            {
                float a = k * 40f;
                LookDevKit.Obj(g, "Leaf", leafMesh, new Vector3(Mathf.Cos(a * Mathf.Deg2Rad) * 0.06f, 0.42f, Mathf.Sin(a * Mathf.Deg2Rad) * 0.06f), leaf,
                    new Vector3(0f, a, 18f + (k % 3) * 10f)).transform.localScale = new Vector3(1f, 1f + (k % 2) * 0.3f, 0.35f);
            }
            Solid(g, "Collider", new Vector3(0f, 0.4f, 0f), new Vector3(0.6f, 0.8f, 0.6f));
        }

        /// <summary>Wanduhr mit Zeiger, der den Schichtfortschritt zeigt (WallClock).</summary>
        static void Clock(Transform parent, Vector3 pos)
        {
            var g = BlockoutKit.Group(parent, "WallClock", pos, 180f);
            LookDevKit.Cyl(g, "Face", new Vector3(0f, 0f, 0f), 0.5f, 0.04f, L.CreamPlate, new Vector3(90f, 0f, 0f), true, 32);
            LookDevKit.Obj(g, "Rim", LookDevKit.Ring(0.48f, 0.54f, 0.08f, 32), new Vector3(0f, 0f, -0.02f), L.BlackPaint, new Vector3(90f, 0f, 0f));
            for (int k = 0; k < 12; k++)
            {
                float a = k * 30f * Mathf.Deg2Rad;
                LookDevKit.Box(g, "Tick", new Vector3(Mathf.Sin(a) * 0.4f, Mathf.Cos(a) * 0.4f, -0.025f), new Vector3(0.03f, k % 3 == 0 ? 0.1f : 0.06f, 0.005f), L.BlackPaint, new Vector3(0f, 0f, -k * 30f));
            }
            var pivot = BlockoutKit.Group(g, "HandPivot", new Vector3(0f, 0f, -0.03f));
            LookDevKit.Box(pivot, "Hand", new Vector3(0f, 0.16f, 0f), new Vector3(0.03f, 0.36f, 0.008f), L.BlackPaint, null, false);
            LookDevKit.Cyl(g, "Cap", new Vector3(0f, 0f, -0.036f), 0.03f, 0.01f, L.Brass, new Vector3(90f, 0f, 0f), true, 12);
            BlockoutKit.SetStatic(pivot.gameObject, false);
            g.gameObject.AddComponent<WallClock>().Setup(pivot);
        }

        /// <summary>Unsichtbarer Kollider (Spieler läuft nicht durch Möbel).</summary>
        static void Solid(Transform g, string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(g, false);
            var bc = go.AddComponent<BoxCollider>();
            bc.center = center;
            bc.size = size;
        }
    }
}
