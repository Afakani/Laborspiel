using UnityEngine;
using M = PlanErfuellt.EditorTools.LabMaterials;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Glaskammer B-12 mit Rahmen, Messingringen, Düsen, Gas, Kammerlicht, Förderband und Laken (GDD 3, Art Direction 1).
    /// </summary>
    public static class LabChamberBuilder
    {
        public static GameObject Build(LayoutConfig L, float playerRadius, GameObject cowPrefab, GameObject henPrefab)
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
            var rings = new System.Collections.Generic.List<Renderer>();
            foreach (var p in new[] { new Vector2(-1.1f, -0.6f), new Vector2(0f, 0.9f), new Vector2(1.1f, -0.6f) })
            {
                var n = BlockoutKit.Group(nozzles, "Nozzle", new Vector3(p.x, y0 + H - 0.32f, p.y));
                BlockoutKit.Cyl(n, "Body", Vector3.zero, 0.24f, 0.55f, M.Steel);
                rings.Add(BlockoutKit.MeshObj(n, "Ring", MeshFactory.Torus(0.22f, 0.045f), new Vector3(0f, -0.3f, 0f), M.ChamberRing, false).GetComponent<Renderer>());
            }

            // Gas als leicht leuchtender Zylinder (wird später animiert)
            var gas = BlockoutKit.Cyl(root.transform, "GasVFX", new Vector3(0f, y0 + H * 0.5f, 0f), R - 0.06f, H - 0.2f, M.Gas, false);
            BlockoutKit.CastShadows(gas, false);
            var gasFx = gas.AddComponent<ChamberGas>();
            var gso = new UnityEditor.SerializedObject(gasFx);
            gso.FindProperty("gas").objectReferenceValue = gas.GetComponent<Renderer>();
            var rp = gso.FindProperty("nozzleRings");
            rp.arraySize = rings.Count;
            for (int i = 0; i < rings.Count; i++) rp.GetArrayElementAtIndex(i).objectReferenceValue = rings[i];
            gso.ApplyModifiedPropertiesWithoutUndo();

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
            // Tier: Kuh und Huhn als verschachtelte Prefabs, Schild und Glühlicht
            var animals = BlockoutKit.Group(root.transform, "Animal", Vector3.zero);
            var cow = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(cowPrefab, animals)).GetComponent<AnimalVisuals>();
            var hen = ((GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(henPrefab, animals)).GetComponent<AnimalVisuals>();
            var label = BuildLabel(animals);
            var glowGo = new GameObject("AnimalGlow");
            glowGo.transform.SetParent(animals, false);
            var glow = glowGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 8f;
            glow.intensity = 0f;
            glow.shadows = LightShadows.None;
            glow.lightmapBakeType = LightmapBakeType.Realtime;
            var chamberLogic = root.AddComponent<Chamber>();
            chamberLogic.SetModels(cow, hen, label, glow);
            var clothInteraction = root.AddComponent<Cloth>();
            clothInteraction.SetStationId("cloth");
            clothInteraction.SetVisual(cloth);

            // Spieler-Kollision: Mittelpunkt des Spielers bleibt mindestens playerCollisionRadius (GDD 3: 3.1) entfernt
            var col = root.AddComponent<CapsuleCollider>();
            col.radius = c.playerCollisionRadius - playerRadius;
            col.height = L.room.height;
            col.center = new Vector3(0f, L.room.height * 0.5f, 0f);
            return root;
        }

        /// <summary>Schild über dem Tier: Rahmen (einfärbbar), dunkle Platte, zwei Zeilen, Stempel.</summary>
        static AnimalLabel BuildLabel(Transform parent)
        {
            var root = BlockoutKit.Group(parent, "AnimalLabel", Vector3.zero);
            var frame = BlockoutKit.Quad(root, "Frame", new Vector3(0f, 0f, 0.01f), new Vector2(2.3f, 0.8f), M.Brass, false);
            frame.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var plate = BlockoutKit.Quad(root, "Plate", new Vector3(0f, 0f, 0f), new Vector2(2.2f, 0.7f), M.SignPlate, false);
            plate.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var l1 = LabelText(root, "Line1", new Vector3(0f, 0.14f, -0.02f), 3.2f);
            var l2 = LabelText(root, "Line2", new Vector3(0f, -0.17f, -0.02f), 2.4f);
            var st = LabelText(root, "Stamp", new Vector3(0.85f, 0.32f, -0.03f), 1.6f);
            st.color = LabMaterials.Hex("#D01A1A");
            st.transform.localEulerAngles = new Vector3(0f, 0f, 14f);
            st.gameObject.SetActive(false);
            var label = root.gameObject.AddComponent<AnimalLabel>();
            var so = new UnityEditor.SerializedObject(label);
            so.FindProperty("line1").objectReferenceValue = l1;
            so.FindProperty("line2").objectReferenceValue = l2;
            so.FindProperty("stamp").objectReferenceValue = st;
            so.FindProperty("frame").objectReferenceValue = frame.GetComponent<Renderer>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return label;
        }

        static TMPro.TextMeshPro LabelText(Transform parent, string name, Vector3 pos, float size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            var tmp = go.AddComponent<TMPro.TextMeshPro>();
            tmp.font = FontAssetCreator.Ensure($"{BlockoutKit.FontDir}/Oswald/Oswald-Bold.ttf", $"{BlockoutKit.FontDir}/Oswald/Oswald-Bold SDF.asset");
            tmp.fontSize = size;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(2.1f, 0.4f);
            tmp.richText = true;
            return tmp;
        }
    }
}
