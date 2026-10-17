using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using S = PlanErfuellt.EditorTools.SdfMesher;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Kuh und Huhn als glatte, organische Modelle aus Abstandsfeldern (SdfMesher), Maße wie im Prototyp
    /// (buildCowModel/buildChickenModel). Blick in +X (Laufrichtung auf dem Band).
    /// Bewegliche Teile bleiben eigene Objekte (Körper, Kopf, Beine, Euter, Zusatz-Euter, Flügel), damit
    /// AnimalVisuals wachsen, aufblähen, laufen und flattern lassen kann. Flecken sind eine angehobene Schale auf dem Körper.
    /// </summary>
    public static class AnimalBuilder
    {
        public const string CowPath = "Assets/_Project/Prefabs/Animals/Cow.prefab";
        public const string HenPath = "Assets/_Project/Prefabs/Animals/Hen.prefab";
        const string MeshDir = "Assets/_Project/Art/Models/Animals";

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

        // ================= Kuh =================

        static readonly Vector3 CowBodyPivot = new Vector3(0f, 1.0f, 0f);
        static readonly Vector3 CowHeadPivot = new Vector3(1.42f, 1.22f, 0f);

        /// <summary>Rumpf mit Brust, Hüfte, Bauch, Widerrist, Hüfthöckern, Schwanzansatz, Hals und Wamme (Modellraum).</summary>
        static float CowTorso(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, 1.0f, 0f), new Vector3(1.0f, 0.48f, 0.46f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.55f, 0.98f, 0f), new Vector3(0.5f, 0.5f, 0.44f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.62f, 1.06f, 0f), new Vector3(0.46f, 0.46f, 0.45f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.05f, 0.86f, 0f), new Vector3(0.72f, 0.38f, 0.47f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.62f, 1.38f, 0f), new Vector3(0.32f, 0.14f, 0.22f)), 0.1f);
            d = S.SMin(d, S.Sphere(p, new Vector3(-0.72f, 1.44f, 0.26f), 0.11f), 0.1f);
            d = S.SMin(d, S.Sphere(p, new Vector3(-0.72f, 1.44f, -0.26f), 0.11f), 0.1f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-1.02f, 1.36f, 0f), new Vector3(0.16f, 0.1f, 0.12f)), 0.1f);
            d = S.SMin(d, S.Capsule(p, new Vector3(0.75f, 1.12f, 0f), new Vector3(1.3f, 1.26f, 0f), 0.33f, 0.22f), 0.15f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.98f, 0.92f, 0f), new Vector3(0.25f, 0.22f, 0.12f)), 0.12f);
            return d;
        }

        /// <summary>Holstein-Flecken: wenige große Kleckse mit unruhigem Rand.</summary>
        static float CowSpots(Vector3 p)
        {
            var blobs = new[]
            {
                new Vector4(0.2f, 1.35f, 0.32f, 0.34f), new Vector4(-0.5f, 1.2f, 0.45f, 0.32f), new Vector4(0.55f, 1.0f, -0.42f, 0.3f),
                new Vector4(-0.2f, 0.95f, -0.46f, 0.36f), new Vector4(0.95f, 1.32f, 0.16f, 0.2f), new Vector4(-0.85f, 1.3f, -0.22f, 0.26f),
                new Vector4(0.1f, 0.78f, 0.42f, 0.2f), new Vector4(-0.95f, 1.0f, 0.3f, 0.22f), new Vector4(0.45f, 1.48f, -0.1f, 0.2f)
            };
            float d = float.MaxValue;
            foreach (var b in blobs) d = Mathf.Min(d, (p - (Vector3)b).magnitude - b.w);
            return d + Wobble(p) * 0.09f;
        }

        static float Wobble(Vector3 p) =>
            Mathf.PerlinNoise(p.x * 3.1f + p.z * 1.7f + 11f, p.y * 3.3f + 5f) + Mathf.PerlinNoise(p.z * 2.9f + 3f, p.x * 2.3f + p.y * 1.9f) - 1f;

        /// <summary>Kopf (Kopfraum): Schädel, Stirn, Gesicht, Kiefer.</summary>
        static float CowHead(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0.02f, 0.03f, 0f), new Vector3(0.28f, 0.24f, 0.21f));
            d = S.SMin(d, S.Capsule(p, new Vector3(0.05f, 0f, 0f), new Vector3(0.5f, -0.16f, 0f), 0.19f, 0.14f), 0.08f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.26f, -0.18f, 0f), new Vector3(0.26f, 0.1f, 0.13f)), 0.08f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.08f, 0.2f, 0f), new Vector3(0.12f, 0.08f, 0.18f)), 0.08f);
            // Augenbögen
            d = S.SMin(d, S.Sphere(p, new Vector3(0.12f, 0.1f, 0.15f), 0.08f), 0.06f);
            d = S.SMin(d, S.Sphere(p, new Vector3(0.12f, 0.1f, -0.15f), 0.08f), 0.06f);
            return d;
        }

        static float CowHeadSpots(Vector3 p) =>
            Mathf.Min(S.Sphere(p, new Vector3(0.02f, 0.14f, 0.16f), 0.19f), S.Sphere(p, new Vector3(-0.08f, 0.08f, -0.2f), 0.15f)) + Wobble(p * 2f) * 0.05f;

        static float CowLeg(Vector3 p, bool rear)
        {
            if (rear)
            {
                float d = S.Capsule(p, new Vector3(0f, 0.12f, 0f), new Vector3(-0.06f, -0.28f, 0f), 0.16f, 0.09f);
                d = S.SMin(d, S.Sphere(p, new Vector3(-0.06f, -0.3f, 0f), 0.075f), 0.05f);
                d = S.SMin(d, S.Capsule(p, new Vector3(-0.06f, -0.3f, 0f), new Vector3(0f, -0.56f, 0f), 0.06f, 0.056f), 0.05f);
                return S.SMin(d, S.Sphere(p, new Vector3(0f, -0.58f, 0f), 0.064f), 0.04f);
            }
            float f = S.Capsule(p, new Vector3(0f, 0.08f, 0f), new Vector3(0.02f, -0.3f, 0f), 0.13f, 0.085f);
            f = S.SMin(f, S.Sphere(p, new Vector3(0.02f, -0.33f, 0f), 0.078f), 0.05f);
            f = S.SMin(f, S.Capsule(p, new Vector3(0.02f, -0.33f, 0f), new Vector3(0.01f, -0.56f, 0f), 0.064f, 0.056f), 0.05f);
            return S.SMin(f, S.Sphere(p, new Vector3(0.01f, -0.58f, 0f), 0.064f), 0.04f);
        }

        static float CowUdder(Vector3 p, float s)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, -0.02f, 0f) * s, new Vector3(0.3f, 0.2f, 0.26f) * s);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.1f, -0.07f, 0f) * s, new Vector3(0.18f, 0.17f, 0.22f) * s), 0.08f * s);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.12f, -0.06f, 0f) * s, new Vector3(0.18f, 0.17f, 0.22f) * s), 0.08f * s);
            foreach (var t in new[] { new Vector2(0.11f, 0.1f), new Vector2(0.11f, -0.1f), new Vector2(-0.11f, 0.1f), new Vector2(-0.11f, -0.1f) })
                d = S.SMin(d, S.Capsule(p, new Vector3(t.x, -0.15f, t.y) * s, new Vector3(t.x * 1.08f, -0.34f, t.y * 1.08f) * s, 0.045f * s, 0.032f * s), 0.05f * s);
            return d;
        }

        static float SmallUdder(Vector3 p)
        {
            float d = S.Ellipsoid(p, Vector3.zero, new Vector3(0.19f, 0.15f, 0.17f));
            return S.SMin(d, S.Capsule(p, new Vector3(0f, -0.1f, 0f), new Vector3(0f, -0.27f, 0f), 0.04f, 0.03f), 0.04f);
        }

        public static GameObject BuildCow()
        {
            var root = new GameObject("Cow");
            var t = root.transform;
            var body = Body("CowBody", "#F4F1EA");
            var spot = M("CowSpot", "#1C1A19", 0.15f);
            var pink = M("CowPink", "#EFA7B4", 0.35f);
            var udderM = M("CowUdder", "#F5A0B2", 0.4f);
            var hoof = M("CowHoof", "#2A211C", 0.45f);
            var nost = M("CowNostril", "#4A2228", 0.6f);
            var eyeW = M("EyeWhite", "#F4EFE3", 0.8f);
            var eyeB = M("EyePupil", "#0A0806", 0.9f);
            var horn = M("CowHorn", "#E2D4B4", 0.5f);
            var collar = M("CowCollar", "#C01A1A", 0.25f);
            var tag = M("EarTag", "#F2B705", 0.4f);

            var bodyRenderers = new List<Renderer>();
            var spots = new List<Renderer>();

            // Rumpf mit Hals und Fleckenschale
            var torsoBounds = new Bounds(new Vector3(0.15f, 1.0f, 0f), new Vector3(3.0f, 1.4f, 1.25f));
            var bodyGo = Part(t, "Body", CowBodyPivot, BuildMesh("Cow_Body", CowTorso, torsoBounds, 0.022f, CowBodyPivot), body, bodyRenderers);
            // Flecken: Schnittmenge aus leicht vergrößertem Rumpf und Fleckfeld → glatte Ränder
            Part(bodyGo.transform, "Spots", Vector3.zero, BuildMesh("Cow_BodySpots", p => Mathf.Max(CowTorso(p) - 0.005f, CowSpots(p)), torsoBounds, 0.016f, CowBodyPivot), spot, spots, true);
            LookDevKit.Obj(bodyGo.transform, "Collar", LookDevKit.Ring(0.26f, 0.31f, 0.1f, 32), new Vector3(1.02f, 1.2f, 0f) - CowBodyPivot, collar, new Vector3(0f, 0f, -76f), false);

            // Kopf
            var head = BlockoutKit.Group(t, "Head", CowHeadPivot);
            var headBounds = new Bounds(new Vector3(0.18f, -0.02f, 0f), new Vector3(1.0f, 0.75f, 0.55f));
            Part(head, "Skull", Vector3.zero, BuildMesh("Cow_Head", CowHead, headBounds, 0.014f, Vector3.zero), body, bodyRenderers);
            Part(head, "HeadSpots", Vector3.zero, BuildMesh("Cow_HeadSpots", p => Mathf.Max(CowHead(p) - 0.004f, CowHeadSpots(p)), headBounds, 0.01f, Vector3.zero), spot, spots, true);
            Part(head, "Muzzle", Vector3.zero, BuildMesh("Cow_Muzzle", p => S.Ellipsoid(p, new Vector3(0.6f, -0.2f, 0f), new Vector3(0.12f, 0.12f, 0.16f)),
                new Bounds(new Vector3(0.6f, -0.2f, 0f), new Vector3(0.32f, 0.32f, 0.4f)), 0.014f, Vector3.zero), pink, null);
            foreach (float z in new[] { 0.07f, -0.07f })
                Part(head, "Nostril", Vector3.zero, BuildMesh("Cow_Nostril" + (z > 0 ? "L" : "R"), p => S.Ellipsoid(p, new Vector3(0.71f, -0.17f, z), new Vector3(0.022f, 0.036f, 0.026f)),
                    new Bounds(new Vector3(0.71f, -0.17f, z), Vector3.one * 0.12f), 0.006f, Vector3.zero), nost, null);
            foreach (float zz in new[] { 1f, -1f })
            {
                LookDevKit.Obj(head, "Eye", SphereMesh(0.045f), new Vector3(0.15f, 0.07f, 0.19f * zz), eyeW, null, false);
                LookDevKit.Obj(head, "Pupil", SphereMesh(0.03f), new Vector3(0.18f, 0.075f, 0.215f * zz), eyeB, null, false);
                var earRot = Quaternion.Euler(-22f * zz, -12f * zz, 0f);
                Part(head, "Ear", Vector3.zero, BuildMesh("Cow_Ear" + (zz > 0 ? "L" : "R"), p => S.Ellipsoid(p, new Vector3(-0.08f, 0.1f, 0.27f * zz), new Vector3(0.065f, 0.032f, 0.15f), earRot),
                    new Bounds(new Vector3(-0.08f, 0.1f, 0.27f * zz), Vector3.one * 0.42f), 0.008f, Vector3.zero), spot, spots);
                LookDevKit.Obj(head, "Horn", LookDevKit.Lathe("CowHorn", new[] { new Vector2(0.045f, 0f), new Vector2(0.042f, 0.06f), new Vector2(0.032f, 0.13f), new Vector2(0.016f, 0.19f), new Vector2(0f, 0.22f) }, 16),
                    new Vector3(-0.06f, 0.2f, 0.15f * zz), horn, new Vector3(-38f * zz, 0f, -10f), false);
            }
            LookDevKit.Box(head, "EarTag", new Vector3(-0.07f, 0.03f, 0.39f), new Vector3(0.05f, 0.06f, 0.01f), tag, new Vector3(-22f, -12f, 0f), false);

            // Beine (Drehpunkt an der Hüfte) mit Hufen
            var legs = new List<Transform>();
            foreach (var p in new[] { new Vector3(0.6f, 0.72f, -0.24f), new Vector3(0.6f, 0.72f, 0.24f), new Vector3(-0.62f, 0.72f, -0.25f), new Vector3(-0.62f, 0.72f, 0.25f) })
            {
                bool rear = p.x < 0f;
                var pv = BlockoutKit.Group(t, "Leg", p);
                Part(pv, "LegMesh", Vector3.zero, BuildMesh(rear ? "Cow_LegRear" : "Cow_LegFront", q => CowLeg(q, rear),
                    new Bounds(new Vector3(0f, -0.25f, 0f), new Vector3(0.45f, 0.85f, 0.4f)), 0.012f, Vector3.zero), body, bodyRenderers);
                LookDevKit.Obj(pv, "Hoof", LookDevKit.Lathe("CowHoof", new[] { new Vector2(0f, 0f), new Vector2(0.075f, 0f), new Vector2(0.074f, 0.03f), new Vector2(0.066f, 0.1f), new Vector2(0.058f, 0.12f), new Vector2(0f, 0.12f) }, 20),
                    new Vector3(rear ? 0f : 0.01f, -0.72f, 0f), hoof, null, false);
                legs.Add(pv);
            }

            // Schwanz mit Quaste
            bodyRenderers.Add(LookDevKit.Obj(bodyGo.transform, "Tail", LookDevKit.Tube("CowTail", new Vector3(-1.1f, 1.38f, 0f) - CowBodyPivot, new Vector3(-1.24f, 1.25f, 0f) - CowBodyPivot,
                new Vector3(-1.24f, 0.95f, 0f) - CowBodyPivot, new Vector3(-1.21f, 0.66f, 0f) - CowBodyPivot, 0.028f, 20, 10), Vector3.zero, body, null, false).GetComponent<Renderer>());
            Part(bodyGo.transform, "TailTuft", Vector3.zero, BuildMesh("Cow_TailTuft", p => S.Ellipsoid(p, new Vector3(-1.21f, 0.56f, 0f), new Vector3(0.07f, 0.15f, 0.07f)) + Wobble(p * 6f) * 0.02f,
                new Bounds(new Vector3(-1.21f, 0.56f, 0f), new Vector3(0.3f, 0.45f, 0.3f)), 0.01f, CowBodyPivot), spot, spots);

            // Euter und Zusatz-Euter
            var udder = BlockoutKit.Group(t, "Udder", new Vector3(-0.3f, 0.66f, 0f));
            Part(udder, "Bag", Vector3.zero, BuildMesh("Cow_Udder", p => CowUdder(p, 1f), new Bounds(new Vector3(0f, -0.12f, 0f), new Vector3(0.75f, 0.6f, 0.65f)), 0.014f, Vector3.zero), udderM, null);
            var extra = new List<GameObject>();
            var small = BuildMesh("Cow_UdderSmall", SmallUdder, new Bounds(new Vector3(0f, -0.08f, 0f), new Vector3(0.5f, 0.5f, 0.45f)), 0.016f, Vector3.zero);
            foreach (var p in new[] { new Vector2(0.25f, 0f), new Vector2(0.55f, 0f), new Vector2(-0.65f, 0f), new Vector2(0.05f, 0.3f),
                         new Vector2(0.05f, -0.3f), new Vector2(-0.35f, 0.33f), new Vector2(-0.35f, -0.33f), new Vector2(0.4f, 0.3f) })
            {
                var eu = BlockoutKit.Group(t, "ExtraUdder", new Vector3(p.x, 0.62f, p.y));
                Part(eu, "Bag", Vector3.zero, small, udderM, null);
                eu.gameObject.SetActive(false);
                extra.Add(eu.gameObject);
            }

            SetupVisuals(root, AnimalKind.Cow, bodyGo.transform, legs, udder, extra, new List<Transform>(), bodyRenderers, spots);
            return root;
        }

        // ================= Huhn =================

        static readonly Vector3 HenBodyPivot = new Vector3(0f, 0.95f, 0f);
        static readonly Vector3 HenHeadPivot = new Vector3(0.55f, 1.55f, 0f);

        static float HenBody(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, 0.95f, 0f), new Vector3(0.58f, 0.48f, 0.44f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.28f, 0.9f, 0f), new Vector3(0.38f, 0.42f, 0.38f)), 0.12f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.25f, 1.1f, 0f), new Vector3(0.4f, 0.35f, 0.36f)), 0.12f);
            d = S.SMin(d, S.Capsule(p, new Vector3(0.3f, 1.15f, 0f), new Vector3(0.52f, 1.48f, 0f), 0.24f, 0.15f), 0.12f);
            // Schwanzfedern als Fächer
            for (int i = 0; i < 5; i++)
            {
                float z = (i - 2) * 0.075f;
                var rot = Quaternion.Euler((i - 2) * 10f, 0f, 40f - Mathf.Abs(i - 2) * 6f);
                d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.62f, 1.38f - Mathf.Abs(i - 2) * 0.04f, z), new Vector3(0.09f, 0.3f, 0.045f), rot), 0.06f);
            }
            return d + Wobble(p * 5f) * 0.008f;
        }

        static float HenHead(Vector3 p)
        {
            float d = S.Sphere(p, Vector3.zero, 0.2f);
            return S.SMin(d, S.Ellipsoid(p, new Vector3(0.07f, -0.06f, 0f), new Vector3(0.17f, 0.14f, 0.15f)), 0.06f);
        }

        static float HenComb(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(-0.13f, 0.17f, 0f), new Vector3(0.06f, 0.07f, 0.03f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.05f, 0.23f, 0f), new Vector3(0.07f, 0.08f, 0.032f)), 0.03f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.04f, 0.25f, 0f), new Vector3(0.075f, 0.085f, 0.034f)), 0.03f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.12f, 0.2f, 0f), new Vector3(0.06f, 0.07f, 0.03f)), 0.03f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.16f, -0.15f, 0.03f), new Vector3(0.03f, 0.065f, 0.026f)), 0.02f);
            return S.SMin(d, S.Ellipsoid(p, new Vector3(0.16f, -0.15f, -0.03f), new Vector3(0.03f, 0.065f, 0.026f)), 0.02f);
        }

        static float HenWing(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(-0.18f, -0.04f, 0f), new Vector3(0.36f, 0.2f, 0.07f));
            return S.SMin(d, S.Ellipsoid(p, new Vector3(-0.44f, -0.12f, 0f), new Vector3(0.17f, 0.1f, 0.05f)), 0.06f) + Wobble(p * 7f) * 0.006f;
        }

        static float HenLeg(Vector3 p)
        {
            float d = S.Capsule(p, new Vector3(0f, 0.05f, 0f), new Vector3(0f, -0.25f, 0f), 0.045f, 0.035f);
            var ankle = new Vector3(0f, -0.27f, 0f);
            foreach (var toe in new[] { new Vector3(0.17f, -0.29f, 0f), new Vector3(0.13f, -0.29f, 0.08f), new Vector3(0.13f, -0.29f, -0.08f), new Vector3(-0.08f, -0.28f, 0f) })
                d = S.SMin(d, S.Capsule(p, ankle, toe, 0.026f, 0.016f), 0.02f);
            return d;
        }

        public static GameObject BuildHen()
        {
            var root = new GameObject("Hen");
            var t = root.transform;
            var body = Body("HenBody", "#F6F1E4");
            var comb = M("HenComb", "#D21E28", 0.45f);
            var beakM = M("HenBeak", "#EFA01A", 0.45f);
            var eyeW = M("EyeWhite", "#F4EFE3", 0.8f);
            var eyeB = M("EyePupil", "#0A0806", 0.9f);
            var bump = M("HenBump", "#FF9FB5", 0.4f);
            var bodyRenderers = new List<Renderer>();

            var bodyGo = Part(t, "Body", HenBodyPivot, BuildMesh("Hen_Body", HenBody, new Bounds(new Vector3(-0.05f, 1.1f, 0f), new Vector3(1.6f, 1.2f, 1.05f)), 0.02f, HenBodyPivot), body, bodyRenderers);

            var head = BlockoutKit.Group(t, "Head", HenHeadPivot);
            Part(head, "Skull", Vector3.zero, BuildMesh("Hen_Head", HenHead, new Bounds(new Vector3(0.03f, -0.02f, 0f), new Vector3(0.6f, 0.55f, 0.5f)), 0.013f, Vector3.zero), body, bodyRenderers);
            Part(head, "Comb", Vector3.zero, BuildMesh("Hen_Comb", HenComb, new Bounds(new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.6f, 0.2f)), 0.008f, Vector3.zero), comb, null);
            LookDevKit.Obj(head, "Beak", LookDevKit.Lathe("HenBeak", new[] { new Vector2(0.065f, 0f), new Vector2(0.06f, 0.05f), new Vector2(0.035f, 0.12f), new Vector2(0f, 0.18f) }, 16),
                new Vector3(0.17f, -0.01f, 0f), beakM, new Vector3(0f, 0f, -98f), false);
            foreach (float zz in new[] { 1f, -1f })
            {
                LookDevKit.Obj(head, "Eye", SphereMesh(0.045f), new Vector3(0.13f, 0.05f, 0.135f * zz), eyeW, null, false);
                LookDevKit.Obj(head, "Pupil", SphereMesh(0.028f), new Vector3(0.155f, 0.055f, 0.16f * zz), eyeB, null, false);
            }

            var wings = new List<Transform>();
            var wingMesh = BuildMesh("Hen_Wing", HenWing, new Bounds(new Vector3(-0.25f, -0.06f, 0f), new Vector3(0.95f, 0.55f, 0.25f)), 0.014f, Vector3.zero);
            foreach (float zz in new[] { 0.44f, -0.44f })
            {
                var w = Part(t, "Wing", new Vector3(0.14f, 1.05f, zz), wingMesh, body, bodyRenderers);
                wings.Add(w.transform);
            }

            var legs = new List<Transform>();
            var legMesh = BuildMesh("Hen_Leg", HenLeg, new Bounds(new Vector3(0.04f, -0.12f, 0f), new Vector3(0.4f, 0.5f, 0.3f)), 0.008f, Vector3.zero);
            foreach (float zz in new[] { 0.15f, -0.15f })
            {
                var pv = BlockoutKit.Group(t, "Leg", new Vector3(0.05f, 0.5f, zz));
                Part(pv, "LegMesh", Vector3.zero, legMesh, beakM, null);
                legs.Add(pv);
            }

            var extra = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                var eu = LookDevKit.Obj(t, "Growth", SphereMesh(0.075f), new Vector3(-0.5f + (i % 3) * 0.14f, 0.6f + (i / 3) * 0.1f, i % 2 == 1 ? 0.2f : -0.2f), bump, null, false);
                eu.SetActive(false);
                extra.Add(eu);
            }
            SetupVisuals(root, AnimalKind.Hen, bodyGo.transform, legs, null, extra, wings, bodyRenderers, new List<Renderer>());
            return root;
        }

        // ================= Hilfen =================

        static Mesh BuildMesh(string name, SdfMesher.Field f, Bounds b, float cell, Vector3 origin, System.Func<Vector3, bool> keep = null, float offset = 0f) =>
            SdfMesher.Save(SdfMesher.Build(name, f, b, cell, origin, keep, offset), $"{MeshDir}/{name}.asset");

        /// <summary>Glatte Kugel als Drehkörper (Augen, Wucherungen).</summary>
        static Mesh SphereMesh(float r)
        {
            var prof = new Vector2[13];
            for (int i = 0; i < prof.Length; i++)
            {
                float a = i / (float)(prof.Length - 1) * Mathf.PI;
                prof[i] = new Vector2(Mathf.Sin(a) * r, -Mathf.Cos(a) * r);
            }
            return LookDevKit.Lathe("Sphere_" + r.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture), prof, 20);
        }

        static GameObject Part(Transform parent, string name, Vector3 pos, Mesh mesh, Material mat, List<Renderer> list, bool noShadow = false)
        {
            var go = LookDevKit.Obj(parent, name, mesh, pos, mat, null, false, !noShadow);
            list?.Add(go.GetComponent<Renderer>());
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
                if (r.name != "Spots" && r.name != "HeadSpots") r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            foreach (var tr in root.GetComponentsInChildren<Transform>(true)) BlockoutKit.SetStatic(tr.gameObject, false);
        }

        static void Fill<T>(SerializedProperty p, List<T> items) where T : Object
        {
            p.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
