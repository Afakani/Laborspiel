using UnityEditor;
using UnityEngine;
using L = PlanErfuellt.EditorTools.LookDevMaterials;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Maschinen des Baumodus (GDD 6.10, Prototyp buildMachineModel) im Look-Pass: Melkpumpe, Auto-Sprüher,
    /// Kühlaggregat, Notfall-Automat als Prefabs, dazu Rohr-Prefab und Materialien. Das Prefab schaut mit +Z zur Kammer;
    /// das Modell darin ist um 180° gedreht, damit Bedienteile, Kreuz und Lüfter zur Kammer und damit zum Spieler zeigen.
    /// </summary>
    public static class LookDevBuildMachines
    {
        const string PrefabDir = "Assets/_Project/Prefabs/Machines";

        public static Material Ghost()
        {
            var m = BlockoutKit.Transparent("LD_BuildGhost", new Color(0.45f, 1f, 0.55f, 0.4f), 0.4f, new Color(0.1f, 0.5f, 0.15f));
            return m;
        }

        public static Material PipeMaterial(string id) =>
            id == BuildLogic.Pump ? Enamel() : id == BuildLogic.Cool ? L.BarBlue : L.Chrome;

        /// <summary>Sauberer Emaillelack (die Look-Dev-Emaille ist für neue Maschinen zu rostig).</summary>
        static Material Enamel() => BlockoutKit.Mat("LD_MachineEnamel", LabMaterials.Hex("#E6E0D0"), 0.05f, 0.6f);
        static Material CoolWhite() => BlockoutKit.Mat("LD_MachineWhite", LabMaterials.Hex("#D8E6EF"), 0.2f, 0.55f);

        /// <summary>Alle vier Maschinen als Prefabs bauen bzw. überschreiben.</summary>
        public static MachinePrefab[] BuildAll()
        {
            BlockoutKit.EnsureFolder(PrefabDir);
            var ids = new[] { BuildLogic.Pump, BuildLogic.Spray, BuildLogic.Cool, BuildLogic.Doc };
            var list = new MachinePrefab[ids.Length];
            for (int i = 0; i < ids.Length; i++)
                list[i] = new MachinePrefab { id = ids[i], prefab = Save(ids[i]), pipeMaterial = PipeMaterial(ids[i]) };
            return list;
        }

        /// <summary>Rohr: Einheitszylinder (Länge 1 entlang Y, Mitte im Ursprung).</summary>
        public static GameObject Pipe()
        {
            var root = new GameObject("Pipe");
            LookDevKit.Cyl(root.transform, "Tube", Vector3.zero, 0.06f, 1f, L.Chrome, null, false, 12);
            foreach (var c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/Pipe.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject Save(string id)
        {
            var root = new GameObject("Machine_" + id);
            var t = BlockoutKit.Group(root.transform, "Model", Vector3.zero, 180f);
            LookDevKit.Box(t, "Base", new Vector3(0f, 0.1f, 0f), new Vector3(1.6f, 0.2f, 1.6f), L.SteelDark, null, false);
            LookDevKit.Bolts(t, new Vector3(-0.7f, 0.2f, -0.81f), new Vector3(0.7f, 0.2f, -0.81f), 6, Vector3.back, L.Brass, 0.016f);
            Transform piston = null, fan = null;
            Renderer glow = null;
            switch (id)
            {
                case BuildLogic.Pump: piston = Pump(t); break;
                case BuildLogic.Spray: glow = Spray(t); break;
                case BuildLogic.Cool: fan = Cool(t); break;
                default: glow = Doc(t); break;
            }
            foreach (var c in root.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            foreach (var tr in root.GetComponentsInChildren<Transform>()) tr.gameObject.isStatic = false;
            var col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.1f, 0f);
            col.size = new Vector3(1.5f, 2.2f, 1.5f);
            root.AddComponent<MachineFx>().Setup(piston, fan, glow);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, $"{PrefabDir}/Machine_{id}.prefab");
            Object.DestroyImmediate(root);
            return prefab;
        }

        // ---------- Modelle ----------

        /// <summary>Melkpumpe: Emaille-Kessel, rote Haube, Messingring, Kolben mit Querhaupt, Manometer, Milchleitung.</summary>
        static Transform Pump(Transform t)
        {
            LookDevKit.Obj(t, "Tank", LookDevKit.Lathe("PumpTank", new[] { new Vector2(0.6f, 0.2f), new Vector2(0.6f, 0.3f), new Vector2(0.56f, 1.5f), new Vector2(0.5f, 1.65f) }, 32),
                Vector3.zero, Enamel(), null, false);
            LookDevKit.Cyl(t, "Cap", new Vector3(0f, 1.73f, 0f), 0.58f, 0.18f, L.RedPaint, null, false, 32);
            BlockoutKit.MeshObj(t, "Ring", MeshFactory.Torus(0.6f, 0.04f), new Vector3(0f, 0.6f, 0f), L.Brass, false);
            BlockoutKit.MeshObj(t, "Ring", MeshFactory.Torus(0.58f, 0.04f), new Vector3(0f, 1.3f, 0f), L.Brass, false);
            var piston = BlockoutKit.Group(t, "Piston", new Vector3(0f, 1.82f, 0f));
            LookDevKit.Cyl(piston, "Rod", new Vector3(0f, 0.3f, 0f), 0.06f, 0.6f, L.Chrome, null, false, 12);
            LookDevKit.Box(piston, "Head", new Vector3(0f, 0.62f, 0f), new Vector3(0.55f, 0.1f, 0.14f), L.SteelDark, null, false);
            Gauge(t, new Vector3(0f, 1.1f, -0.6f), 0.15f);
            LookDevKit.Obj(t, "MilkLine", LookDevKit.Tube("PumpLine", new Vector3(0.45f, 0.4f, 0.3f), new Vector3(0.7f, 0.5f, 0.5f), new Vector3(0.4f, 1.1f, 0.7f), new Vector3(0f, 1.1f, 0.75f), 0.05f, 16, 10),
                Vector3.zero, Enamel(), null, false);
            return piston;
        }

        /// <summary>Auto-Sprüher: Säule, rote Sprühkammer, Düse zur Kammer, Serumflasche, Kontrolllampe.</summary>
        static Renderer Spray(Transform t)
        {
            LookDevKit.Obj(t, "Column", LookDevKit.Lathe("SprayColumn", new[] { new Vector2(0.3f, 0.2f), new Vector2(0.26f, 0.35f), new Vector2(0.22f, 1.7f), new Vector2(0.28f, 1.75f) }, 24),
                Vector3.zero, L.SteelPanel, null, false);
            LookDevKit.Box(t, "Head", new Vector3(0f, 2.0f, 0f), new Vector3(0.7f, 0.5f, 0.7f), L.RedPaint, null, false);
            LookDevKit.Bolts(t, new Vector3(-0.3f, 2.2f, -0.355f), new Vector3(0.3f, 2.2f, -0.355f), 4, Vector3.back, L.Brass, 0.014f);
            LookDevKit.Cyl(t, "Nozzle", new Vector3(0f, 2.0f, 0.55f), 0.08f, 0.45f, L.Chrome, new Vector3(90f, 0f, 0f), false, 16);
            LookDevKit.Cyl(t, "NozzleTip", new Vector3(0f, 2.0f, 0.8f), 0.11f, 0.06f, L.Brass, new Vector3(90f, 0f, 0f), false, 16);
            // Serumflasche hinten
            LookDevKit.Cyl(t, "Bottle", new Vector3(0f, 1.35f, -0.36f), 0.12f, 0.45f, L.GlassTank, null, false, 16);
            BlockoutKit.Cyl(t, "Serum", new Vector3(0f, 1.3f, -0.36f), 0.1f, 0.32f, M.LiquidB, false);
            var led = BlockoutKit.Sphere(t, "Led", new Vector3(0f, 2.32f, -0.2f), 0.09f, M.LampGreen, false);
            return led.GetComponent<Renderer>();
        }

        /// <summary>Kühlaggregat: Schrank mit Kühlrippen, blaues Band, Lüfter im Gitter, Kältemittelleitung.</summary>
        static Transform Cool(Transform t)
        {
            LookDevKit.Box(t, "Cabinet", new Vector3(0f, 1.05f, 0f), new Vector3(1.5f, 1.7f, 1.1f), CoolWhite(), null, false);
            LookDevKit.Box(t, "Band", new Vector3(0f, 1.95f, 0f), new Vector3(1.52f, 0.18f, 1.12f), L.BarBlue, null, false);
            for (int i = 0; i < 7; i++)
                LookDevKit.Box(t, "Fin", new Vector3(0.76f, 0.5f + i * 0.17f, 0f), new Vector3(0.04f, 0.06f, 1.0f), L.SteelPanel, null, false);
            var grille = BlockoutKit.Group(t, "Grille", new Vector3(0f, 1.1f, -0.57f));
            BlockoutKit.MeshObj(grille, "Ring", MeshFactory.Torus(0.42f, 0.04f), Vector3.zero, L.SteelDark, false, new Vector3(90f, 0f, 0f));
            var fan = BlockoutKit.Group(grille, "Fan", Vector3.zero);
            for (int i = 0; i < 4; i++)
                LookDevKit.Box(fan, "Blade", Vector3.zero, new Vector3(0.72f, 0.1f, 0.02f), L.SteelPanel, new Vector3(0f, 0f, i * 45f), false);
            LookDevKit.Cyl(fan, "Hub", Vector3.zero, 0.07f, 0.06f, L.Chrome, new Vector3(90f, 0f, 0f), false, 12);
            LookDevKit.Obj(t, "Coolant", LookDevKit.Tube("CoolLine", new Vector3(0.5f, 1.9f, 0.4f), new Vector3(0.5f, 2.3f, 0.5f), new Vector3(0.2f, 1.6f, 0.7f), new Vector3(0f, 1.1f, 0.75f), 0.05f, 16, 10),
                Vector3.zero, L.Frost, null, false);
            return fan;
        }

        /// <summary>Notfall-Automat: weißer Schrank mit rotem Kreuz, grüner Bildschirm, Spritzen-Arm, Gegenmittel-Fenster.</summary>
        static Renderer Doc(Transform t)
        {
            LookDevKit.Box(t, "Cabinet", new Vector3(0f, 1.25f, 0f), new Vector3(1.2f, 2.1f, 0.9f), Enamel(), null, false);
            LookDevKit.Box(t, "CrossH", new Vector3(0f, 1.55f, -0.465f), new Vector3(0.62f, 0.16f, 0.03f), L.RedPaint, null, false);
            LookDevKit.Box(t, "CrossV", new Vector3(0f, 1.55f, -0.465f), new Vector3(0.16f, 0.62f, 0.03f), L.RedPaint, null, false);
            LookDevKit.Box(t, "ScreenFrame", new Vector3(0f, 2.02f, -0.455f), new Vector3(0.78f, 0.36f, 0.03f), L.BlackPaint, null, false);
            var screen = LookDevKit.Box(t, "Screen", new Vector3(0f, 2.02f, -0.47f), new Vector3(0.7f, 0.28f, 0.01f), M.LampGreen, null, false);
            LookDevKit.Box(t, "Window", new Vector3(0f, 0.85f, -0.455f), new Vector3(0.5f, 0.35f, 0.02f), L.GlassBlue, null, false);
            for (int i = 0; i < 3; i++)
                BlockoutKit.Cyl(t, "Ampoule", new Vector3(-0.15f + i * 0.15f, 0.85f, -0.4f), 0.04f, 0.22f, M.Antidote, false);
            LookDevKit.Obj(t, "Arm", LookDevKit.Tube("DocArm", new Vector3(0.4f, 1.9f, 0.45f), new Vector3(0.4f, 2.4f, 0.7f), new Vector3(0.1f, 1.6f, 0.9f), new Vector3(0f, 1.1f, 0.8f), 0.04f, 16, 8),
                Vector3.zero, L.Chrome, null, false);
            return screen.GetComponent<Renderer>();
        }

        static void Gauge(Transform parent, Vector3 pos, float r)
        {
            LookDevKit.Cyl(parent, "GaugeRim", pos, r + 0.02f, 0.04f, L.Brass, new Vector3(90f, 0f, 0f), false, 24);
            LookDevKit.Cyl(parent, "GaugeFace", pos + new Vector3(0f, 0f, -0.022f), r, 0.01f, L.DialFace, new Vector3(90f, 0f, 0f), false, 24);
        }
    }
}
