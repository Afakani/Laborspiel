using UnityEngine;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Platzhalter-Modelle der Stationen in den Proportionen des Prototyps. Vorderseite zeigt nach -Z.
    /// Kollider und Schild kommen aus StationLayout.
    /// </summary>
    public static class LabStationFactory
    {
        public static GameObject Build(StationLayout s)
        {
            var root = new GameObject("Station_" + s.id);
            var v = BlockoutKit.Group(root.transform, "Visual", Vector3.zero);

            switch (s.id)
            {
                case "clone": CloneLook(v); break;
                case "processing": LookDevMachines.Processing(v); break;
                case "pult": Pult(v, s.z); break;
                case "beltMotor": LookDevMachines.BeltMotor(v); break;
                case "mixing": Mixing(v); break;
                case "microscope": LookDevMachines.Microscope(v); break;
                case "centrifuge": LookDevMachines.Centrifuge(v); break;
                case "cryo": LookDevMachines.Cryo(v); break;
                case "tasting": LookDevMachines.Tasting(v); break;
                case "gramophone": LookDevMachines.Gramophone(v); break;
                case "rohrpost": LookDevMachines.Rohrpost(v); break;
                case "terminal": LookDevMachines.Terminal(v); break;
                case "storage": LookDevMachines.Storage(v); break;
                case "tv": Tv(v); break;
                case "angerScale": AngerScale(v); break;
                case "planBoard": PlanBoard(v); break;
                default: BlockoutKit.Box(v, "Placeholder", new Vector3(0f, s.Size.y * 0.5f, 0f), s.Size, M.SteelDark); break;
            }

            // Kollider: an der Wand hängende Teile (y > 0) sind um ihren Ursprung zentriert, sonst steht der Kollider auf dem Boden
            var col = root.AddComponent<BoxCollider>();
            col.size = s.Size;
            col.center = s.y > 0f ? Vector3.zero : new Vector3(0f, s.Size.y * 0.5f, 0f);

            AddInteraction(root, s.id);

            if (!string.IsNullOrEmpty(s.signKey))
                BlockoutKit.Sign(root.transform, "Sign", s.signKey, new Vector3(0f, s.signHeight - s.y, 0f), 3.2f, SignColor(s.id), M.SignPlate, true);
            return root;
        }

        /// <summary>Interaktions-Komponente passend zur Station (Architektur 05: Scripts/Stations).</summary>
        static void AddInteraction(GameObject root, string id)
        {
            Interactable c;
            switch (id)
            {
                case "pult": c = root.AddComponent<ControlPanel>(); break;
                case "mixing": c = root.AddComponent<MixingStation>(); break;
                case "gramophone": c = root.AddComponent<Gramophone>(); break;
                case "tasting": c = root.AddComponent<TastingMachine>(); break;
                case "rohrpost": c = root.AddComponent<Rohrpost>(); break;
                case "beltMotor": c = root.AddComponent<BeltMotor>(); break;
                case "centrifuge": c = root.AddComponent<Centrifuge>(); break;
                case "cryo": c = root.AddComponent<CryoChamber>(); break;
                case "terminal": c = root.AddComponent<OrderTerminal>(); break;
                case "storage": c = root.AddComponent<Storage>(); break;
                default: return;
            }
            c.SetStationId(id);
        }

        static Color SignColor(string id)
        {
            switch (id)
            {
                case "mixing":
                case "processing": return M.Hex("#9DFF6A");
                case "centrifuge": return M.Hex("#9FD3FF");
                case "cryo": return M.Hex("#9FE8FF");
                case "tasting": return M.Hex("#FFB0A0");
                case "beltMotor": return M.Hex("#FFB020");
                case "storage": return M.Hex("#FF6B6B");
                default: return M.Gold;
            }
        }

        // ---------- Stationen ----------


        /// <summary>Pult im Look von Bild 08 (LookDevPult); die Kipphebel werden von PultLevers bewegt.</summary>
        static void Pult(Transform v, float chamberDistance)
        {
            var pivots = LookDevPult.Build(v, chamberDistance);
            var levers = v.gameObject.AddComponent<PultLevers>();
            var lso = new UnityEditor.SerializedObject(levers);
            var lp = lso.FindProperty("levers");
            lp.arraySize = pivots.Length;
            for (int i = 0; i < pivots.Length; i++) lp.GetArrayElementAtIndex(i).objectReferenceValue = pivots[i];
            lso.ApplyModifiedPropertiesWithoutUndo();
            AddHoseCollider(v);
        }

        /// <summary>Kollider über dem Schlauchbündel zwischen Pult und Kammer, damit der Spieler nicht hindurchläuft.</summary>
        static void AddHoseCollider(Transform v)
        {
            Bounds b = default;
            bool any = false;
            foreach (var r in v.GetComponentsInChildren<MeshRenderer>())
            {
                if (r.name != "Hose") continue;
                var c = v.InverseTransformPoint(r.bounds.center);
                var e = v.InverseTransformVector(r.bounds.extents);
                var lb = new Bounds(c, new Vector3(Mathf.Abs(e.x), Mathf.Abs(e.y), Mathf.Abs(e.z)) * 2f);
                if (!any) { b = lb; any = true; } else b.Encapsulate(lb);
            }
            if (!any) return;
            // Nur der Teil hinter dem Pult (lokal +z); der Pult selbst hat seinen eigenen Kollider
            float zStart = 0.65f, zEnd = b.max.z + 0.1f;
            var col = v.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(b.center.x, 0.5f, (zStart + zEnd) * 0.5f);
            col.size = new Vector3(b.size.x + 0.2f, 1f, zEnd - zStart);
        }


        /// <summary>Mischstation im Look des Pults: Stahltisch, drei Glastanks mit Etiketten, Rührschale, Serum-Regal.</summary>
        static void Mixing(Transform v)
        {
            LookDevKit.Box(v, "Table", new Vector3(0f, 0.45f, -0.15f), new Vector3(2.9f, 0.9f, 1.1f), LookDevMaterials.SteelPanel);
            LookDevKit.Box(v, "Top", new Vector3(0f, 0.93f, -0.15f), new Vector3(3.0f, 0.06f, 1.2f), LookDevMaterials.SteelDark);
            LookDevKit.Box(v, "Kick", new Vector3(0f, 0.08f, -0.71f), new Vector3(2.9f, 0.16f, 0.02f), LookDevMaterials.Hazard);
            LookDevKit.Bolts(v, new Vector3(-1.4f, 0.93f, -0.755f), new Vector3(1.4f, 0.93f, -0.755f), 14, Vector3.back, LookDevMaterials.Brass, 0.014f);
            foreach (float x in new[] { -0.97f, 0f, 0.97f })
                LookDevKit.Box(v, "Hatch", new Vector3(x, 0.48f, -0.705f), new Vector3(0.85f, 0.6f, 0.02f), LookDevMaterials.SteelFrame);

            var liquids = new[] { M.LiquidA, M.LiquidB, M.LiquidC };
            var icons = new[] { LookDevMaterials.IconLeaf, LookDevMaterials.IconDrop, LookDevMaterials.IconMolecule };
            string[] labels = { "ui.signs.tankA", "ui.signs.tankB", "ui.signs.tankC" };
            float[] tx = { -1.0f, -0.2f, 0.6f };
            for (int i = 0; i < 3; i++)
            {
                var t = BlockoutKit.Group(v, "Tank_" + "ABC"[i], new Vector3(tx[i], 0.96f, 0.2f));
                LookDevKit.Cyl(t, "Foot", new Vector3(0f, 0.05f, 0f), 0.33f, 0.1f, LookDevMaterials.SteelDark, null, true, 32);
                LookDevKit.BoltCircle(t, 0.29f, 0.1f, 10, LookDevMaterials.Brass, true, 0.014f);
                var glass = LookDevKit.Obj(t, "Glass", LookDevKit.Ring(0.29f, 0.31f, 1.3f, 40), new Vector3(0f, 0.75f, 0f), LookDevMaterials.GlassTank, null, true, false);
                glass.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                BlockoutKit.Cyl(t, "Liquid", new Vector3(0f, 0.7f, 0f), 0.27f, 1.2f, liquids[i], false);
                LookDevKit.Cyl(t, "Cap", new Vector3(0f, 1.45f, 0f), 0.33f, 0.14f, LookDevMaterials.SteelPanel, null, true, 32);
                LookDevKit.BoltCircle(t, 0.29f, 1.52f, 10, LookDevMaterials.Brass, true, 0.014f);
                foreach (float a in new[] { 45f, 135f, 225f, 315f })
                {
                    float r = a * Mathf.Deg2Rad;
                    LookDevKit.Cyl(t, "Rod", new Vector3(Mathf.Cos(r) * 0.32f, 0.75f, Mathf.Sin(r) * 0.32f), 0.015f, 1.3f, LookDevMaterials.Chrome, null, true, 8);
                }
                LookDevKit.Box(t, "LabelPlate", new Vector3(0f, 0.8f, -0.325f), new Vector3(0.36f, 0.42f, 0.01f), LookDevMaterials.CreamPlate);
                LookDevKit.Label(t, "Label", labels[i], new Vector3(0f, 0.86f, -0.333f), 0.75f, M.Hex("#111111"));
                LookDevKit.Plane(t, "Icon", new Vector3(0f, 0.68f, -0.333f), new Vector2(0.12f, 0.12f), icons[i]);
            }

            // Rührschale mit Arm und Motor
            var bowl = LookDevKit.Obj(v, "Bowl", LookDevKit.Lathe("MixBowl", new[] { new Vector2(0.12f, 0f), new Vector2(0.26f, 0.06f), new Vector2(0.32f, 0.3f), new Vector2(0.33f, 0.5f) }, 32, true),
                new Vector3(0.55f, 0.99f, -0.35f), LookDevMaterials.GlassTank, null, true, false);
            bowl.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LookDevKit.Cyl(v, "BowlFoot", new Vector3(0.55f, 0.98f, -0.35f), 0.16f, 0.04f, LookDevMaterials.SteelDark, null, true, 24);
            var bowlLiquid = BlockoutKit.Cyl(v, "BowlLiquid", new Vector3(0.55f, 1.15f, -0.35f), 0.27f, 0.28f, M.LampGreen, false);
            bowlLiquid.SetActive(false);
            var stirrer = BlockoutKit.Group(v, "Stirrer", new Vector3(0.55f, 1.75f, -0.35f));
            LookDevKit.Cyl(stirrer, "Rod", new Vector3(0f, -0.35f, 0f), 0.02f, 0.8f, LookDevMaterials.Chrome, null, false, 8);
            LookDevKit.Box(stirrer, "Blade", new Vector3(0f, -0.7f, 0f), new Vector3(0.24f, 0.04f, 0.04f), LookDevMaterials.Chrome, null, false);
            LookDevKit.Box(v, "StirMotor", new Vector3(0.55f, 2.0f, -0.35f), new Vector3(0.34f, 0.28f, 0.34f), LookDevMaterials.GreenPaint);
            LookDevKit.Box(v, "StirArm", new Vector3(0.55f, 1.55f, 0.05f), new Vector3(0.08f, 0.9f, 0.08f), LookDevMaterials.SteelDark);
            LookDevKit.Box(v, "StirBeam", new Vector3(0.55f, 2.0f, -0.12f), new Vector3(0.08f, 0.08f, 0.42f), LookDevMaterials.SteelDark);

            // Serum-Regal mit 6 Plätzen (Fläschchen je nach Regal sichtbar)
            LookDevKit.Box(v, "Rack", new Vector3(1.2f, 1.25f, -0.15f), new Vector3(0.5f, 0.05f, 0.95f), LookDevMaterials.SteelDark);
            foreach (float z in new[] { -0.6f, 0.3f })
                LookDevKit.Box(v, "RackPost", new Vector3(1.2f, 1.1f, z), new Vector3(0.05f, 0.3f, 0.05f), LookDevMaterials.SteelDark);
            var vials = new Renderer[6];
            for (int i = 0; i < 6; i++)
            {
                var vial = BlockoutKit.Cyl(v, "Vial_" + i, new Vector3(1.08f + (i % 2) * 0.24f, 1.43f, 0.18f - (i / 2) * 0.3f), 0.07f, 0.3f, M.LampGreen, false);
                vial.SetActive(false);
                vials[i] = vial.GetComponent<Renderer>();
            }

            var vis = v.gameObject.AddComponent<MixingStationVisuals>();
            var so = new UnityEditor.SerializedObject(vis);
            so.FindProperty("stirrer").objectReferenceValue = stirrer;
            so.FindProperty("bowlLiquid").objectReferenceValue = bowlLiquid.GetComponent<Renderer>();
            var tanks = so.FindProperty("tankLiquids");
            tanks.arraySize = 3;
            for (int i = 0; i < 3; i++) tanks.GetArrayElementAtIndex(i).objectReferenceValue = v.Find("Tank_" + "ABC"[i] + "/Liquid");
            var vp = so.FindProperty("vials");
            vp.arraySize = 6;
            for (int i = 0; i < 6; i++) vp.GetArrayElementAtIndex(i).objectReferenceValue = vials[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }









        /// <summary>Fernseher wie Bild 08: Direktor links, Parolen rechts auf blauem Feld, schwerer Rahmen mit Schrauben.</summary>
        static void Tv(Transform v)
        {
            LookDevKit.Box(v, "Frame", new Vector3(0f, 0f, 0.1f), new Vector3(5.7f, 3.6f, 0.4f), LookDevMaterials.SteelDark);
            LookDevKit.Box(v, "Bezel", new Vector3(0f, 0f, -0.11f), new Vector3(5.4f, 3.3f, 0.04f), LookDevMaterials.Rubber);
            LookDevKit.Plane(v, "Screen", new Vector3(-0.85f, 0f, -0.14f), new Vector2(3.4f, 3.1f), LookDevMaterials.DirectorCrop);
            LookDevKit.Plane(v, "SloganPanel", new Vector3(1.7f, 0f, -0.14f), new Vector2(1.7f, 3.1f), LookDevMaterials.ScreenBlue);
            LookDevKit.Label(v, "Slogan", "ui.signs.tvSlogan", new Vector3(1.7f, 0f, -0.16f), 2.5f, M.Hex("#DDE8FF"));
            LookDevKit.Bolts(v, new Vector3(-2.7f, 1.68f, -0.11f), new Vector3(2.7f, 1.68f, -0.11f), 12, Vector3.back, LookDevMaterials.Brass, 0.02f);
            LookDevKit.Bolts(v, new Vector3(-2.7f, -1.68f, -0.11f), new Vector3(2.7f, -1.68f, -0.11f), 12, Vector3.back, LookDevMaterials.Brass, 0.02f);
            foreach (var x in new[] { -2.2f, 2.2f })
                LookDevKit.Cyl(v, "Hanger", new Vector3(x, 2.4f, 0.1f), 0.04f, 1.6f, LookDevMaterials.SteelDark, null, true, 8);

            var lightGo = new GameObject("TvLight");
            lightGo.transform.SetParent(v, false);
            lightGo.transform.localPosition = new Vector3(0f, -0.2f, -1.5f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = M.Hex("#FF6A50");
            light.intensity = 1.5f;
            light.range = 8f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;
        }

        static void AngerScale(Transform v)
        {
            LookDevKit.Box(v, "Back", new Vector3(0f, 0f, 0.03f), new Vector3(0.62f, 2.5f, 0.05f), LookDevMaterials.SteelDark);
            BlockoutKit.Quad(v, "Scale", new Vector3(0f, 0f, -0.01f), new Vector2(0.5f, 2.3f), M.TermScreen, false);
            LookDevKit.Bolts(v, new Vector3(-0.27f, 1.2f, -0.005f), new Vector3(0.27f, 1.2f, -0.005f), 2, Vector3.back, LookDevMaterials.Brass, 0.015f);
            LookDevKit.Bolts(v, new Vector3(-0.27f, -1.2f, -0.005f), new Vector3(0.27f, -1.2f, -0.005f), 2, Vector3.back, LookDevMaterials.Brass, 0.015f);
        }

        /// <summary>Plantafel wie Bild 08: drei Felder (Milch, Eier, Fleisch) mit Wachstumsbalken, rote Leiste mit Parole.</summary>
        static void PlanBoard(Transform v)
        {
            LookDevKit.Box(v, "Frame", new Vector3(0f, -0.25f, 0.05f), new Vector3(5.3f, 2.0f, 0.2f), LookDevMaterials.SteelDark);
            LookDevKit.Plane(v, "Panels", new Vector3(0f, -0.05f, -0.06f), new Vector2(5.0f, 1.45f), LookDevMaterials.PlanBoardCrop);
            var keys = new[] { "ui.signs.planBoardMilk", "ui.signs.planBoardEggs", "ui.signs.planBoardMeat" };
            for (int i = 0; i < 3; i++)
                LookDevKit.Label(v, "Title_" + i, keys[i], new Vector3((i - 1) * 1.667f, 0.5f, -0.07f), 1.5f, M.Hex("#2A2119"));
            LookDevKit.Box(v, "SloganBar", new Vector3(0f, -1.0f, -0.04f), new Vector3(5.0f, 0.36f, 0.03f), LookDevMaterials.RedPaint);
            LookDevKit.Label(v, "Slogan", "ui.signs.planSlogan", new Vector3(0f, -1.0f, -0.06f), 1.5f, M.CreamText);
            LookDevKit.Bolts(v, new Vector3(-2.55f, 0.68f, -0.055f), new Vector3(2.55f, 0.68f, -0.055f), 10, Vector3.back, LookDevMaterials.Brass, 0.016f);
        }

        /// <summary>Klonanlage im Look-Material (Stahl, Messing, roter Lack).</summary>
        static void CloneLook(Transform v)
        {
            LookDevKit.Cyl(v, "Machine", new Vector3(0f, 7.4f, 0f), 1.05f, 1.7f, LookDevMaterials.SteelPanel, null, true, 32);
            LookDevKit.Cyl(v, "RedBand", new Vector3(0f, 6.5f, 0f), 1.13f, 0.28f, LookDevMaterials.RedPaint, null, true, 32);
            LookDevKit.BoltCircle(v, 1.13f, 6.5f, 24, LookDevMaterials.Brass, false, 0.02f);
            LookDevKit.Obj(v, "Tube", LookDevKit.Ring(0.58f, 0.62f, 6.0f, 48), new Vector3(0f, 3.4f, 0f), LookDevMaterials.GlassClear, null, true, false);
            LookDevKit.Cyl(v, "Foot", new Vector3(0f, 0.2f, 0f), 0.85f, 0.4f, LookDevMaterials.SteelFrame, null, true, 32);
            foreach (var y in new[] { 0.5f, 6.3f })
                BlockoutKit.MeshObj(v, "BrassRing", MeshFactory.Torus(0.64f, 0.05f), new Vector3(0f, y, 0f), LookDevMaterials.Brass);
            LookDevKit.Cyl(v, "TopPipe", new Vector3(0f, 8.4f, 0f), 0.12f, 0.6f, LookDevMaterials.SteelDark);
            BlockoutKit.Sphere(v, "Lamp", new Vector3(0f, 6.5f, -1.13f), 0.13f, M.LampGreen, false);
            BlockoutKit.Sign(v, "Plate", "ui.signs.cloneUnit", new Vector3(0f, 7.6f, -1.1f), 1.6f, M.CreamText, M.SignPlate, false);
        }
    }
}
