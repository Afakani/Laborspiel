using TMPro;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Kommissar (GDD 9): graue Uniform mit langem Mantel, Schirmmütze mit rotem Band, Klemmbrett, Schulterstücke,
    /// Schnurrbart. Darüber Schild „KOMMISSAR“ und Verdachtsbalken. Blickrichtung +Z; Beine und Arme an Gelenk-Pivots.
    /// </summary>
    public static class LookDevInspector
    {
        public static Inspector Build(Transform parent)
        {
            var root = BlockoutKit.Group(parent, "Inspector", Vector3.zero);
            var uniform = BlockoutKit.Mat("LD_InspUniform", LabMaterials.Hex("#5B6770"), 0f, 0.22f);
            var uniformDark = BlockoutKit.Mat("LD_InspTrousers", LabMaterials.Hex("#3A4146"), 0f, 0.2f);
            var skin = BlockoutKit.Mat("LD_InspSkin", LabMaterials.Hex("#F2C9A0"), 0f, 0.35f);
            var black = BlockoutKit.Mat("LD_InspBlack", LabMaterials.Hex("#1B1B1B"), 0f, 0.55f);
            var red = BlockoutKit.Mat("LD_InspRed", LabMaterials.Hex("#D01A1A"), 0f, 0.3f);
            var leather = BlockoutKit.Mat("LD_InspLeather", LabMaterials.Hex("#3B2416"), 0f, 0.5f);
            var hair = BlockoutKit.Mat("LD_InspHair", LabMaterials.Hex("#4A3524"), 0f, 0.2f);
            var bottleGlass = BlockoutKit.Transparent("LD_InspBottle", new Color(0.75f, 0.9f, 0.8f, 0.55f), 0.9f);

            var figure = BlockoutKit.Group(root, "Figure", Vector3.zero);

            // Beine mit Stiefeln
            Transform Leg(string name, float x)
            {
                var p = BlockoutKit.Group(figure, name, new Vector3(x, 0.86f, 0f));
                LookDevKit.Cyl(p, "Trouser", new Vector3(0f, -0.38f, 0f), 0.085f, 0.62f, uniformDark, null, false, 16);
                LookDevKit.Cyl(p, "BootShaft", new Vector3(0f, -0.72f, 0f), 0.095f, 0.22f, black, null, false, 16);
                LookDevKit.Box(p, "BootFoot", new Vector3(0f, -0.81f, 0.06f), new Vector3(0.16f, 0.1f, 0.3f), black, null, false);
                return p;
            }
            var legL = Leg("LegL", -0.11f);
            var legR = Leg("LegR", 0.11f);

            // Mantel (Drehkörper, oval) mit Gürtel, Knöpfen, Schulterstücken
            var coat = LookDevKit.Obj(figure, "Coat", LookDevKit.Lathe("InspectorCoat", new[]
            {
                new Vector2(0.27f, 0.5f), new Vector2(0.26f, 0.62f), new Vector2(0.245f, 0.85f), new Vector2(0.235f, 1.05f),
                new Vector2(0.25f, 1.3f), new Vector2(0.27f, 1.47f), new Vector2(0.23f, 1.58f), new Vector2(0.1f, 1.64f), new Vector2(0.06f, 1.66f)
            }, 32), Vector3.zero, uniform, null, false);
            coat.transform.localScale = new Vector3(1f, 1f, 0.72f);
            var belt = LookDevKit.Cyl(figure, "Belt", new Vector3(0f, 1.05f, 0f), 0.243f, 0.07f, leather, null, false, 32);
            belt.transform.localScale = new Vector3(1f, 1f, 0.74f);
            LookDevKit.Box(figure, "Buckle", new Vector3(0f, 1.05f, 0.18f), new Vector3(0.08f, 0.06f, 0.02f), LookDevMaterials.Brass, null, false);
            LookDevKit.Box(figure, "Strap", new Vector3(0.08f, 1.3f, 0.165f), new Vector3(0.035f, 0.5f, 0.015f), leather, new Vector3(0f, 0f, -28f), false);
            for (int i = 0; i < 4; i++)
                BlockoutKit.Sphere(figure, "Button", new Vector3(-0.06f, 1.42f - i * 0.12f, 0.178f + i * 0.004f), 0.016f, LookDevMaterials.Brass, false);
            foreach (float x in new[] { -0.2f, 0.2f })
                LookDevKit.Box(figure, "Epaulette", new Vector3(x, 1.585f, 0f), new Vector3(0.12f, 0.025f, 0.15f), red, null, false);
            foreach (float x in new[] { -0.05f, 0.05f })
                LookDevKit.Box(figure, "CollarTab", new Vector3(x, 1.6f, 0.11f), new Vector3(0.05f, 0.05f, 0.01f), red, new Vector3(-20f, 0f, 0f), false);

            // Arme: links Klemmbrett, rechts (beim Trinken) Wodkaflasche
            Transform Arm(string name, float x)
            {
                var p = BlockoutKit.Group(figure, name, new Vector3(x, 1.52f, 0f));
                LookDevKit.Cyl(p, "Sleeve", new Vector3(0f, -0.3f, 0f), 0.072f, 0.6f, uniform, null, false, 16);
                LookDevKit.Cyl(p, "Cuff", new Vector3(0f, -0.58f, 0f), 0.076f, 0.06f, red, null, false, 16);
                BlockoutKit.Sphere(p, "Hand", new Vector3(0f, -0.66f, 0.01f), 0.062f, skin, false);
                return p;
            }
            var armL = Arm("ArmL", -0.31f);
            var armR = Arm("ArmR", 0.31f);
            var board = BlockoutKit.Group(armL, "Clipboard", new Vector3(0.04f, -0.6f, 0.12f));
            board.localEulerAngles = new Vector3(-70f, 0f, 0f);
            LookDevKit.Box(board, "Board", Vector3.zero, new Vector3(0.24f, 0.32f, 0.015f), leather, null, false);
            LookDevKit.Box(board, "Sheet", new Vector3(0f, -0.01f, 0.009f), new Vector3(0.21f, 0.27f, 0.002f), LookDevMaterials.Paper, null, false);
            LookDevKit.Box(board, "Clip", new Vector3(0f, 0.15f, 0.012f), new Vector3(0.08f, 0.03f, 0.012f), LookDevMaterials.Chrome, null, false);
            var bottle = BlockoutKit.Group(armR, "Bottle", new Vector3(0f, -0.7f, 0.06f));
            LookDevKit.Cyl(bottle, "Body", new Vector3(0f, 0.02f, 0f), 0.045f, 0.2f, bottleGlass, null, false, 16);
            LookDevKit.Cyl(bottle, "Neck", new Vector3(0f, 0.16f, 0f), 0.018f, 0.1f, bottleGlass, null, false, 12);
            LookDevKit.Box(bottle, "Label", new Vector3(0f, 0.02f, 0.046f), new Vector3(0.06f, 0.07f, 0.004f), red, null, false);
            bottle.gameObject.SetActive(false);

            // Kopf mit Schirmmütze
            var head = BlockoutKit.Group(figure, "Head", new Vector3(0f, 1.64f, 0f));
            LookDevKit.Cyl(head, "Neck", new Vector3(0f, 0.05f, 0f), 0.06f, 0.1f, skin, null, false, 16);
            var skull = BlockoutKit.Sphere(head, "Skull", new Vector3(0f, 0.2f, 0f), 0.125f, skin, false);
            skull.transform.localScale = new Vector3(0.23f, 0.28f, 0.25f);
            BlockoutKit.Sphere(head, "Nose", new Vector3(0f, 0.19f, 0.125f), 0.028f, skin, false);
            foreach (float x in new[] { -0.045f, 0.045f })
            {
                BlockoutKit.Sphere(head, "Eye", new Vector3(x, 0.235f, 0.108f), 0.014f, black, false);
                LookDevKit.Box(head, "Brow", new Vector3(x, 0.262f, 0.112f), new Vector3(0.05f, 0.012f, 0.012f), hair, null, false);
                BlockoutKit.Sphere(head, "Ear", new Vector3(x * 2.75f, 0.2f, 0f), 0.03f, skin, false);
            }
            LookDevKit.Box(head, "Moustache", new Vector3(0f, 0.155f, 0.118f), new Vector3(0.11f, 0.025f, 0.025f), hair, null, false);
            var cap = BlockoutKit.Group(head, "Cap", new Vector3(0f, 0.3f, -0.005f));
            LookDevKit.Cyl(cap, "Band", new Vector3(0f, 0f, 0f), 0.135f, 0.05f, red, null, false, 32);
            LookDevKit.Obj(cap, "Crown", LookDevKit.Lathe("InspectorCapCrown", new[]
            {
                new Vector2(0.135f, 0.02f), new Vector2(0.15f, 0.06f), new Vector2(0.175f, 0.1f), new Vector2(0.17f, 0.12f), new Vector2(0.0f, 0.125f)
            }, 32), Vector3.zero, uniform, null, false);
            LookDevKit.Box(cap, "Visor", new Vector3(0f, -0.02f, 0.14f), new Vector3(0.2f, 0.012f, 0.09f), black, new Vector3(12f, 0f, 0f), false);
            LookDevKit.Box(cap, "Badge", new Vector3(0f, 0.01f, 0.137f), new Vector3(0.035f, 0.035f, 0.01f), LookDevMaterials.Brass, new Vector3(0f, 0f, 45f), false);

            // Schild und Verdachtsbalken (drehen sich zur Kamera)
            var overhead = BlockoutKit.Group(root, "Overhead", new Vector3(0f, 2.6f, 0f));
            var plate = LookDevKit.Box(overhead, "Plate", Vector3.zero, new Vector3(1.0f, 0.26f, 0.02f), red, null, false);
            var label = LookDevKit.Label(overhead, "Sign", "ui.signs.inspector", new Vector3(0f, 0f, -0.015f), 1.6f, Color.white);
            Object.DestroyImmediate(label.GetComponent<StationSign>());
            var tmp = label.GetComponentInChildren<TMP_Text>();
            tmp.rectTransform.sizeDelta = new Vector2(0.95f, 0.24f);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.5f;
            tmp.fontSizeMax = 1.6f;
            LookDevKit.Box(overhead, "BarBg", new Vector3(0f, -0.24f, 0f), new Vector3(0.84f, 0.1f, 0.02f), black, null, false);
            var fillPivot = BlockoutKit.Group(overhead, "BarFill", new Vector3(-0.4f, -0.24f, -0.012f));
            var fillMat = BlockoutKit.Emissive("LD_InspSuspicion", LabMaterials.Hex("#FF4A3D"), 0.7f, false);
            LookDevKit.Box(fillPivot, "Fill", new Vector3(0.4f, 0f, 0f), new Vector3(0.8f, 0.06f, 0.01f), fillMat, null, false);
            fillPivot.localScale = new Vector3(0.001f, 1f, 1f);

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                r.shadowCastingMode = r.transform.IsChildOf(overhead) ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = false;

            var ins = root.gameObject.AddComponent<Inspector>();
            ins.Setup(figure, legL, legR, armL, armR, head, bottle.gameObject, overhead, tmp, plate.GetComponent<Renderer>(), fillPivot);
            return ins;
        }
    }
}
