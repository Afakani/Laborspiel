using UnityEngine;
using UnityEngine.Rendering;
using L = PlanErfuellt.EditorTools.LookDevMaterials;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Maschinen im Look von Bild 07/08 (abgenutzter Stahl, Lack, Messing, Schrauben, Anzeigen, Lampen).
    /// Grundflächen wie in layout.stations, damit Kollider und Zonen gleich bleiben. Vorderseite zeigt nach −Z.
    /// Bewegliche Teile tragen feste Namen (Rotor, Record, Crank, StampArm, Shaft), damit spätere Phasen sie animieren können.
    /// </summary>
    public static class LookDevMachines
    {
        // ---------- Verarbeitung (2,3 × 2,6 × 2,6) ----------

        public static void Processing(Transform v)
        {
            // Gehäuse mit Rippen, Einlass zum Band (−X)
            LookDevKit.Box(v, "Body", new Vector3(0.1f, 1.25f, 0f), new Vector3(2.1f, 2.5f, 2.5f), L.SteelPanel);
            LookDevKit.Box(v, "Plinth", new Vector3(0.1f, 0.08f, 0f), new Vector3(2.3f, 0.16f, 2.6f), L.SteelDark);
            foreach (float z in new[] { -1.26f, 1.26f })
            for (int i = 0; i < 4; i++)
                LookDevKit.Box(v, "Rib", new Vector3(-0.75f + i * 0.55f, 1.3f, z), new Vector3(0.1f, 2.3f, 0.06f), L.SteelDark);
            LookDevKit.Bolts(v, new Vector3(-0.9f, 2.45f, -1.27f), new Vector3(1.1f, 2.45f, -1.27f), 12, Vector3.back, L.Brass, 0.018f);
            LookDevKit.Bolts(v, new Vector3(-0.9f, 0.25f, -1.27f), new Vector3(1.1f, 0.25f, -1.27f), 12, Vector3.back, L.Brass, 0.018f);

            // Einlass mit Warnrahmen und Gummilamellen
            var hole = BlockoutKit.Quad(v, "Hole", new Vector3(-0.96f, 1.0f, 0f), new Vector2(1.7f, 1.4f), M.Hole, true, new Vector3(0f, 90f, 0f));
            hole.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            LookDevKit.Box(v, "FrameTop", new Vector3(-1.0f, 1.8f, 0f), new Vector3(0.12f, 0.2f, 2.0f), L.Hazard);
            foreach (float z in new[] { -0.95f, 0.95f })
                LookDevKit.Box(v, "FrameSide", new Vector3(-1.0f, 0.95f, z), new Vector3(0.12f, 1.7f, 0.12f), L.Hazard);
            for (int i = 0; i < 7; i++)
                LookDevKit.Box(v, "Flap", new Vector3(-1.02f, 1.3f, -0.75f + i * 0.25f), new Vector3(0.02f, 0.9f, 0.22f), L.Rubber, null, false);

            // Trichter mit grünem Bad oben
            LookDevKit.Obj(v, "Hopper", LookDevKit.Lathe("ProcHopper", new[] { new Vector2(0.45f, 0f), new Vector2(0.85f, 0.45f), new Vector2(0.9f, 0.6f) }, 32, true),
                new Vector3(0.2f, 2.5f, 0.1f), L.SteelDark);
            LookDevKit.Cyl(v, "Vat", new Vector3(0.2f, 2.95f, 0.1f), 0.82f, 0.12f, M.VatGreen, null, false, 32);
            BlockoutKit.MeshObj(v, "VatRim", MeshFactory.Torus(0.88f, 0.05f), new Vector3(0.2f, 3.1f, 0.1f), L.Brass);

            // Abgasrohr, Leitungen, Anzeige, Lampe, Schild
            LookDevKit.Cyl(v, "Stack", new Vector3(0.85f, 3.4f, 0.8f), 0.16f, 1.8f, L.SteelDark, null, true, 16);
            LookDevKit.Cyl(v, "StackCap", new Vector3(0.85f, 4.32f, 0.8f), 0.22f, 0.06f, L.SteelFrame, null, true, 16);
            LookDevKit.Obj(v, "Pipe", LookDevKit.Tube("ProcPipe", new Vector3(1.15f, 2.0f, -0.6f), new Vector3(1.6f, 2.0f, -0.6f), new Vector3(1.6f, 0.4f, -0.6f), new Vector3(1.2f, 0.3f, -0.6f), 0.07f), Vector3.zero, L.GreenPaint);
            Dial(v, new Vector3(0.6f, 1.9f, -1.29f), 0.16f);
            Dial(v, new Vector3(0.95f, 1.9f, -1.29f), 0.12f);
            WarnLamp(v, new Vector3(-0.6f, 2.6f, -1.15f), M.LampAmber);
            LookDevKit.Box(v, "HazardBand", new Vector3(0.1f, 0.35f, -1.26f), new Vector3(2.1f, 0.2f, 0.02f), L.Hazard);
        }

        // ---------- Bandmotor (0,9 × 0,8 × 0,7) ----------

        public static void BeltMotor(Transform v)
        {
            LookDevKit.Box(v, "Base", new Vector3(0f, 0.05f, 0f), new Vector3(0.9f, 0.1f, 0.7f), L.SteelDark);
            LookDevKit.Bolts(v, new Vector3(-0.38f, 0.105f, -0.28f), new Vector3(0.38f, 0.105f, -0.28f), 2, Vector3.up, L.Brass, 0.02f);
            LookDevKit.Bolts(v, new Vector3(-0.38f, 0.105f, 0.28f), new Vector3(0.38f, 0.105f, 0.28f), 2, Vector3.up, L.Brass, 0.02f);
            // Motor liegt quer (Achse Z), Kühlrippen
            LookDevKit.Cyl(v, "Housing", new Vector3(0f, 0.42f, -0.02f), 0.28f, 0.5f, L.GreenPaint, new Vector3(90f, 0f, 0f), true, 32);
            for (int i = 0; i < 6; i++)
                LookDevKit.Cyl(v, "Fin", new Vector3(0f, 0.42f, -0.22f + i * 0.08f), 0.31f, 0.025f, L.GreenPaint, new Vector3(90f, 0f, 0f), true, 32);
            LookDevKit.Cyl(v, "EndBell", new Vector3(0f, 0.42f, -0.29f), 0.24f, 0.06f, L.SteelDark, new Vector3(90f, 0f, 0f), true, 24);
            LookDevKit.Box(v, "Feet", new Vector3(0f, 0.17f, -0.02f), new Vector3(0.5f, 0.14f, 0.45f), L.GreenPaint);
            LookDevKit.Box(v, "Terminal", new Vector3(0f, 0.74f, -0.02f), new Vector3(0.2f, 0.12f, 0.2f), L.SteelDark);
            // Antrieb: Welle, Riemenscheibe, Schutzhaube
            var shaft = BlockoutKit.Group(v, "Shaft", new Vector3(0f, 0.42f, 0.3f));
            LookDevKit.Cyl(shaft, "Axle", new Vector3(0f, 0f, 0.02f), 0.04f, 0.14f, L.Chrome, new Vector3(90f, 0f, 0f), false, 12);
            LookDevKit.Cyl(shaft, "Pulley", new Vector3(0f, 0f, 0.08f), 0.17f, 0.06f, L.SteelPanel, new Vector3(90f, 0f, 0f), false, 24);
            LookDevKit.Box(v, "Guard", new Vector3(0f, 0.42f, 0.33f), new Vector3(0.42f, 0.42f, 0.04f), L.Hazard);
            WarnLamp(v, new Vector3(0.2f, 0.85f, -0.02f), M.LampGreen);
        }

        // ---------- Mikroskop (auf der Mischstation) ----------

        public static void Microscope(Transform v)
        {
            LookDevKit.Box(v, "Base", new Vector3(0f, 0.02f, 0.02f), new Vector3(0.24f, 0.04f, 0.3f), L.BlackPaint);
            LookDevKit.Obj(v, "Arm", LookDevKit.Tube("MicroArm", new Vector3(0f, 0.04f, 0.1f), new Vector3(0f, 0.25f, 0.16f), new Vector3(0f, 0.38f, 0.12f), new Vector3(0f, 0.42f, 0.04f), 0.03f, 12, 8), Vector3.zero, L.BlackPaint);
            LookDevKit.Box(v, "Stage", new Vector3(0f, 0.16f, -0.01f), new Vector3(0.18f, 0.015f, 0.16f), L.BlackPaint);
            LookDevKit.Cyl(v, "Dish", new Vector3(0f, 0.175f, -0.02f), 0.05f, 0.01f, M.CryoWindow, null, false, 16);
            LookDevKit.Cyl(v, "Tube", new Vector3(0f, 0.36f, 0.0f), 0.032f, 0.26f, L.Brass, new Vector3(-20f, 0f, 0f), true, 16);
            LookDevKit.Cyl(v, "Eyepiece", new Vector3(0f, 0.5f, 0.05f), 0.022f, 0.08f, L.BlackPaint, new Vector3(-20f, 0f, 0f), true, 12);
            LookDevKit.Cyl(v, "Objective", new Vector3(0f, 0.23f, -0.03f), 0.018f, 0.06f, L.Chrome, null, true, 12);
            LookDevKit.Cyl(v, "Knob", new Vector3(0.05f, 0.28f, 0.13f), 0.035f, 0.02f, L.Brass, new Vector3(0f, 0f, 90f), true, 12);
        }

        // ---------- Zentrifuge (1,7 × 1,8 × 1,7) ----------

        public static void Centrifuge(Transform v)
        {
            // Trommel mit Emaille, Stahlbändern, Füßen
            LookDevKit.Obj(v, "Body", LookDevKit.Lathe("CentBody", new[] { new Vector2(0.72f, 0.12f), new Vector2(0.8f, 0.2f), new Vector2(0.82f, 0.5f), new Vector2(0.8f, 0.86f), new Vector2(0.74f, 0.92f) }, 48),
                Vector3.zero, L.EnamelCream);
            foreach (float y in new[] { 0.28f, 0.78f })
                BlockoutKit.MeshObj(v, "Band", MeshFactory.Torus(0.825f, 0.03f), new Vector3(0f, y, 0f), L.SteelDark);
            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                LookDevKit.Box(v, "Foot", new Vector3(Mathf.Cos(a) * 0.62f, 0.06f, Mathf.Sin(a) * 0.62f), new Vector3(0.18f, 0.12f, 0.18f), L.SteelDark);
            }
            LookDevKit.Cyl(v, "Rim", new Vector3(0f, 0.94f, 0f), 0.8f, 0.05f, L.SteelPanel, null, true, 48);
            LookDevKit.BoltCircle(v, 0.76f, 0.97f, 16, L.Brass, true, 0.018f);

            // Rotor mit Röhrchen (dreht sich später)
            var rotor = BlockoutKit.Group(v, "Rotor", new Vector3(0f, 0.98f, 0f));
            LookDevKit.Cyl(rotor, "Hub", new Vector3(0f, 0.08f, 0f), 0.12f, 0.16f, L.Chrome, null, false, 24);
            var tubeMats = new[] { M.LiquidA, M.LiquidB, M.LiquidC, M.LiquidA, M.LiquidB, M.LiquidC };
            for (int i = 0; i < 6; i++)
            {
                var arm = BlockoutKit.Group(rotor, "Arm", Vector3.zero, i * 60f);
                LookDevKit.Box(arm, "Spoke", new Vector3(0.25f, 0.06f, 0f), new Vector3(0.36f, 0.04f, 0.06f), L.SteelPanel, null, false);
                LookDevKit.Cyl(arm, "Holder", new Vector3(0.47f, 0.18f, 0f), 0.065f, 0.3f, L.Chrome, new Vector3(0f, 0f, 29f), false, 16);
                BlockoutKit.Cyl(arm, "Tube", new Vector3(0.47f, 0.2f, 0f), 0.05f, 0.4f, tubeMats[i], false, new Vector3(0f, 0f, 29f));
            }

            // Glaskuppel mit Scharnier und Griff
            var dome = LookDevKit.Obj(v, "Dome", LookDevKit.Lathe("CentDome", new[] { new Vector2(0.8f, 0f), new Vector2(0.78f, 0.18f), new Vector2(0.66f, 0.42f), new Vector2(0.42f, 0.6f), new Vector2(0.0f, 0.68f) }, 48, true),
                new Vector3(0f, 0.96f, 0f), L.GlassClear, null, true, false);
            dome.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            LookDevKit.Box(v, "Hinge", new Vector3(0f, 1.0f, 0.8f), new Vector3(0.3f, 0.08f, 0.08f), L.SteelDark);
            LookDevKit.Cyl(v, "Handle", new Vector3(0f, 1.62f, 0f), 0.05f, 0.06f, L.Chrome, null, true, 16);

            // Bedienfeld vorn
            var panel = BlockoutKit.Group(v, "ControlPanel", new Vector3(0f, 0.55f, -0.8f));
            LookDevKit.Box(panel, "Plate", new Vector3(0f, 0f, -0.02f), new Vector3(0.55f, 0.32f, 0.04f), L.SteelDark);
            Dial(panel, new Vector3(-0.14f, 0.03f, -0.045f), 0.09f);
            Knob(panel, new Vector3(0.08f, 0.05f, -0.05f));
            Knob(panel, new Vector3(0.18f, 0.05f, -0.05f));
            WarnLamp(panel, new Vector3(0.13f, -0.09f, -0.05f), M.LampAmber, 0.04f);
            LookDevKit.Bolts(panel, new Vector3(-0.25f, 0.13f, -0.045f), new Vector3(0.25f, 0.13f, -0.045f), 2, Vector3.back, L.Brass, 0.012f);
        }

        // ---------- Kryokammer (1,3 × 1,9 × 1,1) ----------

        public static void Cryo(Transform v)
        {
            LookDevKit.Box(v, "Cabinet", new Vector3(0f, 0.97f, 0.03f), new Vector3(1.24f, 1.82f, 1.04f), L.SteelPanel);
            LookDevKit.Box(v, "Plinth", new Vector3(0f, 0.05f, 0f), new Vector3(1.3f, 0.1f, 1.1f), L.SteelDark);
            LookDevKit.Box(v, "Top", new Vector3(0f, 1.9f, 0.03f), new Vector3(1.3f, 0.06f, 1.1f), L.SteelDark);
            // Tür mit Bullaugen-Fenster, Rahmen, Riegeln
            var door = BlockoutKit.Group(v, "Door", new Vector3(0f, 0.98f, -0.5f));
            LookDevKit.Box(door, "Panel", new Vector3(0f, 0f, -0.02f), new Vector3(1.1f, 1.6f, 0.05f), L.SteelFrame);
            LookDevKit.Obj(door, "WindowRing", LookDevKit.Ring(0.3f, 0.36f, 0.06f, 40), new Vector3(0f, 0.18f, -0.06f), L.Brass, new Vector3(90f, 0f, 0f));
            LookDevKit.Cyl(door, "Window", new Vector3(0f, 0.18f, -0.075f), 0.3f, 0.01f, L.GlassBlue, new Vector3(90f, 0f, 0f), true, 40);
            LookDevKit.Cyl(door, "Inside", new Vector3(0f, 0.18f, -0.05f), 0.29f, 0.01f, L.CryoGlow, new Vector3(90f, 0f, 0f), true, 40);
            for (int i = 0; i < 12; i++)
            {
                float a = i / 12f * Mathf.PI * 2f;
                LookDevKit.Cyl(door, "Bolt", new Vector3(Mathf.Cos(a) * 0.33f, 0.18f + Mathf.Sin(a) * 0.33f, -0.095f), 0.016f, 0.02f, L.Chrome, new Vector3(90f, 0f, 0f), true, 8);
            }
            foreach (float y in new[] { 0.45f, -0.45f })
            {
                LookDevKit.Box(door, "Latch", new Vector3(0.42f, y, -0.08f), new Vector3(0.18f, 0.06f, 0.06f), L.Chrome);
                LookDevKit.Cyl(door, "LatchPin", new Vector3(0.5f, y, -0.06f), 0.03f, 0.1f, L.SteelDark, null, true, 12);
            }
            LookDevKit.Box(door, "Handle", new Vector3(-0.4f, -0.1f, -0.1f), new Vector3(0.05f, 0.45f, 0.05f), L.Chrome);
            // Frost am unteren Rand, Stickstoff-Flasche, Leitungen, Anzeige, Lampe
            LookDevKit.Box(v, "Frost", new Vector3(0f, 0.2f, -0.555f), new Vector3(1.18f, 0.14f, 0.03f), L.Frost);
            LookDevKit.Obj(v, "Bottle", LookDevKit.Lathe("CryoBottle", new[] { new Vector2(0f, 0f), new Vector2(0.17f, 0f), new Vector2(0.18f, 0.05f), new Vector2(0.18f, 1.25f), new Vector2(0.12f, 1.42f), new Vector2(0.05f, 1.48f), new Vector2(0.05f, 1.56f), new Vector2(0f, 1.56f) }, 24),
                new Vector3(0.82f, 0f, 0.2f), L.WhitePaint);
            LookDevKit.Cyl(v, "BottleBand", new Vector3(0.82f, 1.05f, 0.2f), 0.185f, 0.1f, L.BarBlue, null, true, 24);
            LookDevKit.Obj(v, "Hose", LookDevKit.Tube("CryoHose", new Vector3(0.82f, 1.56f, 0.2f), new Vector3(0.82f, 1.9f, 0.2f), new Vector3(0.6f, 1.8f, 0.2f), new Vector3(0.62f, 1.4f, 0.3f), 0.025f, 16, 8), Vector3.zero, L.Rubber);
            LookDevKit.Cyl(v, "Pipe", new Vector3(-0.4f, 2.6f, 0.3f), 0.1f, 1.4f, L.Frost, null, true, 16);
            foreach (float y in new[] { 2.05f, 2.9f })
                LookDevKit.Cyl(v, "Flange", new Vector3(-0.4f, y, 0.3f), 0.14f, 0.06f, L.SteelDark, null, true, 16);
            Dial(v, new Vector3(0.38f, 1.7f, -0.535f), 0.08f);
            WarnLamp(v, new Vector3(-0.38f, 1.72f, -0.52f), M.LampBlue);
        }

        // ---------- Verkostungsautomat (0,9 × 1,7 × 0,7) ----------

        public static void Tasting(Transform v)
        {
            LookDevKit.Box(v, "Body", new Vector3(0f, 0.85f, 0.02f), new Vector3(0.86f, 1.62f, 0.66f), L.RedPaint);
            LookDevKit.Box(v, "Plinth", new Vector3(0f, 0.04f, 0.02f), new Vector3(0.9f, 0.08f, 0.7f), L.SteelDark);
            LookDevKit.Box(v, "Crown", new Vector3(0f, 1.69f, 0.02f), new Vector3(0.9f, 0.08f, 0.7f), L.Chrome);
            foreach (float x in new[] { -0.43f, 0.43f })
                LookDevKit.Box(v, "Trim", new Vector3(x, 0.85f, -0.31f), new Vector3(0.03f, 1.62f, 0.03f), L.Chrome);
            BlockoutKit.Sign(v, "Plate", "ui.signs.tastingPlate", new Vector3(0f, 1.4f, -0.33f), 0.9f, M.Hex("#B3121B"), M.Cream, false);
            // Mund mit Lippen aus Messing, Ausgabefach
            LookDevKit.Box(v, "MouthFrame", new Vector3(0f, 1.05f, -0.315f), new Vector3(0.5f, 0.16f, 0.03f), L.Brass);
            BlockoutKit.Box(v, "Mouth", new Vector3(0f, 1.05f, -0.33f), new Vector3(0.4f, 0.07f, 0.01f), M.Hole);
            LookDevKit.Box(v, "Tray", new Vector3(0f, 0.45f, -0.36f), new Vector3(0.5f, 0.05f, 0.14f), L.Chrome);
            LookDevKit.Box(v, "TrayBack", new Vector3(0f, 0.55f, -0.32f), new Vector3(0.5f, 0.25f, 0.02f), L.SteelDark);
            // Trichter oben, Stempelarm, Lampe, Anzeige
            LookDevKit.Obj(v, "Funnel", LookDevKit.Lathe("TasteFunnel", new[] { new Vector2(0.05f, 0f), new Vector2(0.06f, 0.12f), new Vector2(0.2f, 0.32f), new Vector2(0.22f, 0.36f) }, 24, true),
                new Vector3(0f, 1.73f, 0.05f), L.Chrome);
            var arm = BlockoutKit.Group(v, "StampArm", new Vector3(0.3f, 0.75f, -0.34f));
            LookDevKit.Box(arm, "Arm", new Vector3(0f, 0f, -0.15f), new Vector3(0.06f, 0.06f, 0.3f), L.SteelDark, null, false);
            LookDevKit.Cyl(arm, "Stamp", new Vector3(0f, -0.04f, -0.3f), 0.1f, 0.08f, M.StopButton, null, false, 24);
            WarnLamp(v, new Vector3(-0.3f, 1.62f, -0.33f), M.LampAmber);
            Dial(v, new Vector3(0.26f, 1.62f, -0.335f), 0.06f);
        }

        // ---------- Grammophon (1,0 × 1,5 × 0,8) ----------

        public static void Gramophone(Transform v)
        {
            LookDevKit.Box(v, "TableTop", new Vector3(0f, 0.9f, 0f), new Vector3(1.0f, 0.06f, 0.8f), L.Wood);
            LookDevKit.Box(v, "Apron", new Vector3(0f, 0.82f, 0f), new Vector3(0.9f, 0.1f, 0.7f), L.Wood);
            foreach (var p in new[] { new Vector2(-0.42f, -0.32f), new Vector2(0.42f, -0.32f), new Vector2(-0.42f, 0.32f), new Vector2(0.42f, 0.32f) })
                LookDevKit.Obj(v, "Leg", LookDevKit.Lathe("TableLeg", new[] { new Vector2(0.025f, 0f), new Vector2(0.03f, 0.05f), new Vector2(0.022f, 0.3f), new Vector2(0.035f, 0.45f), new Vector2(0.024f, 0.6f), new Vector2(0.032f, 0.8f), new Vector2(0.032f, 0.87f) }, 16),
                    new Vector3(p.x, 0f, p.y), L.Wood);
            LookDevKit.Box(v, "Case", new Vector3(0f, 1.05f, 0f), new Vector3(0.56f, 0.24f, 0.56f), L.Wood);
            LookDevKit.Box(v, "CaseTrim", new Vector3(0f, 1.17f, 0f), new Vector3(0.58f, 0.02f, 0.58f), L.Brass);
            var record = BlockoutKit.Group(v, "Record", new Vector3(0f, 1.19f, 0f));
            LookDevKit.Cyl(record, "Disc", Vector3.zero, 0.24f, 0.012f, L.BlackPaint, null, false, 48);
            LookDevKit.Cyl(record, "Label", new Vector3(0f, 0.007f, 0f), 0.07f, 0.004f, L.RedPaint, null, false, 24);
            // Tonarm und Horn (Drehkörper, Messing)
            LookDevKit.Obj(v, "ToneArm", LookDevKit.Tube("ToneArm", new Vector3(0.24f, 1.2f, 0.2f), new Vector3(0.24f, 1.32f, 0.2f), new Vector3(0.15f, 1.3f, 0.05f), new Vector3(0.08f, 1.22f, 0.02f), 0.018f, 12, 8), Vector3.zero, L.Brass);
            var horn = LookDevKit.Obj(v, "Horn", LookDevKit.Lathe("GramHorn", new[] { new Vector2(0.03f, 0f), new Vector2(0.05f, 0.25f), new Vector2(0.1f, 0.5f), new Vector2(0.2f, 0.7f), new Vector2(0.36f, 0.82f), new Vector2(0.44f, 0.86f) }, 32, true),
                new Vector3(0.24f, 1.32f, 0.2f), L.Brass, new Vector3(55f, -40f, 0f));
            BlockoutKit.SetStatic(horn, true);
            var crank = BlockoutKit.Group(v, "Crank", new Vector3(0.29f, 1.05f, 0f));
            LookDevKit.Cyl(crank, "Axle", new Vector3(0.03f, 0f, 0f), 0.012f, 0.06f, L.Brass, new Vector3(0f, 0f, 90f), false, 8);
            LookDevKit.Box(crank, "Handle", new Vector3(0.06f, 0.06f, 0f), new Vector3(0.02f, 0.14f, 0.02f), L.Brass, null, false);
            LookDevKit.Cyl(crank, "Grip", new Vector3(0.08f, 0.12f, 0f), 0.015f, 0.05f, L.Wood, new Vector3(0f, 0f, 90f), false, 8);
        }

        // ---------- Rohrpost (0,6 × 1,6 × 0,5, Rohr bis unter die Decke) ----------

        public static void Rohrpost(Transform v)
        {
            LookDevKit.Cyl(v, "Tube", new Vector3(0f, 4.6f, 0.05f), 0.13f, 6.8f, L.Brass, null, true, 24);
            for (float y = 2.2f; y < 8f; y += 1.5f)
                LookDevKit.Cyl(v, "Collar", new Vector3(0f, y, 0.05f), 0.17f, 0.08f, L.SteelDark, null, true, 24);
            LookDevKit.Box(v, "Box", new Vector3(0f, 1.25f, 0.05f), new Vector3(0.6f, 0.75f, 0.45f), L.SteelPanel);
            LookDevKit.Box(v, "Stand", new Vector3(0f, 0.45f, 0.05f), new Vector3(0.16f, 0.9f, 0.16f), L.SteelDark);
            LookDevKit.Box(v, "Foot", new Vector3(0f, 0.04f, 0.05f), new Vector3(0.45f, 0.08f, 0.4f), L.SteelDark);
            // Runde Klappe mit Scharnier, Klinke, Anzeige
            LookDevKit.Obj(v, "HatchRing", LookDevKit.Ring(0.15f, 0.2f, 0.05f, 32), new Vector3(0f, 1.27f, -0.19f), L.Brass, new Vector3(90f, 0f, 0f));
            LookDevKit.Cyl(v, "Hatch", new Vector3(0f, 1.27f, -0.2f), 0.16f, 0.03f, L.SteelDark, new Vector3(90f, 0f, 0f), false, 32);
            LookDevKit.Box(v, "Latch", new Vector3(0.12f, 1.27f, -0.23f), new Vector3(0.08f, 0.03f, 0.03f), L.Chrome);
            LookDevKit.Bolts(v, new Vector3(-0.26f, 1.58f, -0.18f), new Vector3(0.26f, 1.58f, -0.18f), 2, Vector3.back, L.Brass, 0.014f);
            LookDevKit.Bolts(v, new Vector3(-0.26f, 0.92f, -0.18f), new Vector3(0.26f, 0.92f, -0.18f), 2, Vector3.back, L.Brass, 0.014f);
            Dial(v, new Vector3(-0.18f, 1.52f, -0.185f), 0.05f);
            WarnLamp(v, new Vector3(0.2f, 1.53f, -0.17f), M.LampGreen, 0.035f);
        }

        // ---------- Bestellterminal (1,5 × 2,2 × 1,0) ----------

        public static void Terminal(Transform v)
        {
            LookDevKit.Box(v, "Base", new Vector3(0f, 0.55f, 0.05f), new Vector3(1.46f, 1.1f, 0.9f), L.GreenPaint);
            LookDevKit.Box(v, "Kick", new Vector3(0f, 0.08f, -0.39f), new Vector3(1.46f, 0.16f, 0.02f), L.Hazard);
            LookDevKit.Box(v, "Desk", new Vector3(0f, 1.12f, -0.05f), new Vector3(1.5f, 0.05f, 1.0f), L.SteelDark);
            // Tastatur und Knöpfe
            LookDevKit.Box(v, "Keyboard", new Vector3(-0.1f, 1.17f, -0.25f), new Vector3(0.8f, 0.05f, 0.28f), L.BlackPaint, new Vector3(8f, 0f, 0f));
            for (int r = 0; r < 3; r++)
            for (int c = 0; c < 10; c++)
                LookDevKit.Box(v, "Key", new Vector3(-0.44f + c * 0.075f, 1.21f - r * 0.012f, -0.33f + r * 0.08f), new Vector3(0.06f, 0.025f, 0.06f), L.EnamelCream, new Vector3(8f, 0f, 0f));
            foreach (var (x, mat) in new[] { (0.45f, M.StopButton), (0.58f, M.LampGreen) })
                LookDevKit.Cyl(v, "Button", new Vector3(x, 1.17f, -0.28f), 0.045f, 0.04f, mat, null, true, 16);
            // Röhrenbildschirm im schrägen Gehäuse
            var head = BlockoutKit.Group(v, "Head", new Vector3(0f, 1.7f, 0.15f));
            head.localEulerAngles = new Vector3(15f, 0f, 0f);
            LookDevKit.Box(head, "Housing", new Vector3(0f, 0f, 0.1f), new Vector3(1.3f, 0.95f, 0.5f), L.GreenPaint);
            LookDevKit.Box(head, "Bezel", new Vector3(0f, 0.02f, -0.16f), new Vector3(1.0f, 0.72f, 0.04f), L.BlackPaint);
            BlockoutKit.Quad(head, "Screen", new Vector3(0f, 0.02f, -0.185f), new Vector2(0.88f, 0.6f), M.TermScreen, false);
            LookDevKit.Bolts(head, new Vector3(-0.6f, 0.42f, -0.16f), new Vector3(0.6f, 0.42f, -0.16f), 6, Vector3.back, L.Brass, 0.014f);
            // Fernschreiber-Papierrolle
            LookDevKit.Cyl(v, "PaperRoll", new Vector3(0.55f, 1.25f, 0.25f), 0.08f, 0.3f, L.EnamelCream, new Vector3(0f, 0f, 90f), true, 16);
            WarnLamp(v, new Vector3(-0.6f, 2.25f, 0.15f), M.LampAmber);
        }

        // ---------- Lager (2,6 × 1,2 × 1,8) ----------

        public static void Storage(Transform v)
        {
            LookDevKit.Box(v, "Pallet", new Vector3(0f, 0.07f, 0f), new Vector3(2.6f, 0.14f, 1.8f), L.CratePlanks);
            var lids = new[] { M.LiquidA, M.LiquidB, M.LiquidC, M.Antidote };
            string[] keys = { "ui.signs.crateA", "ui.signs.crateB", "ui.signs.crateC", "ui.signs.crateAntidote" };
            for (int i = 0; i < 8; i++)
            {
                var c = BlockoutKit.Group(v, "Crate_" + i, new Vector3(-0.92f + (i % 4) * 0.61f, 0.14f + (i / 4) * 0.52f, -0.2f + (i / 4) * 0.12f));
                LookDevKit.Box(c, "Box", new Vector3(0f, 0.25f, 0f), new Vector3(0.56f, 0.48f, 0.62f), L.CratePlanks);
                LookDevKit.Box(c, "Band", new Vector3(0f, 0.25f, 0f), new Vector3(0.58f, 0.06f, 0.64f), L.SteelDark);
                LookDevKit.Box(c, "Mark", new Vector3(0f, 0.36f, -0.32f), new Vector3(0.3f, 0.12f, 0.01f), lids[i % 4]);
                LookDevKit.Label(c, "Stencil", keys[i % 4], new Vector3(0f, 0.16f, -0.32f), i % 4 == 3 ? 1.1f : 1.6f, M.Hex("#1A1612"));
            }
            // Kanister daneben
            foreach (float x in new[] { 1.1f, 0.85f })
                LookDevKit.Obj(v, "Canister", LookDevKit.Lathe("Canister", new[] { new Vector2(0f, 0f), new Vector2(0.15f, 0f), new Vector2(0.16f, 0.04f), new Vector2(0.16f, 0.5f), new Vector2(0.1f, 0.6f), new Vector2(0.04f, 0.62f), new Vector2(0f, 0.62f) }, 20),
                    new Vector3(x, 0.14f, 0.55f), L.RedPaint);
        }

        // ---------- Kleinteile ----------

        /// <summary>Rundes Zeigerinstrument mit Messingring (Vorderseite −Z).</summary>
        static void Dial(Transform parent, Vector3 pos, float r)
        {
            var g = BlockoutKit.Group(parent, "Dial", pos);
            LookDevKit.Cyl(g, "Ring", Vector3.zero, r, 0.03f, L.Brass, new Vector3(90f, 0f, 0f), true, 24);
            LookDevKit.Cyl(g, "Face", new Vector3(0f, 0f, -0.012f), r * 0.85f, 0.01f, L.DialFace, new Vector3(90f, 0f, 0f), true, 24);
            LookDevKit.Box(g, "Needle", new Vector3(r * 0.25f, r * 0.2f, -0.02f), new Vector3(r * 0.6f, r * 0.08f, 0.004f), L.BlackPaint, new Vector3(0f, 0f, 35f));
        }

        static void Knob(Transform parent, Vector3 pos)
        {
            LookDevKit.Cyl(parent, "Knob", pos, 0.035f, 0.04f, L.BlackPaint, new Vector3(90f, 0f, 0f), true, 16);
        }

        /// <summary>Kleine Signallampe mit Gitterkorb.</summary>
        static void WarnLamp(Transform parent, Vector3 pos, Material glow, float r = 0.06f)
        {
            var g = BlockoutKit.Group(parent, "Lamp", pos);
            LookDevKit.Cyl(g, "Socket", new Vector3(0f, -r * 0.9f, 0f), r * 0.9f, r * 0.5f, L.SteelDark, null, true, 12);
            var bulb = BlockoutKit.Sphere(g, "Bulb", Vector3.zero, r, glow, false);
            bulb.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
            BlockoutKit.MeshObj(g, "Cage", MeshFactory.Torus(r * 1.1f, r * 0.12f), Vector3.zero, L.SteelDark, false, new Vector3(90f, 0f, 0f));
            BlockoutKit.MeshObj(g, "Cage", MeshFactory.Torus(r * 1.1f, r * 0.12f), Vector3.zero, L.SteelDark, false, new Vector3(90f, 90f, 0f));
        }
    }
}
