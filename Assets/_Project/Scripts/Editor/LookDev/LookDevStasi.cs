using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Stasi-Mann für das Spielende Sibirien (D162): langer dunkler Ledermantel, Hut, Sonnenbrille, breite Schultern,
    /// grimmiger Mund. Blickrichtung +Z; Beine an Hüft-Pivots für den Gang. Ohne Kollider, nicht statisch.
    /// </summary>
    public static class LookDevStasi
    {
        public static Transform Build(Transform parent, string name, Vector3 pos, out Transform legL, out Transform legR, bool withSign)
        {
            var coatMat = BlockoutKit.Mat("LD_StasiCoat", LabMaterials.Hex("#1E1F22"), 0.1f, 0.55f);
            var trousers = BlockoutKit.Mat("LD_StasiTrousers", LabMaterials.Hex("#2A2C30"), 0f, 0.25f);
            var skin = BlockoutKit.Mat("LD_StasiSkin", LabMaterials.Hex("#E8BE96"), 0f, 0.35f);
            var black = BlockoutKit.Mat("LD_StasiBlack", LabMaterials.Hex("#0E0E0E"), 0.2f, 0.85f);
            var hat = BlockoutKit.Mat("LD_StasiHat", LabMaterials.Hex("#33302C"), 0f, 0.25f);
            var band = BlockoutKit.Mat("LD_StasiBand", LabMaterials.Hex("#111111"), 0f, 0.3f);
            var red = BlockoutKit.Mat("LD_StasiRed", LabMaterials.Hex("#C0201A"), 0f, 0.3f);
            var plate = BlockoutKit.Mat("LD_StasiSign", LabMaterials.Hex("#EDE3C8"), 0f, 0.3f);

            var root = BlockoutKit.Group(parent, name, pos);

            Transform Leg(string n, float x)
            {
                var p = BlockoutKit.Group(root, n, new Vector3(x, 0.86f, 0f));
                LookDevKit.Cyl(p, "Trouser", new Vector3(0f, -0.4f, 0f), 0.095f, 0.7f, trousers, null, false, 16);
                LookDevKit.Box(p, "Shoe", new Vector3(0f, -0.81f, 0.06f), new Vector3(0.17f, 0.1f, 0.32f), black, null, false);
                return p;
            }
            legL = Leg("LegL", -0.12f);
            legR = Leg("LegR", 0.12f);

            // Mantel bis unters Knie, breite Schultern
            var coat = LookDevKit.Obj(root, "Coat", LookDevKit.Lathe("StasiCoat", new[]
            {
                new Vector2(0.31f, 0.42f), new Vector2(0.29f, 0.6f), new Vector2(0.27f, 0.9f), new Vector2(0.27f, 1.1f),
                new Vector2(0.3f, 1.35f), new Vector2(0.32f, 1.5f), new Vector2(0.26f, 1.6f), new Vector2(0.11f, 1.66f), new Vector2(0.06f, 1.68f)
            }, 32), Vector3.zero, coatMat, null, false);
            coat.transform.localScale = new Vector3(1.12f, 1f, 0.75f);
            var belt = LookDevKit.Cyl(root, "Belt", new Vector3(0f, 1.05f, 0f), 0.275f, 0.07f, black, null, false, 32);
            belt.transform.localScale = new Vector3(1.12f, 1f, 0.77f);
            LookDevKit.Box(root, "Lapel", new Vector3(0f, 1.48f, 0.2f), new Vector3(0.22f, 0.22f, 0.02f), black, new Vector3(-15f, 0f, 0f), false);

            // Arme hängen steif herab
            foreach (float x in new[] { -0.37f, 0.37f })
            {
                var a = BlockoutKit.Group(root, x < 0f ? "ArmL" : "ArmR", new Vector3(x, 1.52f, 0f));
                LookDevKit.Cyl(a, "Sleeve", new Vector3(0f, -0.32f, 0f), 0.085f, 0.64f, coatMat, null, false, 16);
                BlockoutKit.Sphere(a, "Glove", new Vector3(0f, -0.68f, 0.01f), 0.07f, black, false);
            }

            // Kopf mit Hut und Sonnenbrille
            var head = BlockoutKit.Group(root, "Head", new Vector3(0f, 1.66f, 0f));
            LookDevKit.Cyl(head, "Neck", new Vector3(0f, 0.05f, 0f), 0.07f, 0.1f, skin, null, false, 16);
            var skull = BlockoutKit.Sphere(head, "Skull", new Vector3(0f, 0.2f, 0f), 0.13f, skin, false);
            skull.transform.localScale = new Vector3(0.25f, 0.29f, 0.26f);
            LookDevKit.Box(head, "Jaw", new Vector3(0f, 0.11f, 0.03f), new Vector3(0.2f, 0.08f, 0.17f), skin, null, false);
            LookDevKit.Box(head, "Glasses", new Vector3(0f, 0.235f, 0.12f), new Vector3(0.2f, 0.05f, 0.02f), black, null, false);
            LookDevKit.Box(head, "Mouth", new Vector3(0f, 0.13f, 0.12f), new Vector3(0.07f, 0.012f, 0.01f), band, null, false);
            var hatG = BlockoutKit.Group(head, "Hat", new Vector3(0f, 0.31f, 0f));
            LookDevKit.Cyl(hatG, "Brim", Vector3.zero, 0.24f, 0.02f, hat, null, false, 32);
            LookDevKit.Cyl(hatG, "Crown", new Vector3(0f, 0.08f, 0f), 0.15f, 0.15f, hat, null, false, 32);
            LookDevKit.Cyl(hatG, "Band", new Vector3(0f, 0.025f, 0f), 0.152f, 0.035f, band, null, false, 32);

            // Schild „Sibirien-Express – einfache Fahrt“ vor der Brust
            if (withSign)
            {
                var sign = BlockoutKit.Group(root, "Sign", new Vector3(0f, 1.15f, 0.3f));
                LookDevKit.Box(sign, "Board", Vector3.zero, new Vector3(0.9f, 0.45f, 0.02f), plate, null, false);
                LookDevKit.Box(sign, "Stripe", new Vector3(0f, -0.03f, 0.012f), new Vector3(0.86f, 0.03f, 0.004f), red, null, false);
                var l1 = LookDevKit.Label(sign, "Line1", "ui.signs.siberiaSign", new Vector3(0f, 0.09f, 0.015f), 1.1f, LabMaterials.Hex("#8E1B14"), new Vector3(0f, 180f, 0f));
                var l2 = LookDevKit.Label(sign, "Line2", "ui.signs.siberiaSign2", new Vector3(0f, -0.12f, 0.015f), 0.8f, LabMaterials.Hex("#2A2119"), new Vector3(0f, 180f, 0f));
                Object.DestroyImmediate(l1.GetComponent<StationSign>());
                Object.DestroyImmediate(l2.GetComponent<StationSign>());
            }

            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.isStatic = false;
            return root;
        }
    }
}
