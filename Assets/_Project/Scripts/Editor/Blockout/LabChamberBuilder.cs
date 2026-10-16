using UnityEngine;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Glaskammer B-12 mit Rahmen, Messingringen, Düsen, Gas, Kammerlicht, Förderband und Laken (GDD 3, Art Direction 1).
    /// </summary>
    public static class LabChamberBuilder
    {
        public static GameObject Build(LayoutConfig L)
        {
            var c = L.chamber;
            float R = c.radius, H = c.height, y0 = c.baseY;
            var root = new GameObject("Chamber");

            var frame = BlockoutKit.Group(root.transform, "Frame", Vector3.zero);
            BlockoutKit.Cyl(frame, "Base", new Vector3(0f, y0, 0f), R + 0.35f, 0.3f, M.SteelDark);
            BlockoutKit.Cyl(frame, "TopCap", new Vector3(0f, y0 + H + 0.1f, 0f), R + 0.2f, 0.35f, M.SteelDark);
            foreach (var ry in new[] { 0.35f, 2.4f, 4.6f, 6.6f })
                BlockoutKit.MeshObj(frame, "BrassRing", MeshFactory.Torus(R + 0.02f, 0.06f), new Vector3(0f, y0 + ry, 0f), M.Brass);
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                BlockoutKit.Box(frame, "Strut", new Vector3(Mathf.Cos(a) * (R + 0.03f), y0 + H * 0.5f, Mathf.Sin(a) * (R + 0.03f)),
                    new Vector3(0.1f, H, 0.12f), M.SteelDark, true, new Vector3(0f, -a * Mathf.Rad2Deg, 0f));
            }

            var glass = BlockoutKit.Cyl(root.transform, "Glass", new Vector3(0f, y0 + H * 0.5f, 0f), R, H, M.Glass);
            BlockoutKit.CastShadows(glass, false);

            // Düsen (Prototyp: (-1.1, 0.6), (0, -0.9), (1.1, 0.6) → z gespiegelt)
            var nozzles = BlockoutKit.Group(root.transform, "Nozzles", Vector3.zero);
            foreach (var p in new[] { new Vector2(-1.1f, -0.6f), new Vector2(0f, 0.9f), new Vector2(1.1f, -0.6f) })
            {
                var n = BlockoutKit.Group(nozzles, "Nozzle", new Vector3(p.x, y0 + H - 0.32f, p.y));
                BlockoutKit.Cyl(n, "Body", Vector3.zero, 0.24f, 0.55f, M.Steel);
                BlockoutKit.MeshObj(n, "Ring", MeshFactory.Torus(0.22f, 0.045f), new Vector3(0f, -0.3f, 0f), M.ChamberRing, false);
            }

            // Gas als leicht leuchtender Zylinder (wird später animiert)
            var gas = BlockoutKit.Cyl(root.transform, "GasVFX", new Vector3(0f, y0 + H * 0.5f, 0f), R - 0.06f, H - 0.2f, M.Gas, false);
            BlockoutKit.CastShadows(gas, false);

            // Schilder vorne oben
            BlockoutKit.Sign(root.transform, "Plate_Number", "ui.signs.chamberNumber", new Vector3(0f, y0 + H + 0.1f, -(R + 0.22f)), 4.5f, M.CreamText, M.SignPlate, false);
            BlockoutKit.Sign(root.transform, "Plate_Slogan", "ui.signs.chamberPlate", new Vector3(0f, y0 + H - 0.5f, -(R + 0.12f)), 2.0f, M.CreamText, M.PropRedBright, false);

            // Kammerlicht (Echtzeit, ohne Schatten)
            var lightGo = new GameObject("ChamberLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 3.2f, -0.6f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = LabMaterials.Hex("#6DFF4A");
            light.intensity = 3f;
            light.range = 9f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;

            // Förderband quer durch die Kammer
            var b = L.belt;
            var belt = BlockoutKit.Box(root.transform, "Belt", new Vector3((b.xMin + b.xMax) * 0.5f, b.height * 0.5f, 0f),
                new Vector3(b.xMax - b.xMin, b.height, b.halfWidth * 2f), M.Rubber);
            for (float x = b.xMin + 0.5f; x < b.xMax; x += 1f)
                BlockoutKit.Box(belt.transform.parent, "BeltChevron", new Vector3(x, b.height + 0.005f, 0f), new Vector3(0.12f, 0.01f, b.halfWidth * 1.4f), M.Hazard, true, new Vector3(0f, 30f, 0f));

            // Laken (abgedeckter Zustand, standardmäßig aus)
            var cloth = BlockoutKit.Cyl(root.transform, "Cloth", new Vector3(0f, y0 + 4.2f, 0f), R + 0.5f, 8.4f, M.Cloth, false);
            cloth.SetActive(false);

            // Spieler-Kollision: Radius aus der Config (GDD 3: 3.1)
            var col = root.AddComponent<CapsuleCollider>();
            col.radius = c.playerCollisionRadius;
            col.height = L.room.height;
            col.center = new Vector3(0f, L.room.height * 0.5f, 0f);
            return root;
        }
    }
}
