using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Platzhalter-Modelle von Kuh und Huhn aus Grundkörpern, Maße wie im Prototyp (buildCowModel/buildChickenModel).
    /// Blick in +X (Laufrichtung auf dem Band). Werden in Phase 11 durch echte Modelle ersetzt.
    /// </summary>
    public static class AnimalBuilder
    {
        public const string CowPath = "Assets/_Project/Prefabs/Animals/Cow.prefab";
        public const string HenPath = "Assets/_Project/Prefabs/Animals/Hen.prefab";

        public static GameObject LoadOrBuild(string path, bool rebuild, System.Func<GameObject> create)
        {
            BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(path));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && !rebuild) return prefab;
            var temp = create();
            prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        static Material Body(string name, string hex) => BlockoutKit.Emissive(name, LabMaterials.Hex(hex), 0f, false);
        static Material M(string name, string hex, float smooth = 0.25f) => BlockoutKit.Mat(name, LabMaterials.Hex(hex), 0f, smooth);

        public static GameObject BuildCow()
        {
            var root = new GameObject("Cow");
            var t = root.transform;
            var body = Body("CowBody", "#FFFFFF");
            var spot = M("CowSpot", "#222222", 0.1f);
            var pink = M("CowPink", "#F6B6C1");
            var udderM = M("CowUdder", "#FF9FB5", 0.3f);
            var teat = M("CowTeat", "#FF7F9D", 0.3f);
            var hoof = M("CowHoof", "#1D1612", 0.5f);
            var nost = M("CowNostril", "#3A1A1F", 0.6f);
            var eyeW = M("EyeWhite", "#F4EFE3", 0.8f);
            var eyeB = M("EyePupil", "#0A0806", 0.9f);
            var horn = M("CowHorn", "#E8DCC0", 0.5f);
            var collar = M("CowCollar", "#D01A1A", 0.2f);
            var tag = M("EarTag", "#F2B705", 0.4f);

            var bodyGo = Ellipsoid(t, "Body", new Vector3(0f, 0.95f, 0f), new Vector3(1.15f, 0.62f, 0.62f), body);
            var spots = new List<Renderer>();
            foreach (var s in new[] { new Vector3(0.3f, 0.9f, 0.3f), new Vector3(2.6f, 0.4f, 0.3f), new Vector3(-0.5f, 0.7f, 0.3f),
                         new Vector3(0.6f, 0.3f, 0.55f), new Vector3(0.2f, -0.1f, 0.7f), new Vector3(-0.6f, 0.2f, 0.62f) })
            {
                float th = s.x + 2.2f, ph = s.y;
                var p = new Vector3(1.15f * Mathf.Cos(ph) * Mathf.Cos(th), 0.95f + 0.62f * Mathf.Sin(ph), 0.62f * Mathf.Cos(ph) * Mathf.Sin(th));
                var n = new Vector3(p.x / (1.15f * 1.15f), (p.y - 0.95f) / (0.62f * 0.62f), p.z / (0.62f * 0.62f)).normalized;
                spots.Add(BlockoutKit.Sphere(t, "Spot", p - n * 0.2f, s.z * 0.9f, spot, false).GetComponent<Renderer>());
            }

            var legs = new List<Transform>();
            var bodyRenderers = new List<Renderer> { bodyGo.GetComponent<Renderer>() };
            foreach (var p in new[] { new Vector2(-0.65f, -0.28f), new Vector2(-0.65f, 0.28f), new Vector2(0.65f, -0.28f), new Vector2(0.65f, 0.28f) })
            {
                var pv = BlockoutKit.Group(t, "Leg", new Vector3(p.x, 0.62f, p.y));
                bodyRenderers.Add(BlockoutKit.Cyl(pv, "Upper", new Vector3(0f, -0.17f, 0f), 0.09f, 0.34f, body, false).GetComponent<Renderer>());
                bodyRenderers.Add(BlockoutKit.Sphere(pv, "Knee", new Vector3(0f, -0.35f, 0f), 0.075f, body, false).GetComponent<Renderer>());
                bodyRenderers.Add(BlockoutKit.Cyl(pv, "Lower", new Vector3(0f, -0.5f, 0f), 0.057f, 0.28f, body, false).GetComponent<Renderer>());
                BlockoutKit.Cyl(pv, "Hoof", new Vector3(0f, -0.64f, 0f), 0.078f, 0.1f, hoof, false);
                legs.Add(pv);
            }

            bodyRenderers.Add(Ellipsoid(t, "Neck", new Vector3(1.12f, 1.08f, 0f), new Vector3(0.36f, 0.342f, 0.317f), body).GetComponent<Renderer>());
            var head = BlockoutKit.Group(t, "Head", new Vector3(1.4f, 1.14f, 0f));
            bodyRenderers.Add(Ellipsoid(head, "Skull", new Vector3(0.05f, 0f, 0f), new Vector3(0.39f, 0.285f, 0.246f), body).GetComponent<Renderer>());
            Ellipsoid(head, "Snout", new Vector3(0.5f, -0.14f, 0f), new Vector3(0.22f, 0.17f, 0.19f), pink);
            BlockoutKit.Sphere(head, "Nostril", new Vector3(0.69f, -0.1f, 0.07f), 0.035f, nost, false);
            BlockoutKit.Sphere(head, "Nostril", new Vector3(0.69f, -0.1f, -0.07f), 0.035f, nost, false);
            foreach (var zz in new[] { 0.2f, -0.2f })
            {
                BlockoutKit.Sphere(head, "Eye", new Vector3(0.2f, 0.1f, zz * 1.05f), 0.055f, eyeW, false);
                BlockoutKit.Sphere(head, "Pupil", new Vector3(0.245f, 0.1f, zz * 1.2f), 0.035f, eyeB, false);
                var ear = Ellipsoid(head, "Ear", new Vector3(-0.1f, 0.12f, zz * 1.7f), new Vector3(0.055f, 0.028f, 0.14f), spot);
                ear.transform.localEulerAngles = new Vector3(zz > 0 ? -23f : 23f, 0f, 0f);
                spots.Add(ear.GetComponent<Renderer>());
                BlockoutKit.MeshObj(head, "Horn", MeshFactory.Cone(0.045f, 0.22f), new Vector3(-0.02f, 0.2f, zz * 0.62f), horn, false, new Vector3(zz > 0 ? -26f : 26f, 0f, 0f));
            }
            BlockoutKit.Cyl(t, "Collar", new Vector3(1.0f, 1.08f, 0f), 0.42f, 0.3f, collar, false, new Vector3(0f, 0f, -74.5f));
            bodyRenderers.Add(BlockoutKit.Cyl(t, "Tail", new Vector3(-1.22f, 0.95f, 0f), 0.03f, 0.8f, body, false, new Vector3(0f, 0f, 20f)).GetComponent<Renderer>());
            Ellipsoid(t, "TailTuft", new Vector3(-1.36f, 0.58f, 0f), new Vector3(0.075f, 0.12f, 0.075f), spot);
            BlockoutKit.Box(t, "EarTag", new Vector3(1.3f, 1.22f, 0.42f), new Vector3(0.03f, 0.12f, 0.1f), tag, false);

            var udder = BlockoutKit.Group(t, "Udder", new Vector3(-0.3f, 0.62f, 0f));
            BlockoutKit.Sphere(udder, "Bag", Vector3.zero, 0.3f, udderM, false);
            foreach (var p in new[] { new Vector2(-0.1f, -0.1f), new Vector2(0.1f, -0.1f), new Vector2(-0.1f, 0.1f), new Vector2(0.1f, 0.1f) })
                BlockoutKit.Cyl(udder, "Teat", new Vector3(p.x, -0.28f, p.y), 0.038f, 0.16f, teat, false);

            var extra = new List<GameObject>();
            foreach (var p in new[] { new Vector2(0.25f, 0f), new Vector2(0.55f, 0f), new Vector2(-0.65f, 0f), new Vector2(0.05f, 0.3f),
                         new Vector2(0.05f, -0.3f), new Vector2(-0.35f, 0.33f), new Vector2(-0.35f, -0.33f), new Vector2(0.4f, 0.3f) })
            {
                var eu = BlockoutKit.Group(t, "ExtraUdder", new Vector3(p.x, 0.6f, p.y));
                BlockoutKit.Sphere(eu, "Bag", Vector3.zero, 0.2f, udderM, false);
                BlockoutKit.Cyl(eu, "Teat", new Vector3(0f, -0.2f, 0f), 0.033f, 0.14f, teat, false);
                eu.gameObject.SetActive(false);
                extra.Add(eu.gameObject);
            }

            SetupVisuals(root, AnimalKind.Cow, bodyGo.transform, legs, udder, extra, new List<Transform>(), bodyRenderers, spots);
            return root;
        }

        public static GameObject BuildHen()
        {
            var root = new GameObject("Hen");
            var t = root.transform;
            var body = Body("HenBody", "#F7F3EA");
            var comb = M("HenComb", "#D8202A", 0.4f);
            var beakM = M("HenBeak", "#F2A516", 0.4f);
            var eyeW = M("EyeWhite", "#F4EFE3", 0.8f);
            var eyeB = M("EyePupil", "#0A0806", 0.9f);
            var bump = M("HenBump", "#FF9FB5", 0.4f);

            var bodyGo = Ellipsoid(t, "Body", new Vector3(0f, 0.95f, 0f), new Vector3(0.6325f, 0.5225f, 0.4675f), body);
            var bodyRenderers = new List<Renderer> { bodyGo.GetComponent<Renderer>() };
            var head = BlockoutKit.Group(t, "Head", new Vector3(0.55f, 1.55f, 0f));
            bodyRenderers.Add(BlockoutKit.Sphere(head, "Skull", Vector3.zero, 0.26f, body, false).GetComponent<Renderer>());
            foreach (var o in new[] { new Vector2(0f, 0.29f), new Vector2(0.1f, 0.25f), new Vector2(-0.1f, 0.25f) })
                BlockoutKit.Sphere(head, "Comb", new Vector3(o.x, o.y, 0f), 0.08f, comb, false);
            Ellipsoid(head, "Wattle", new Vector3(0.22f, -0.16f, 0f), new Vector3(0.048f, 0.084f, 0.048f), comb);
            BlockoutKit.MeshObj(head, "Beak", MeshFactory.Cone(0.07f, 0.2f), new Vector3(0.21f, -0.02f, 0f), beakM, false, new Vector3(0f, 0f, -90f));
            foreach (var zz in new[] { 0.12f, -0.12f })
            {
                BlockoutKit.Sphere(head, "Eye", new Vector3(0.17f, 0.06f, zz), 0.05f, eyeW, false);
                BlockoutKit.Sphere(head, "Pupil", new Vector3(0.21f, 0.06f, zz * 1.15f), 0.03f, eyeB, false);
            }
            var wings = new List<Transform>();
            foreach (var zz in new[] { 0.47f, -0.47f })
            {
                var w = Ellipsoid(t, "Wing", new Vector3(-0.05f, 0.98f, zz), new Vector3(0.385f, 0.21f, 0.0875f), body);
                bodyRenderers.Add(w.GetComponent<Renderer>());
                wings.Add(w.transform);
            }
            bodyRenderers.Add(BlockoutKit.MeshObj(t, "Tail", MeshFactory.Cone(0.22f, 0.5f), new Vector3(-0.72f, 1.05f, 0f), body, false, new Vector3(0f, 0f, 51.6f)).GetComponent<Renderer>());
            var legs = new List<Transform>();
            foreach (var zz in new[] { 0.15f, -0.15f })
            {
                var pv = BlockoutKit.Group(t, "Leg", new Vector3(0.05f, 0.5f, zz));
                BlockoutKit.Cyl(pv, "Shank", new Vector3(0f, -0.07f, 0f), 0.035f, 0.42f, beakM, false);
                BlockoutKit.Box(pv, "Foot", new Vector3(0.05f, -0.28f, 0f), new Vector3(0.22f, 0.03f, 0.12f), beakM, false);
                legs.Add(pv);
            }
            var extra = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                var eu = BlockoutKit.Sphere(t, "Growth", new Vector3(-0.5f + (i % 3) * 0.14f, 0.6f + (i / 3) * 0.1f, i % 2 == 1 ? 0.2f : -0.2f), 0.075f, bump, false);
                eu.SetActive(false);
                extra.Add(eu);
            }
            SetupVisuals(root, AnimalKind.Hen, bodyGo.transform, legs, null, extra, wings, bodyRenderers, new List<Renderer>());
            return root;
        }

        /// <summary>Kugel mit Halbachsen (radius je Achse).</summary>
        static GameObject Ellipsoid(Transform parent, string name, Vector3 center, Vector3 radii, Material m)
        {
            var go = BlockoutKit.Sphere(parent, name, center, 0.5f, m, false);
            go.transform.localScale = radii * 2f;
            return go;
        }

        static void SetupVisuals(GameObject root, AnimalKind kind, Transform body, List<Transform> legs, Transform udder,
            List<GameObject> extra, List<Transform> wings, List<Renderer> bodyRenderers, List<Renderer> spots)
        {
            var v = root.AddComponent<AnimalVisuals>();
            var so = new SerializedObject(v);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("body").objectReferenceValue = body;
            so.FindProperty("udder").objectReferenceValue = udder;
            Fill(so.FindProperty("legs"), legs);
            Fill(so.FindProperty("extraUdders"), extra);
            Fill(so.FindProperty("wings"), wings);
            Fill(so.FindProperty("bodyRenderers"), bodyRenderers);
            Fill(so.FindProperty("spotRenderers"), spots);
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        static void Fill<T>(SerializedProperty p, List<T> items) where T : Object
        {
            p.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
