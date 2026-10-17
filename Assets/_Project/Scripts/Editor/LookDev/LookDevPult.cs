using UnityEngine;
using L = PlanErfuellt.EditorTools.LookDevMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Pult nach Bild 08 (Ego-Sicht): links drei Glastanks mit Schläuchen zur Kammer, Feld „MISCHUNG“ mit drei
    /// leuchtenden Schiebern, Statustafel aus Blech/Papier, NOT-STOPP mit Klappdeckel und Warnstreifen,
    /// drei Kipphebel GAS / KLON / FÖRDERBAND. Abgenutzter Stahl, Kantenleisten, Schrauben.
    /// Vorderseite zeigt nach -Z (zum Spieler). Maße sind Modellierdaten.
    /// </summary>
    public static class LookDevPult
    {
        const float W = 4.8f, D = 1.25f, H = 0.86f;

        /// <summary>Baut das Pult und gibt die Hebel-Drehpunkte (GAS, KLON, FÖRDERBAND) zurück.</summary>
        public static Transform[] Build(Transform v, float chamberDistance)
        {
            var root = BlockoutKit.Group(v, "Console", Vector3.zero);

            // ---------- Korpus ----------
            LookDevKit.Box(root, "Body", new Vector3(0f, H * 0.5f, 0f), new Vector3(W, H, D), L.SteelPanel);
            LookDevKit.Box(root, "Top", new Vector3(0f, H + 0.03f, 0.02f), new Vector3(W + 0.06f, 0.06f, D + 0.06f), L.SteelDark);
            LookDevKit.Box(root, "Kick", new Vector3(0f, 0.06f, -D * 0.5f - 0.005f), new Vector3(W, 0.12f, 0.02f), L.Hazard);
            // Kantenleisten mit Schrauben
            foreach (float x in new[] { -W * 0.5f, -0.95f, 0.95f, W * 0.5f })
            {
                LookDevKit.Box(root, "Post", new Vector3(x, H * 0.5f, -D * 0.5f - 0.02f), new Vector3(0.07f, H, 0.04f), L.SteelDark);
                LookDevKit.Bolts(root, new Vector3(x, 0.15f, -D * 0.5f - 0.045f), new Vector3(x, H - 0.1f, -D * 0.5f - 0.045f), 5, Vector3.back, L.SteelDark);
            }
            LookDevKit.Box(root, "FrontLip", new Vector3(0f, H - 0.03f, -D * 0.5f - 0.03f), new Vector3(W + 0.06f, 0.07f, 0.06f), L.SteelDark);
            LookDevKit.Bolts(root, new Vector3(-W * 0.5f + 0.1f, H - 0.03f, -D * 0.5f - 0.065f), new Vector3(W * 0.5f - 0.1f, H - 0.03f, -D * 0.5f - 0.065f), 18, Vector3.back, L.Brass);
            // Frontklappen mit Nähten und Lüftungsschlitzen
            for (int i = 0; i < 4; i++)
            {
                float x = -1.65f + i * 1.1f;
                LookDevKit.Box(root, "Hatch", new Vector3(x, 0.45f, -D * 0.5f - 0.012f), new Vector3(0.95f, 0.5f, 0.02f), L.SteelPanel);
                for (int s = 0; s < 6; s++)
                    LookDevKit.Box(root, "Vent", new Vector3(x, 0.32f + s * 0.045f, -D * 0.5f - 0.025f), new Vector3(0.5f, 0.012f, 0.012f), L.SteelDark);
                LookDevKit.Bolts(root, new Vector3(x - 0.42f, 0.67f, -D * 0.5f - 0.024f), new Vector3(x + 0.42f, 0.67f, -D * 0.5f - 0.024f), 2, Vector3.back, L.SteelDark);
                LookDevKit.Bolts(root, new Vector3(x - 0.42f, 0.23f, -D * 0.5f - 0.024f), new Vector3(x + 0.42f, 0.23f, -D * 0.5f - 0.024f), 2, Vector3.back, L.SteelDark);
            }

            BuildTanks(root, chamberDistance);
            BuildMixingPanel(root);
            BuildStatusBoard(root);
            BuildEmergencyStop(root);
            return BuildLevers(root);
        }

        // ---------- Tanks A/B/C mit Schläuchen ----------
        static void BuildTanks(Transform root, float chamberDistance)
        {
            var g = BlockoutKit.Group(root, "Tanks", new Vector3(-1.75f, H + 0.06f, 0.1f));
            LookDevKit.Box(g, "Shelf", new Vector3(0f, 0.04f, 0f), new Vector3(1.35f, 0.08f, 0.7f), L.SteelDark);
            var liquids = new[] { LabMaterials.LiquidA, LabMaterials.LiquidB, LabMaterials.LiquidC };
            var hoses = new[] { L.HoseGreen, L.HoseBlue, L.HoseYellow };
            var icons = new[] { L.IconLeaf, L.IconDrop, L.IconMolecule };
            string[] labels = { "ui.signs.tankA", "ui.signs.tankB", "ui.signs.tankC" };
            for (int i = 0; i < 3; i++)
            {
                float x = -0.44f + i * 0.44f;
                var t = BlockoutKit.Group(g, "Tank_" + "ABC"[i], new Vector3(x, 0.08f, 0f));
                LookDevKit.Cyl(t, "Foot", new Vector3(0f, 0.06f, 0f), 0.2f, 0.12f, L.SteelDark);
                LookDevKit.BoltCircle(t, 0.17f, 0.125f, 8, L.Brass, true, 0.012f);
                var glass = LookDevKit.Cyl(t, "Glass", new Vector3(0f, 0.43f, 0f), 0.19f, 0.62f, L.GlassTank, null, true, 32);
                glass.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                LookDevKit.Cyl(t, "Liquid", new Vector3(0f, 0.41f, 0f), 0.17f, 0.56f, liquids[i], null, false, 32);
                LookDevKit.Cyl(t, "Cap", new Vector3(0f, 0.79f, 0f), 0.205f, 0.12f, L.SteelPanel);
                LookDevKit.BoltCircle(t, 0.175f, 0.855f, 8, L.Brass, true, 0.012f);
                LookDevKit.Cyl(t, "Valve", new Vector3(0f, 0.9f, 0f), 0.06f, 0.12f, L.Brass);
                foreach (float a in new[] { 0f, 120f, 240f })
                {
                    var dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                    LookDevKit.Cyl(t, "Rod", dir * 0.2f + new Vector3(0f, 0.43f, 0f), 0.012f, 0.66f, L.SteelDark);
                }
                // Etikett mit Symbol
                LookDevKit.Plane(t, "LabelPlate", new Vector3(0f, 0.43f, -0.196f), new Vector2(0.27f, 0.3f), L.CreamPlate);
                LookDevKit.Label(t, "Label", labels[i], new Vector3(0f, 0.49f, -0.2f), 0.55f, new Color(0.08f, 0.08f, 0.08f));
                LookDevKit.Plane(t, "Icon", new Vector3(0f, 0.36f, -0.199f), new Vector2(0.09f, 0.09f), icons[i]);
                // Schlauch: vom Ventil hoch, nach hinten, runter zum Boden Richtung Kammer
                var start = new Vector3(0f, 0.95f, 0f);
                var end = new Vector3(0.55f - i * 0.12f, -0.9f, chamberDistance - 2.9f);
                var mesh = LookDevKit.Tube("PultHose" + i, start, start + new Vector3(0f, 0.55f + i * 0.08f, 0f),
                    end + new Vector3(0f, 1.6f, -0.8f), end, 0.035f, 32, 10);
                LookDevKit.Obj(t, "Hose", mesh, Vector3.zero, hoses[i]);
            }
            // Rohrverteiler hinter den Tanks
            LookDevKit.Cyl(g, "Manifold", new Vector3(0f, 0.25f, 0.3f), 0.06f, 1.4f, L.SteelDark, new Vector3(0f, 0f, 90f));
        }

        // ---------- MISCHUNG: drei leuchtende Schieber ----------
        static void BuildMixingPanel(Transform root)
        {
            var g = BlockoutKit.Group(root, "MixPanel", new Vector3(-0.55f, H + 0.06f, -0.05f));
            g.localEulerAngles = new Vector3(-22f, 0f, 0f);
            LookDevKit.Box(g, "Housing", new Vector3(0f, 0.22f, 0f), new Vector3(0.75f, 0.44f, 0.5f), L.SteelPanel);
            LookDevKit.Box(g, "Face", new Vector3(0f, 0.445f, 0f), new Vector3(0.7f, 0.02f, 0.46f), L.SteelDark);
            LookDevKit.Label(g, "Title", "ui.signs.mixLabel", new Vector3(0f, 0.46f, 0.17f), 0.6f, LabMaterials.CreamText, new Vector3(90f, 0f, 0f));
            var bars = new[] { L.BarGreen, L.BarBlue, L.BarYellow };
            float[] fill = { 0.75f, 0.45f, 0.6f };
            for (int i = 0; i < 3; i++)
            {
                float x = -0.2f + i * 0.2f;
                LookDevKit.Box(g, "Slot", new Vector3(x, 0.455f, 0f), new Vector3(0.09f, 0.01f, 0.3f), L.Rubber);
                float len = 0.26f * fill[i];
                var bar = LookDevKit.Box(g, "Bar", new Vector3(x, 0.462f, -0.13f + len * 0.5f), new Vector3(0.05f, 0.006f, len), bars[i], null, false);
                bar.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                LookDevKit.Box(g, "Knob", new Vector3(x, 0.48f, -0.13f + len), new Vector3(0.12f, 0.04f, 0.05f), L.SteelDark, null, false);
            }
            LookDevKit.Bolts(g, new Vector3(-0.32f, 0.457f, -0.2f), new Vector3(0.32f, 0.457f, -0.2f), 4, Vector3.up, L.Brass, 0.01f);
            LookDevKit.Bolts(g, new Vector3(-0.32f, 0.457f, 0.2f), new Vector3(0.32f, 0.457f, 0.2f), 4, Vector3.up, L.Brass, 0.01f);
        }

        // ---------- Statustafel ----------
        static void BuildStatusBoard(Transform root)
        {
            var g = BlockoutKit.Group(root, "StatusBoard", new Vector3(0.22f, H + 0.06f, 0.05f));
            g.localEulerAngles = new Vector3(-28f, 0f, 0f);
            LookDevKit.Box(g, "Frame", new Vector3(0f, 0.3f, 0.02f), new Vector3(0.62f, 0.6f, 0.05f), L.SteelDark);
            LookDevKit.Plane(g, "Paper", new Vector3(0f, 0.3f, -0.007f), new Vector2(0.52f, 0.52f), L.Checklist);
            LookDevKit.Label(g, "Title", "ui.signs.statusTitle", new Vector3(0f, 0.5f, -0.01f), 0.55f, new Color(0.12f, 0.1f, 0.08f));
            string[] items = { "ui.pult.statusGas", "ui.pult.statusTemp", "ui.pult.statusBelt", "ui.pult.statusVitals" };
            for (int i = 0; i < 4; i++)
                LookDevKit.Label(g, "Item" + i, items[i], new Vector3(0.07f, 0.405f - i * 0.0855f, -0.01f), 0.42f, new Color(0.12f, 0.1f, 0.08f), null, TMPro.TextAlignmentOptions.Left);
            LookDevKit.Bolts(g, new Vector3(-0.28f, 0.58f, -0.006f), new Vector3(0.28f, 0.58f, -0.006f), 2, Vector3.back, L.Brass, 0.01f);
            LookDevKit.Bolts(g, new Vector3(-0.28f, 0.02f, -0.006f), new Vector3(0.28f, 0.02f, -0.006f), 2, Vector3.back, L.Brass, 0.01f);
        }

        // ---------- NOT-STOPP ----------
        static void BuildEmergencyStop(Transform root)
        {
            var g = BlockoutKit.Group(root, "EmergencyStop", new Vector3(0.95f, H + 0.06f, -0.12f));
            LookDevKit.Box(g, "Base", new Vector3(0f, 0.05f, 0f), new Vector3(0.48f, 0.1f, 0.48f), L.SteelDark);
            LookDevKit.Box(g, "Stripes", new Vector3(0f, 0.102f, 0f), new Vector3(0.44f, 0.005f, 0.44f), L.Hazard);
            LookDevKit.Cyl(g, "Collar", new Vector3(0f, 0.13f, 0f), 0.13f, 0.06f, L.SteelPanel);
            var btn = LookDevKit.Cyl(g, "Button", new Vector3(0f, 0.18f, 0f), 0.11f, 0.06f, LabMaterials.StopButton, null, false, 32);
            BlockoutKit.Sphere(g, "Dome", new Vector3(0f, 0.2f, 0f), 0.11f, LabMaterials.StopButton, false).transform.localScale = new Vector3(0.22f, 0.1f, 0.22f);
            // Klappdeckel: hinten angeschlagen, offen
            var hinge = BlockoutKit.Group(g, "CoverHinge", new Vector3(0f, 0.11f, 0.2f));
            hinge.localEulerAngles = new Vector3(-105f, 0f, 0f);
            var cover = LookDevKit.Box(hinge, "Cover", new Vector3(0f, 0.11f, -0.2f), new Vector3(0.4f, 0.2f, 0.4f), L.CoverRed, null, false);
            cover.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LookDevKit.Cyl(hinge, "HingePin", Vector3.zero, 0.015f, 0.42f, L.SteelDark, new Vector3(0f, 0f, 90f));
            // Schild NOT-STOPP vorn
            LookDevKit.Box(root, "StopPlate", new Vector3(0.95f, H - 0.12f, -D * 0.5f - 0.06f), new Vector3(0.46f, 0.13f, 0.02f), L.RedPaint);
            LookDevKit.Label(root, "StopLabel", "ui.signs.notStop", new Vector3(0.95f, H - 0.12f, -D * 0.5f - 0.072f), 0.7f, LabMaterials.CreamText);
        }

        // ---------- Kipphebel ----------
        static Transform[] BuildLevers(Transform root)
        {
            var g = BlockoutKit.Group(root, "LeverBank", new Vector3(1.85f, H + 0.06f, 0.02f));
            LookDevKit.Box(g, "Housing", new Vector3(0f, 0.12f, 0f), new Vector3(1.0f, 0.24f, 0.7f), L.SteelPanel);
            LookDevKit.Box(g, "Header", new Vector3(0f, 0.36f, 0.33f), new Vector3(1.0f, 0.24f, 0.04f), L.SteelDark);
            string[] keys = { "ui.signs.leverGas", "ui.signs.leverClone", "ui.signs.leverBelt" };
            var handles = new[] { LabMaterials.LampGreen, L.Rubber, LabMaterials.StopButton };
            var pivots = new Transform[3];
            string[] names = { "Lever_Gas", "Lever_Clone", "Lever_Belt" };
            for (int i = 0; i < 3; i++)
            {
                float x = -0.32f + i * 0.32f;
                LookDevKit.Label(g, "Name" + i, keys[i], new Vector3(x, 0.38f, 0.305f), 0.5f, LabMaterials.CreamText);
                LookDevKit.Box(g, "Divider", new Vector3(x + 0.16f, 0.36f, 0.305f), new Vector3(0.01f, 0.2f, 0.01f), L.SteelPanel);
                LookDevKit.Box(g, "Slot", new Vector3(x, 0.243f, -0.02f), new Vector3(0.08f, 0.01f, 0.32f), L.Rubber);
                LookDevKit.Box(g, "Plate", new Vector3(x, 0.244f, -0.02f), new Vector3(0.2f, 0.006f, 0.4f), L.SteelDark);
                LookDevKit.Bolts(g, new Vector3(x - 0.08f, 0.25f, -0.2f), new Vector3(x + 0.08f, 0.25f, -0.2f), 2, Vector3.up, L.Brass, 0.01f);
                LookDevKit.Bolts(g, new Vector3(x - 0.08f, 0.25f, 0.16f), new Vector3(x + 0.08f, 0.25f, 0.16f), 2, Vector3.up, L.Brass, 0.01f);
                var piv = BlockoutKit.Group(g, names[i], new Vector3(x, 0.25f, -0.02f));
                piv.localEulerAngles = new Vector3(20f, 0f, 0f);
                LookDevKit.Box(piv, "Stick", new Vector3(0f, 0.15f, 0f), new Vector3(0.045f, 0.3f, 0.045f), L.SteelPanel, null, false);
                LookDevKit.Box(piv, "Handle", new Vector3(0f, 0.33f, 0f), new Vector3(0.09f, 0.13f, 0.09f), handles[i], null, false);
                pivots[i] = piv;
            }
            return pivots;
        }
    }
}
