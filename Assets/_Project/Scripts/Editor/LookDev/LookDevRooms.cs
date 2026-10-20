using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Einrichtung der Nebenzimmer: Lager (Regale, Kisten, Fässer; Terminal und Lagerausgabe sind Stationen),
    /// Umkleide (Spinde, Bank, Spiegel, Kittel), Schlafraum (Stockbetten, Nachttisch, Teppich), Kantine (Tafel mit
    /// Bänken, Ausgabe mit Töpfen und Samowar, Speiseplan) und Direktorbüro (Schreibtisch, Telefone, Porträt, Fahne,
    /// Bücherregal, Tresor, Globus). Alles eigene Meshes; Texte aus ui.rooms.
    /// </summary>
    public static class LookDevRooms
    {
        /// <summary>Innenmaße eines Zimmers in Unity-Koordinaten.</summary>
        struct Box2
        {
            public float xl, xr, zf, zb;
            public float Cx => (xl + xr) * 0.5f;
            public float Cz => (zf + zb) * 0.5f;
        }

        public static void Build(Transform parent, SideRoomLayout room, LayoutConfig layout)
        {
            float x = layout.room.sideWallX, half = layout.wing.partition * 0.5f;
            var b = new Box2
            {
                xl = room.xMin + (room.xMin > -x + 0.01f ? half : 0f),
                xr = room.xMax - (room.xMax < x - 0.01f ? half : 0f),
                zf = -layout.WingZMin,
                zb = -layout.WingZMax
            };
            var g = BlockoutKit.Group(parent, "Furniture", Vector3.zero);
            switch (room.id)
            {
                case "storage": Storage(g, b); break;
                case "locker": LockerRoom(g, b); break;
                case "bedroom": Bedroom(g, b); break;
                case "canteen": Canteen(g, b); break;
                case "director": Director(g, b, layout, room); break;
            }
        }

        // ---------- Lager ----------

        static void Storage(Transform g, Box2 b)
        {
            Rack(g, new Vector3(b.xl + 0.4f, 0f, b.zf - 1.8f), 3f, 0);
            Rack(g, new Vector3(b.xl + 0.4f, 0f, b.zf - 5.0f), 3f, 3);
            Crate(g, new Vector3(b.xr - 1.1f, 0f, b.zb + 0.7f), 1.2f, 12f);
            Crate(g, new Vector3(b.xr - 2.4f, 0f, b.zb + 0.55f), 0.9f, -8f);
            Crate(g, new Vector3(b.xr - 1.15f, 1.2f, b.zb + 0.7f), 0.7f, 30f);
            Barrel(g, new Vector3(b.xr - 0.6f, 0f, b.zf - 0.65f), L.RedPaint);
            Barrel(g, new Vector3(b.xr - 0.6f, 0f, b.zf - 1.65f), L.GreenPaint);

            // Palette mit Säcken
            var p = BlockoutKit.Group(g, "Pallet", new Vector3(b.xl + 1.5f, 0f, b.zf - 0.8f), 4f);
            LookDevKit.Box(p, "Deck", new Vector3(0f, 0.12f, 0f), new Vector3(1.2f, 0.04f, 1.0f), L.CratePlanks);
            foreach (var x in new[] { -0.55f, 0f, 0.55f })
                LookDevKit.Box(p, "Block", new Vector3(x, 0.05f, 0f), new Vector3(0.1f, 0.1f, 1.0f), L.Wood);
            var sack = BlockoutKit.Mat("LD_Sack", M.Hex("#B8A57E"), 0f, 0.1f);
            for (int i = 0; i < 5; i++)
                LookDevKit.Box(p, "Sack", new Vector3(i < 3 ? (i - 1) * 0.38f : (i - 3.5f) * 0.38f, i < 3 ? 0.27f : 0.53f, 0f),
                    new Vector3(0.36f, 0.26f, 0.85f), sack, new Vector3(0f, (i * 7f) % 10f - 5f, 0f));
            LookDevDecor.Solid(p, "Collider", new Vector3(0f, 0.4f, 0f), new Vector3(1.2f, 0.8f, 1.0f));

            WallSign(g, "ui.rooms.storageRule", new Vector3(b.xl + 2.6f, 2.7f, b.zb + 0.03f), 180f, 3.2f, L.BlackPaint, M.Hex("#FFD24A"));
        }

        /// <summary>Stahlregal (Längsachse z, an der linken Wand) mit Kanistern in den Grundstoff-Farben und Kartons.</summary>
        static void Rack(Transform parent, Vector3 pos, float len, int seed)
        {
            var g = BlockoutKit.Group(parent, "Rack", pos);
            const float depth = 0.7f, h = 2.6f;
            foreach (var y in new[] { 0.15f, 0.8f, 1.45f, 2.1f })
                LookDevKit.Box(g, "Shelf", new Vector3(0f, y, 0f), new Vector3(depth, 0.04f, len), L.SteelPanel);
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                    LookDevKit.Box(g, "Post", new Vector3(sx * (depth * 0.5f - 0.03f), h * 0.5f, sz * (len * 0.5f - 0.03f)), new Vector3(0.05f, h, 0.05f), L.SteelDark);
            var liquids = new[] { M.LiquidA, M.LiquidB, M.LiquidC };
            for (int lvl = 0; lvl < 4; lvl++)
            {
                float y = 0.17f + lvl * 0.65f;
                for (int k = 0; k < 5; k++)
                {
                    float z = -len * 0.5f + 0.35f + k * (len - 0.7f) / 4f;
                    int v = (k + lvl + seed) % 4;
                    if (v == 3)
                    {
                        LookDevKit.Box(g, "Carton", new Vector3(0f, y + 0.18f, z), new Vector3(0.5f, 0.36f, 0.45f), L.CratePlanks, new Vector3(0f, k * 9f, 0f));
                        continue;
                    }
                    LookDevKit.Cyl(g, "Canister", new Vector3(0f, y + 0.2f, z), 0.16f, 0.4f, L.SteelPanel, null, true, 16);
                    LookDevKit.Cyl(g, "Band", new Vector3(0f, y + 0.22f, z), 0.165f, 0.12f, liquids[v], null, true, 16);
                    LookDevKit.Cyl(g, "Cap", new Vector3(0f, y + 0.42f, z), 0.05f, 0.04f, L.SteelDark, null, true, 8);
                }
            }
            LookDevDecor.Solid(g, "Collider", new Vector3(0f, h * 0.5f, 0f), new Vector3(depth, h, len));
        }

        /// <summary>Holzkiste mit Leisten (wie früher in der Halle).</summary>
        internal static void Crate(Transform parent, Vector3 pos, float size, float yaw)
        {
            var crate = BlockoutKit.Group(parent, "Crate", pos, yaw);
            LookDevKit.Box(crate, "Box", new Vector3(0f, size * 0.5f, 0f), Vector3.one * size, L.CratePlanks);
            foreach (var s in new[] { -1f, 1f })
            {
                LookDevKit.Box(crate, "Slat", new Vector3(0f, size * 0.5f, s * (size * 0.5f + 0.012f)), new Vector3(size + 0.02f, 0.1f, 0.025f), L.Wood);
                LookDevKit.Box(crate, "Slat", new Vector3(s * (size * 0.5f + 0.012f), size * 0.5f, 0f), new Vector3(0.025f, 0.1f, size + 0.02f), L.Wood);
            }
            LookDevDecor.Solid(crate, "Collider", new Vector3(0f, size * 0.5f, 0f), Vector3.one * size);
        }

        /// <summary>Fass mit Sicken und Kapsel-Kollider.</summary>
        internal static void Barrel(Transform parent, Vector3 pos, Material mat)
        {
            var mesh = LookDevKit.Lathe("Barrel", new[] { new Vector2(0f, 0f), new Vector2(0.43f, 0f), new Vector2(0.45f, 0.04f), new Vector2(0.45f, 0.3f),
                new Vector2(0.47f, 0.32f), new Vector2(0.45f, 0.34f), new Vector2(0.45f, 0.76f), new Vector2(0.47f, 0.78f), new Vector2(0.45f, 0.8f),
                new Vector2(0.45f, 1.06f), new Vector2(0.43f, 1.1f), new Vector2(0f, 1.1f) }, 24);
            var go = LookDevKit.Obj(parent, "Barrel", mesh, pos, mat, new Vector3(0f, pos.x * 40f, 0f));
            var cc = go.AddComponent<CapsuleCollider>();
            cc.center = new Vector3(0f, 0.55f, 0f);
            cc.radius = 0.45f;
            cc.height = 1.1f;
        }

        // ---------- Umkleide ----------

        static void LockerRoom(Transform g, Box2 b)
        {
            // Spinde an beiden Seitenwänden, Türen zur Raummitte
            for (int i = 0; i < 6; i++)
            {
                float z = b.zf - 1.25f - i * 0.78f;
                LookDevDecor.Locker(g, new Vector3(b.xl + 0.31f, 0f, z), i, -90f);
                LookDevDecor.Locker(g, new Vector3(b.xr - 0.31f, 0f, z), i + 1, 90f);
            }
            float zl = b.zf - 1.25f - 2.5f * 0.78f;
            LookDevDecor.Solid(g, "LockersLeft", new Vector3(b.xl + 0.31f, 1.05f, zl), new Vector3(0.6f, 2.1f, 6f * 0.78f));
            LookDevDecor.Solid(g, "LockersRight", new Vector3(b.xr - 0.31f, 1.05f, zl), new Vector3(0.6f, 2.1f, 6f * 0.78f));

            // Bank in der Mitte
            var bench = BlockoutKit.Group(g, "Bench", new Vector3(b.Cx, 0f, b.Cz - 0.2f));
            LookDevKit.Box(bench, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(0.42f, 0.06f, 3.2f), L.Wood);
            LookDevKit.Box(bench, "Rail", new Vector3(0f, 0.2f, 0f), new Vector3(0.05f, 0.05f, 3.0f), L.SteelDark);
            foreach (var z in new[] { -1.4f, 0f, 1.4f })
                LookDevKit.Box(bench, "Leg", new Vector3(0f, 0.21f, z), new Vector3(0.36f, 0.42f, 0.05f), L.SteelDark);
            LookDevKit.Box(bench, "Towel", new Vector3(0.05f, 0.49f, 0.9f), new Vector3(0.3f, 0.03f, 0.45f), Linen());
            LookDevDecor.Solid(bench, "Collider", new Vector3(0f, 0.25f, 0f), new Vector3(0.45f, 0.5f, 3.2f));

            // Spiegel und Kittel an der Rückwand
            var back = BlockoutKit.Group(g, "BackWall", new Vector3(b.Cx, 0f, b.zb), 180f);
            LookDevKit.Box(back, "MirrorFrame", new Vector3(0f, 1.6f, -0.02f), new Vector3(1.1f, 0.8f, 0.03f), L.SteelDark);
            LookDevKit.Box(back, "Mirror", new Vector3(0f, 1.6f, -0.04f), new Vector3(1.0f, 0.7f, 0.01f), L.Chrome);
            LookDevKit.Box(back, "Sink", new Vector3(0f, 0.85f, -0.25f), new Vector3(0.6f, 0.16f, 0.45f), L.EnamelCream);
            LookDevKit.Cyl(back, "Tap", new Vector3(0f, 1.0f, -0.08f), 0.02f, 0.18f, L.Chrome, null, true, 8);
            foreach (var x in new[] { -1.5f, -1.0f, 1.0f, 1.5f })
            {
                LookDevKit.Cyl(back, "Hook", new Vector3(x, 1.85f, -0.05f), 0.015f, 0.1f, L.Brass, new Vector3(90f, 0f, 0f), true, 8);
                LookDevKit.Box(back, "Coat", new Vector3(x, 1.32f, -0.09f), new Vector3(0.48f, 1.0f, 0.1f), Linen(), new Vector3(0f, 0f, x * 2f));
            }
            WallSign(g, "ui.rooms.lockerRule", new Vector3(b.Cx, 2.75f, b.zb + 0.03f), 180f, 2.6f, L.GreenPaint, M.CreamText);
        }

        // ---------- Schlafraum ----------

        static void Bedroom(Transform g, Box2 b)
        {
            Bunk(g, new Vector3(b.xl + 0.55f, 0f, b.zf - 2.4f));
            Bunk(g, new Vector3(b.xl + 0.55f, 0f, b.zb + 1.25f));
            Bunk(g, new Vector3(b.xr - 0.55f, 0f, b.zb + 1.25f));
            LookDevDecor.Carpet(g, new Vector3(b.Cx, 0f, b.Cz - 0.3f), new Vector2(2.0f, 3.0f));

            // Nachttisch mit Wecker und Lampe
            var n = BlockoutKit.Group(g, "Nightstand", new Vector3(b.xr - 0.4f, 0f, b.zf - 2.6f), 90f);
            LookDevKit.Box(n, "Body", new Vector3(0f, 0.3f, 0f), new Vector3(0.5f, 0.6f, 0.45f), L.Wood);
            LookDevKit.Box(n, "Drawer", new Vector3(0f, 0.42f, -0.23f), new Vector3(0.42f, 0.16f, 0.01f), L.Wood);
            LookDevKit.Box(n, "Knob", new Vector3(0f, 0.42f, -0.245f), new Vector3(0.05f, 0.03f, 0.02f), L.Brass);
            LookDevKit.Cyl(n, "Clock", new Vector3(-0.1f, 0.68f, 0f), 0.08f, 0.05f, L.RedPaint, new Vector3(90f, 0f, 0f), true, 16);
            LookDevKit.Cyl(n, "ClockFace", new Vector3(-0.1f, 0.68f, -0.027f), 0.065f, 0.005f, L.CreamPlate, new Vector3(90f, 0f, 0f), true, 16);
            foreach (var bx in new[] { -0.15f, -0.05f })
                LookDevKit.Cyl(n, "Bell", new Vector3(bx, 0.78f, 0f), 0.03f, 0.02f, L.Brass, null, true, 10);
            LookDevKit.Cyl(n, "LampFoot", new Vector3(0.12f, 0.62f, 0f), 0.05f, 0.04f, L.SteelDark, null, true, 12);
            LookDevKit.Cyl(n, "LampRod", new Vector3(0.12f, 0.75f, 0f), 0.01f, 0.25f, L.SteelDark, null, true, 6);
            BlockoutKit.MeshObj(n, "LampShade", MeshFactory.Cone(0.11f, 0.12f), new Vector3(0.12f, 0.92f, 0f), L.GreenPaint);
            LookDevDecor.Solid(n, "Collider", new Vector3(0f, 0.3f, 0f), new Vector3(0.5f, 0.6f, 0.45f));

            // Hocker mit gefalteter Uniform und Stiefeln
            var s = BlockoutKit.Group(g, "Stool", new Vector3(b.xl + 1.5f, 0f, b.zf - 2.4f));
            LookDevKit.Cyl(s, "Seat", new Vector3(0f, 0.45f, 0f), 0.2f, 0.04f, L.Wood, null, true, 16);
            for (int k = 0; k < 3; k++)
            {
                float a = k * 120f * Mathf.Deg2Rad;
                LookDevKit.Cyl(s, "Leg", new Vector3(Mathf.Cos(a) * 0.14f, 0.22f, Mathf.Sin(a) * 0.14f), 0.02f, 0.44f, L.Wood, null, true, 6);
            }
            var uniform = BlockoutKit.Mat("LD_Uniform", M.Hex("#5B5E3E"), 0f, 0.15f);
            LookDevKit.Box(s, "Uniform", new Vector3(0f, 0.5f, 0f), new Vector3(0.32f, 0.07f, 0.26f), uniform, new Vector3(0f, 12f, 0f));
            foreach (var bx in new[] { -0.12f, 0.12f })
                LookDevKit.Box(s, "Boot", new Vector3(bx + 0.45f, 0.15f, 0.1f), new Vector3(0.11f, 0.3f, 0.26f), L.Rubber);

            // Heizkörper unter der Parole
            var rad = BlockoutKit.Group(g, "Radiator", new Vector3(b.Cx, 0f, b.zb + 0.12f));
            for (int k = 0; k < 10; k++)
                LookDevKit.Box(rad, "Fin", new Vector3(-0.45f + k * 0.1f, 0.5f, 0f), new Vector3(0.06f, 0.65f, 0.12f), L.EnamelCream);
            LookDevKit.Cyl(rad, "Pipe", new Vector3(0f, 0.12f, 0f), 0.02f, 1.2f, L.SteelDark, new Vector3(0f, 0f, 90f), true, 8);
            WallSign(g, "ui.rooms.bedroomRule", new Vector3(b.Cx, 2.3f, b.zb + 0.03f), 180f, 2.8f, L.RedPaint, M.CreamText);
        }

        /// <summary>Eisernes Stockbett (Längsachse z) mit Matratzen, Decken, Kissen und Leiter.</summary>
        static void Bunk(Transform parent, Vector3 pos)
        {
            var g = BlockoutKit.Group(parent, "Bunk", pos);
            const float w = 0.95f, len = 2.05f, h = 1.9f;
            var blanket = BlockoutKit.Mat("LD_Blanket", M.Hex("#4E5A3C"), 0f, 0.1f);
            var stripe = BlockoutKit.Mat("LD_BlanketStripe", M.Hex("#8E2A22"), 0f, 0.1f);
            foreach (var sx in new[] { -1f, 1f })
                foreach (var sz in new[] { -1f, 1f })
                    LookDevKit.Cyl(g, "Post", new Vector3(sx * w * 0.5f, h * 0.5f, sz * len * 0.5f), 0.03f, h, L.SteelDark, null, true, 8);
            foreach (var y in new[] { 0.38f, 1.38f })
            {
                foreach (var sx in new[] { -1f, 1f })
                    LookDevKit.Cyl(g, "Rail", new Vector3(sx * w * 0.5f, y, 0f), 0.022f, len, L.SteelDark, new Vector3(90f, 0f, 0f), true, 8);
                LookDevKit.Box(g, "Mattress", new Vector3(0f, y + 0.09f, 0f), new Vector3(w - 0.08f, 0.15f, len - 0.08f), L.CreamPlate);
                LookDevKit.Box(g, "Blanket", new Vector3(0f, y + 0.175f, -0.25f), new Vector3(w - 0.04f, 0.03f, len * 0.68f), blanket);
                LookDevKit.Box(g, "Stripe", new Vector3(0f, y + 0.192f, 0.3f), new Vector3(w - 0.03f, 0.005f, 0.12f), stripe);
                LookDevKit.Box(g, "Pillow", new Vector3(0f, y + 0.22f, len * 0.5f - 0.3f), new Vector3(0.55f, 0.1f, 0.32f), Linen());
            }
            foreach (var sz in new[] { -1f, 1f })
                LookDevKit.Cyl(g, "HeadBar", new Vector3(0f, h - 0.1f, sz * len * 0.5f), 0.022f, w, L.SteelDark, new Vector3(0f, 0f, 90f), true, 8);
            for (int k = 0; k < 4; k++)
                LookDevKit.Cyl(g, "Rung", new Vector3(w * 0.5f + 0.02f, 0.5f + k * 0.3f, -len * 0.5f + 0.3f), 0.015f, 0.3f, L.SteelDark, new Vector3(90f, 0f, 0f), true, 6);
            LookDevDecor.Solid(g, "Collider", new Vector3(0f, h * 0.5f, 0f), new Vector3(w + 0.1f, h, len + 0.05f));
        }

        // ---------- Kantine ----------

        static void Canteen(Transform g, Box2 b)
        {
            // Lange Tafel mit Bänken, Schüsseln, Brot, Blechbechern
            var t = BlockoutKit.Group(g, "Table", new Vector3(b.Cx, 0f, b.Cz + 0.5f));
            const float len = 3.6f;
            LookDevKit.Box(t, "Top", new Vector3(0f, 0.76f, 0f), new Vector3(1.0f, 0.05f, len), L.Wood);
            foreach (var sz in new[] { -1f, 1f })
                LookDevKit.Box(t, "Leg", new Vector3(0f, 0.37f, sz * (len * 0.5f - 0.25f)), new Vector3(0.8f, 0.74f, 0.06f), L.SteelDark);
            foreach (var sx in new[] { -1f, 1f })
            {
                LookDevKit.Box(t, "Bench", new Vector3(sx * 0.85f, 0.44f, 0f), new Vector3(0.32f, 0.05f, len - 0.2f), L.Wood);
                foreach (var sz in new[] { -1f, 1f })
                    LookDevKit.Box(t, "BenchLeg", new Vector3(sx * 0.85f, 0.21f, sz * (len * 0.5f - 0.4f)), new Vector3(0.28f, 0.42f, 0.05f), L.SteelDark);
            }
            var bowl = LookDevKit.Lathe("SoupBowl", new[] { new Vector2(0f, 0f), new Vector2(0.06f, 0f), new Vector2(0.1f, 0.05f), new Vector2(0.11f, 0.07f), new Vector2(0.1f, 0.07f), new Vector2(0f, 0.03f) }, 16, true);
            var soup = BlockoutKit.Mat("LD_Soup", M.Hex("#9C7A3C"), 0f, 0.6f);
            for (int k = 0; k < 6; k++)
            {
                float sx = k % 2 == 0 ? -0.3f : 0.3f, z = -1.4f + (k / 2) * 1.4f;
                LookDevKit.Obj(t, "Bowl", bowl, new Vector3(sx, 0.785f, z), L.EnamelCream);
                LookDevKit.Cyl(t, "Soup", new Vector3(sx, 0.83f, z), 0.09f, 0.005f, soup, null, true, 12);
                LookDevKit.Box(t, "Spoon", new Vector3(sx + 0.15f, 0.79f, z), new Vector3(0.02f, 0.008f, 0.16f), L.Chrome);
                LookDevKit.Cyl(t, "Cup", new Vector3(sx * 0.4f, 0.83f, z + 0.25f), 0.04f, 0.1f, L.SteelPanel, null, true, 10);
            }
            var bread = BlockoutKit.Mat("LD_Bread", M.Hex("#5A3A1E"), 0f, 0.2f);
            LookDevKit.Box(t, "Bread", new Vector3(0f, 0.83f, 0.7f), new Vector3(0.18f, 0.1f, 0.3f), bread);
            LookDevDecor.Solid(t, "Collider", new Vector3(0f, 0.4f, 0f), new Vector3(2.0f, 0.8f, len));

            // Essensausgabe an der Rückwand: Theke, Töpfe, Samowar, Tablettstapel
            var c = BlockoutKit.Group(g, "Counter", new Vector3(b.Cx, 0f, b.zb + 0.45f));
            float cw = b.xr - b.xl - 1.2f;
            LookDevKit.Box(c, "Body", new Vector3(0f, 0.48f, 0f), new Vector3(cw, 0.96f, 0.8f), L.CreamPlate);
            LookDevKit.Box(c, "Top", new Vector3(0f, 0.98f, 0f), new Vector3(cw + 0.06f, 0.04f, 0.86f), L.SteelPanel);
            LookDevKit.Box(c, "Kick", new Vector3(0f, 0.06f, 0.4f), new Vector3(cw, 0.12f, 0.02f), L.SteelDark);
            foreach (var px in new[] { -1.6f, -0.8f })
            {
                LookDevKit.Cyl(c, "Pot", new Vector3(px, 1.18f, 0f), 0.28f, 0.38f, L.SteelPanel, null, true, 20);
                LookDevKit.Cyl(c, "Lid", new Vector3(px, 1.39f, 0f), 0.29f, 0.03f, L.SteelDark, null, true, 20);
                LookDevKit.Cyl(c, "Knob", new Vector3(px, 1.43f, 0f), 0.04f, 0.05f, L.Rubber, null, true, 8);
            }
            var samovar = LookDevKit.Lathe("Samovar", new[] { new Vector2(0f, 0f), new Vector2(0.16f, 0f), new Vector2(0.1f, 0.08f), new Vector2(0.2f, 0.18f),
                new Vector2(0.24f, 0.42f), new Vector2(0.18f, 0.58f), new Vector2(0.08f, 0.62f), new Vector2(0.06f, 0.75f), new Vector2(0f, 0.75f) }, 24);
            LookDevKit.Obj(c, "Samovar", samovar, new Vector3(1.4f, 1.0f, 0f), L.Brass);
            LookDevKit.Cyl(c, "SamovarTap", new Vector3(1.4f, 1.22f, 0.25f), 0.02f, 0.12f, L.Brass, new Vector3(90f, 0f, 0f), true, 8);
            for (int k = 0; k < 6; k++)
                LookDevKit.Box(c, "Tray", new Vector3(0.4f, 1.01f + k * 0.022f, 0.05f), new Vector3(0.45f, 0.018f, 0.32f), L.SteelPanel, new Vector3(0f, k * 3f, 0f));
            LookDevDecor.Solid(c, "Collider", new Vector3(0f, 0.5f, 0f), new Vector3(cw, 1f, 0.86f));

            // Speiseplan als Kreidetafel
            var board = BlockoutKit.Group(g, "MenuBoard", new Vector3(b.Cx - 0.9f, 2.4f, b.zb + 0.03f), 180f);
            var chalk = M.Hex("#E8E4D4");
            LookDevKit.Box(board, "Frame", new Vector3(0f, 0f, 0.01f), new Vector3(2.3f, 1.35f, 0.04f), L.Wood);
            LookDevKit.Box(board, "Slate", new Vector3(0f, 0f, -0.012f), new Vector3(2.15f, 1.2f, 0.01f), L.Rubber);
            LookDevKit.Label(board, "Title", "ui.rooms.canteenMenuTitle", new Vector3(0f, 0.4f, -0.02f), 2.6f, M.Hex("#FFD24A"));
            for (int k = 0; k < 3; k++)
                LookDevKit.Label(board, "Line_" + k, $"ui.rooms.canteenMenu[{k}]", new Vector3(0f, 0.08f - k * 0.26f, -0.02f), 1.5f, chalk);

            Barrel(g, new Vector3(b.xr - 0.6f, 0f, b.Cz + 0.3f), L.GreenPaint);
        }

        // ---------- Direktorbüro ----------

        static void Director(Transform g, Box2 b, LayoutConfig layout, SideRoomLayout room)
        {
            LookDevDecor.Carpet(g, new Vector3(b.Cx, 0f, b.Cz - 0.2f), new Vector2(3.2f, 4.0f));

            // Schreibtisch (Vorderseite zur Tür)
            var d = BlockoutKit.Group(g, "DirectorDesk", new Vector3(b.Cx, 0f, b.zb + 2.2f), 180f);
            const float top = 0.8f;
            LookDevKit.Box(d, "Top", new Vector3(0f, top - 0.03f, 0f), new Vector3(2.6f, 0.06f, 1.1f), L.Wood);
            LookDevKit.Box(d, "Inlay", new Vector3(0f, top + 0.001f, 0.05f), new Vector3(1.4f, 0.002f, 0.6f), L.GreenPaint);
            foreach (var sx in new[] { -1f, 1f })
            {
                LookDevKit.Box(d, "Pedestal", new Vector3(sx * 0.95f, (top - 0.06f) * 0.5f, 0f), new Vector3(0.6f, top - 0.06f, 1.0f), L.Wood);
                for (int k = 0; k < 3; k++)
                    LookDevKit.Box(d, "Pull", new Vector3(sx * 0.95f, 0.6f - k * 0.22f, 0.51f), new Vector3(0.14f, 0.025f, 0.02f), L.Brass);
            }
            LookDevKit.Box(d, "ModestyPanel", new Vector3(0f, 0.45f, -0.45f), new Vector3(1.3f, 0.6f, 0.04f), L.Wood);
            // Zwei Telefone (schwarz und rot), Lampe, Akten, Tintenfass, Namensschild
            Phone(d, new Vector3(-0.9f, top, 0.15f), L.BlackPaint);
            Phone(d, new Vector3(-0.55f, top, 0.25f), L.RedPaint);
            var lamp = BlockoutKit.Group(d, "Lamp", new Vector3(0.95f, top, 0.25f));
            LookDevKit.Cyl(lamp, "Base", new Vector3(0f, 0.02f, 0f), 0.09f, 0.04f, L.Brass, null, true, 16);
            LookDevKit.Cyl(lamp, "Rod", new Vector3(0f, 0.2f, 0f), 0.012f, 0.36f, L.Brass, null, true, 8);
            LookDevKit.Box(lamp, "Shade", new Vector3(0f, 0.4f, -0.05f), new Vector3(0.36f, 0.08f, 0.16f), BlockoutKit.Mat("LD_BankerGreen", M.Hex("#1E5A34"), 0.2f, 0.7f), new Vector3(-10f, 0f, 0f));
            LookDevKit.Plane(d, "Files", new Vector3(0.2f, top + 0.003f, 0.1f), new Vector2(0.3f, 0.4f), L.Paper, new Vector3(90f, 8f, 0f));
            LookDevKit.Plane(d, "Plan", new Vector3(-0.15f, top + 0.004f, 0.0f), new Vector2(0.3f, 0.4f), L.Checklist, new Vector3(90f, -12f, 0f));
            LookDevKit.Box(d, "Inkwell", new Vector3(0.55f, top + 0.04f, 0.3f), new Vector3(0.1f, 0.08f, 0.1f), L.BlackPaint);
            var plate = BlockoutKit.Group(d, "NamePlate", new Vector3(0f, top + 0.06f, -0.45f));
            LookDevKit.Box(plate, "Plate", Vector3.zero, new Vector3(0.6f, 0.11f, 0.04f), L.Brass, new Vector3(-15f, 0f, 0f));
            LookDevKit.Label(plate, "Text", "ui.rooms.director", new Vector3(0f, 0f, -0.03f), 0.8f, M.Hex("#2A2119"), new Vector3(-15f, 0f, 0f));
            LookDevDecor.Solid(d, "Collider", new Vector3(0f, 0.42f, 0f), new Vector3(2.6f, 0.84f, 1.1f));

            // Ledersessel hinter dem Tisch
            var chair = BlockoutKit.Group(g, "Armchair", new Vector3(b.Cx, 0f, b.zb + 1.0f));
            var leather = BlockoutKit.Mat("LD_Leather", M.Hex("#5A2018"), 0.1f, 0.55f);
            LookDevKit.Box(chair, "Seat", new Vector3(0f, 0.48f, 0f), new Vector3(0.75f, 0.16f, 0.65f), leather);
            LookDevKit.Box(chair, "Back", new Vector3(0f, 1.05f, -0.3f), new Vector3(0.75f, 1.05f, 0.14f), leather, new Vector3(6f, 0f, 0f));
            foreach (var sx in new[] { -1f, 1f })
                LookDevKit.Box(chair, "Arm", new Vector3(sx * 0.42f, 0.68f, 0f), new Vector3(0.1f, 0.22f, 0.65f), leather);
            LookDevKit.Box(chair, "Foot", new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 0.55f), L.Wood);
            LookDevDecor.Solid(chair, "Collider", new Vector3(0f, 0.7f, -0.05f), new Vector3(0.9f, 1.4f, 0.75f));

            // Zwei Besucherstühle
            foreach (var sx in new[] { -0.75f, 0.75f })
            {
                var v = BlockoutKit.Group(g, "VisitorChair", new Vector3(b.Cx + sx, 0f, b.zb + 3.4f), sx * 10f);
                LookDevKit.Box(v, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.05f, 0.42f), L.Wood);
                LookDevKit.Box(v, "Back", new Vector3(0f, 0.75f, 0.2f), new Vector3(0.45f, 0.55f, 0.04f), L.Wood);
                foreach (var lx in new[] { -0.19f, 0.19f })
                    foreach (var lz in new[] { -0.17f, 0.17f })
                        LookDevKit.Box(v, "Leg", new Vector3(lx, 0.22f, lz), new Vector3(0.04f, 0.44f, 0.04f), L.Wood);
                LookDevDecor.Solid(v, "Collider", new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.9f, 0.5f));
            }

            // Porträt des Direktors mit Goldrahmen hinter dem Tisch
            var portrait = BlockoutKit.Group(g, "Portrait", new Vector3(b.Cx, 2.45f, b.zb + 0.03f), 180f);
            LookDevKit.Box(portrait, "Frame", new Vector3(0f, 0f, 0.01f), new Vector3(1.3f, 1.6f, 0.06f), L.Brass);
            LookDevKit.Plane(portrait, "Picture", new Vector3(0f, 0f, -0.025f), new Vector2(1.1f, 1.4f), L.DirectorCrop);

            // Fahne in der Ecke
            var flag = BlockoutKit.Group(g, "Flag", new Vector3(b.xl + 0.45f, 0f, b.zb + 0.45f));
            LookDevKit.Cyl(flag, "Foot", new Vector3(0f, 0.05f, 0f), 0.18f, 0.1f, L.SteelDark, null, true, 16);
            LookDevKit.Cyl(flag, "Pole", new Vector3(0f, 1.35f, 0f), 0.025f, 2.6f, L.Brass, null, true, 8);
            BlockoutKit.Sphere(flag, "Tip", new Vector3(0f, 2.7f, 0f), 0.06f, L.Brass);
            LookDevKit.Box(flag, "Cloth", new Vector3(0.2f, 2.2f, 0.25f), new Vector3(0.02f, 0.75f, 0.5f), L.BannerRed, new Vector3(0f, 35f, -6f));
            LookDevKit.Plane(flag, "Emblem", new Vector3(0.21f, 2.3f, 0.25f), new Vector2(0.22f, 0.22f), L.EmblemGold, new Vector3(0f, -55f, 0f));

            // Bücherregal an der rechten Wand
            var shelf = BlockoutKit.Group(g, "Bookshelf", new Vector3(b.xr - 0.22f, 0f, b.Cz + 0.6f), 90f);
            LookDevKit.Box(shelf, "Body", new Vector3(0f, 1.1f, 0.1f), new Vector3(1.8f, 2.2f, 0.04f), L.Wood);
            foreach (var y in new[] { 0.05f, 0.6f, 1.15f, 1.7f, 2.2f })
                LookDevKit.Box(shelf, "Board", new Vector3(0f, y, -0.05f), new Vector3(1.8f, 0.04f, 0.32f), L.Wood);
            foreach (var sx in new[] { -0.88f, 0.88f })
                LookDevKit.Box(shelf, "Side", new Vector3(sx, 1.1f, -0.05f), new Vector3(0.04f, 2.2f, 0.32f), L.Wood);
            var bookCols = new[] { "#7A1A1C", "#1D3A8A", "#3A5A2A", "#5B3A22", "#A07A2A" };
            for (int lvl = 0; lvl < 4; lvl++)
                for (int k = 0; k < 12; k++)
                {
                    if ((k + lvl * 5) % 7 == 0) continue;
                    var hex = bookCols[(k * 3 + lvl) % bookCols.Length];
                    float bh = 0.34f + ((k * 7 + lvl) % 4) * 0.03f;
                    LookDevKit.Box(shelf, "Book", new Vector3(-0.78f + k * 0.14f, 0.07f + lvl * 0.55f + bh * 0.5f, -0.05f), new Vector3(0.1f, bh, 0.26f),
                        BlockoutKit.Mat("LD_Book_" + hex.TrimStart('#'), M.Hex(hex), 0f, 0.25f));
                }
            LookDevDecor.Solid(shelf, "Collider", new Vector3(0f, 1.1f, -0.05f), new Vector3(1.8f, 2.2f, 0.36f));

            // Tresor und Globus
            var safe = BlockoutKit.Group(g, "Safe", new Vector3(b.xl + 0.45f, 0f, b.zf - 1.6f), -90f);
            LookDevKit.Box(safe, "Body", new Vector3(0f, 0.45f, 0f), new Vector3(0.7f, 0.9f, 0.65f), L.GreenPaint);
            LookDevKit.Cyl(safe, "Dial", new Vector3(0f, 0.55f, -0.33f), 0.08f, 0.03f, L.Chrome, new Vector3(90f, 0f, 0f), true, 16);
            LookDevKit.Box(safe, "Handle", new Vector3(0.18f, 0.4f, -0.34f), new Vector3(0.03f, 0.18f, 0.03f), L.Chrome);
            LookDevDecor.Solid(safe, "Collider", new Vector3(0f, 0.45f, 0f), new Vector3(0.7f, 0.9f, 0.65f));
            var globe = BlockoutKit.Group(g, "Globe", new Vector3(b.xr - 0.55f, 0f, b.zb + 0.6f));
            LookDevKit.Cyl(globe, "Stand", new Vector3(0f, 0.4f, 0f), 0.03f, 0.8f, L.Wood, null, true, 8);
            LookDevKit.Cyl(globe, "Base", new Vector3(0f, 0.03f, 0f), 0.22f, 0.06f, L.Wood, null, true, 16);
            BlockoutKit.Sphere(globe, "Ball", new Vector3(0f, 1.0f, 0f), 0.24f, BlockoutKit.Mat("LD_Globe", M.Hex("#3E6A8A"), 0f, 0.5f));
            BlockoutKit.MeshObj(globe, "Ring", MeshFactory.Torus(0.27f, 0.012f), new Vector3(0f, 1.0f, 0f), L.Brass, true, new Vector3(90f, 0f, 20f));

            // Türschild auf der Hallenseite links neben der Tür
            float zHall = -layout.room.backWallZ;
            var sign = BlockoutKit.Group(g, "DoorPlaque", new Vector3(room.doorX - layout.wing.doorWidth * 0.5f - 0.6f, 1.7f, zHall + 0.03f), 180f);
            var signGo = BlockoutKit.Sign(sign, "Text", "ui.rooms.directorPlaque", Vector3.zero, 0.7f, M.Hex("#2A2119"), L.Brass, false);
            signGo.transform.localPosition = new Vector3(0f, 0f, -0.005f);
        }

        /// <summary>Saubere Wäsche (Kittel, Kissen, Handtuch) ohne Rosttextur.</summary>
        static Material Linen() => BlockoutKit.Mat("LD_Linen", M.Hex("#E8E4DA"), 0f, 0.15f);

        /// <summary>Wählscheibentelefon mit Hörer.</summary>
        static void Phone(Transform parent, Vector3 pos, Material m)
        {
            var g = BlockoutKit.Group(parent, "Phone", pos, 10f);
            LookDevKit.Box(g, "Base", new Vector3(0f, 0.05f, 0f), new Vector3(0.2f, 0.1f, 0.22f), m, new Vector3(-10f, 0f, 0f));
            LookDevKit.Cyl(g, "Dial", new Vector3(0f, 0.1f, -0.03f), 0.06f, 0.01f, L.Chrome, new Vector3(-10f, 0f, 0f), true, 16);
            LookDevKit.Box(g, "Handset", new Vector3(0f, 0.15f, 0.05f), new Vector3(0.24f, 0.04f, 0.05f), m);
            foreach (var sx in new[] { -1f, 1f })
                LookDevKit.Box(g, "Cup", new Vector3(sx * 0.11f, 0.14f, 0.05f), new Vector3(0.06f, 0.05f, 0.07f), m);
        }

        /// <summary>Wandschild mit Platte (Größe passt sich dem Text an), Text aus der JSON.</summary>
        static void WallSign(Transform parent, string key, Vector3 pos, float yaw, float size, Material plate, Color color)
        {
            var g = BlockoutKit.Group(parent, "WallSign", pos, yaw);
            var s = BlockoutKit.Sign(g, "Text", key, Vector3.zero, size, color, plate, false);
            s.transform.localPosition = new Vector3(0f, 0f, -0.005f);
            foreach (var r in s.GetComponentsInChildren<MeshRenderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
