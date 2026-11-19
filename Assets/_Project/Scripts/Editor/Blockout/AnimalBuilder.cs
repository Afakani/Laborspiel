using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using S = PlanErfuellt.EditorTools.SdfMesher;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Kuh und Huhn mit Skelett (Art-Pass, Bild 07/08): eine durchgehende, glatte Haut aus Abstandsfeldern (SdfMesher),
    /// gehäutet auf Knochen (Rumpf, Hals, Kopf, Ohren, Schwanz, Beine mit Knie, Flügel). Mutationen über Blendshapes
    /// (Aufblähen, Atmen) und Knochen (Euter wächst). Kleine harte Teile (Augen, Hörner, Hufe, Kamm, Schnabel)
    /// hängen als eigene Meshes an den Knochen. Maße wie im Prototyp (buildCowModel/buildChickenModel), Blick in +X.
    /// </summary>
    public static class AnimalBuilder
    {
        public const string CowPath = "Assets/_Project/Prefabs/Animals/Cow.prefab";
        public const string HenPath = "Assets/_Project/Prefabs/Animals/Hen.prefab";
        const string MeshDir = "Assets/_Project/Art/Models/Animals";
        /// <summary>Breite des weichen Übergangs zwischen Knochen-Zonen in m.</summary>
        const float SkinSigma = 0.035f;
        static readonly string[] Obsolete = { "Cow_Body", "Cow_BodySpots", "Cow_Head", "Cow_HeadSpots", "Cow_LegRear", "Cow_LegFront", "Hen_Body", "Hen_Head" };

        public static GameObject LoadOrBuild(string path, bool rebuild, System.Func<GameObject> create)
        {
            BlockoutKit.EnsureFolder(System.IO.Path.GetDirectoryName(path));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && !rebuild) return prefab;
            foreach (var n in Obsolete)
                if (AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/{n}.asset") != null) AssetDatabase.DeleteAsset($"{MeshDir}/{n}.asset");
            var temp = create();
            prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        static Material Body(string name, string hex) => BlockoutKit.Emissive(name, LabMaterials.Hex(hex), 0f, false);
        static Material M(string name, string hex, float smooth = 0.25f) => BlockoutKit.Mat(name, LabMaterials.Hex(hex), 0f, smooth);

        // ================= Kuh =================

        static readonly Vector3 CowSpine = new Vector3(0f, 1.0f, 0f);
        static readonly Vector3 CowNeckPos = new Vector3(0.82f, 1.14f, 0f);
        static readonly Vector3 CowHeadPos = new Vector3(1.42f, 1.22f, 0f);
        static readonly Vector3[] CowTailPos = { new Vector3(-1.08f, 1.38f, 0f), new Vector3(-1.22f, 1.22f, 0f), new Vector3(-1.24f, 0.94f, 0f) };
        static readonly Vector3 CowTailEnd = new Vector3(-1.21f, 0.66f, 0f);
        static readonly Vector3[] CowHips = { new Vector3(0.6f, 0.72f, -0.24f), new Vector3(0.6f, 0.72f, 0.24f), new Vector3(-0.62f, 0.72f, -0.25f), new Vector3(-0.62f, 0.72f, 0.25f) };
        static readonly Vector3 KneeFront = new Vector3(0.02f, -0.33f, 0f);
        static readonly Vector3 KneeRear = new Vector3(-0.06f, -0.3f, 0f);
        static readonly Vector3 CowUdderPos = new Vector3(-0.3f, 0.66f, 0f);
        static readonly Vector3 CowNeckA = new Vector3(0.75f, 1.12f, 0f), CowNeckB = new Vector3(1.3f, 1.26f, 0f);
        /// <summary>Aufblähen des Rumpfs je Achse (längs weniger, damit Hals und Kopf nicht verschluckt werden).</summary>
        static readonly Vector3 CowInflAxis = new Vector3(0.6f, 1f, 1f);

        // Knochen-Indizes der Kuh-Haut
        const int CSpine = 0, CNeck = 1, CHead = 2, CTail = 3, CLeg = 6, CShin = 10;

        /// <summary>Rumpf mit Brust, Hüfte, Bauch, Widerrist, Hüfthöckern und Schwanzansatz (ohne Hals).</summary>
        static float CowTrunk(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, 1.0f, 0f), new Vector3(1.0f, 0.48f, 0.46f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.55f, 0.98f, 0f), new Vector3(0.5f, 0.5f, 0.44f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.62f, 1.06f, 0f), new Vector3(0.46f, 0.46f, 0.45f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.05f, 0.86f, 0f), new Vector3(0.72f, 0.38f, 0.47f)), 0.18f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.62f, 1.38f, 0f), new Vector3(0.32f, 0.14f, 0.22f)), 0.1f);
            d = S.SMin(d, S.Sphere(p, new Vector3(-0.72f, 1.44f, 0.26f), 0.11f), 0.1f);
            d = S.SMin(d, S.Sphere(p, new Vector3(-0.72f, 1.44f, -0.26f), 0.11f), 0.1f);
            return S.SMin(d, S.Ellipsoid(p, new Vector3(-1.02f, 1.36f, 0f), new Vector3(0.16f, 0.1f, 0.12f)), 0.1f);
        }

        /// <summary>Hals mit Wamme.</summary>
        static float CowNeck(Vector3 p) =>
            S.SMin(S.Capsule(p, CowNeckA, CowNeckB, 0.33f, 0.22f),
                S.Ellipsoid(p, new Vector3(0.98f, 0.92f, 0f), new Vector3(0.25f, 0.22f, 0.12f)), 0.12f);

        /// <summary>Kopf (Kopfraum): Schädel, Stirn, Gesicht, Kiefer, Augenbögen.</summary>
        static float CowHead(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0.02f, 0.03f, 0f), new Vector3(0.28f, 0.24f, 0.21f));
            d = S.SMin(d, S.Capsule(p, new Vector3(0.05f, 0f, 0f), new Vector3(0.5f, -0.16f, 0f), 0.19f, 0.14f), 0.08f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.26f, -0.18f, 0f), new Vector3(0.26f, 0.1f, 0.13f)), 0.08f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.08f, 0.2f, 0f), new Vector3(0.12f, 0.08f, 0.18f)), 0.08f);
            d = S.SMin(d, S.Sphere(p, new Vector3(0.12f, 0.1f, 0.15f), 0.08f), 0.06f);
            return S.SMin(d, S.Sphere(p, new Vector3(0.12f, 0.1f, -0.15f), 0.08f), 0.06f);
        }

        /// <summary>Oberschenkel bis Knie (Hüftraum), oben tief im Rumpf verankert.</summary>
        static float CowLegUpper(Vector3 q, bool rear) => rear
            ? S.Capsule(q, new Vector3(0f, 0.18f, 0f), KneeRear, 0.16f, 0.09f)
            : S.Capsule(q, new Vector3(0f, 0.16f, 0f), KneeFront, 0.13f, 0.085f);

        /// <summary>Knie, Unterschenkel und Fessel (Hüftraum).</summary>
        static float CowLegLower(Vector3 q, bool rear)
        {
            var k = rear ? KneeRear : KneeFront;
            var foot = rear ? new Vector3(0f, -0.56f, 0f) : new Vector3(0.01f, -0.56f, 0f);
            float d = S.Sphere(q, k, rear ? 0.075f : 0.078f);
            d = S.SMin(d, S.Capsule(q, k, foot, rear ? 0.06f : 0.064f, 0.056f), 0.05f);
            return S.SMin(d, S.Sphere(q, foot - new Vector3(0f, 0.02f, 0f), 0.064f), 0.04f);
        }

        static float CowTailSeg(Vector3 p, int i)
        {
            var a = CowTailPos[i];
            var b = i < 2 ? CowTailPos[i + 1] : CowTailEnd;
            return S.Capsule(p, a, b, 0.042f - i * 0.005f, 0.036f - i * 0.004f);
        }

        static float CowSkin(Vector3 p)
        {
            float d = S.SMin(S.SMin(CowTrunk(p), CowNeck(p), 0.15f), CowHead(p - CowHeadPos), 0.08f);
            float legs = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                var q = p - CowHips[i];
                bool rear = i >= 2;
                legs = Mathf.Min(legs, S.SMin(CowLegUpper(q, rear), CowLegLower(q, rear), 0.05f));
            }
            d = S.SMin(d, legs, 0.07f);
            float tail = Mathf.Min(CowTailSeg(p, 0), Mathf.Min(CowTailSeg(p, 1), CowTailSeg(p, 2)));
            return S.SMin(d, tail, 0.04f);
        }

        /// <summary>Holstein-Flecken auf Rumpf und Kopf: wenige große Kleckse mit unruhigem Rand.</summary>
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
            d += Wobble(p) * 0.09f;
            var h = p - CowHeadPos;
            float head = Mathf.Min(S.Sphere(h, new Vector3(0.02f, 0.14f, 0.16f), 0.19f), S.Sphere(h, new Vector3(-0.08f, 0.08f, -0.2f), 0.15f)) + Wobble(h * 2f) * 0.05f;
            return Mathf.Min(d, head);
        }

        /// <summary>Fleckenschale: Schnitt aus minimal vergrößerter Haut und Fleckfeld (weit weg nur das Fleckfeld, spart Zeit).</summary>
        static float CowSpotShell(Vector3 p)
        {
            float s = CowSpots(p);
            return s > 0.06f ? s : Mathf.Max(CowSkin(p) - 0.005f, s);
        }

        static float Wobble(Vector3 p) =>
            Mathf.PerlinNoise(p.x * 3.1f + p.z * 1.7f + 11f, p.y * 3.3f + 5f) + Mathf.PerlinNoise(p.z * 2.9f + 3f, p.x * 2.3f + p.y * 1.9f) - 1f;

        static float CowUdder(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, -0.02f, 0f), new Vector3(0.3f, 0.2f, 0.26f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.1f, -0.07f, 0f), new Vector3(0.18f, 0.17f, 0.22f)), 0.08f);
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(-0.12f, -0.06f, 0f), new Vector3(0.18f, 0.17f, 0.22f)), 0.08f);
            foreach (var t in new[] { new Vector2(0.11f, 0.1f), new Vector2(0.11f, -0.1f), new Vector2(-0.11f, 0.1f), new Vector2(-0.11f, -0.1f) })
                d = S.SMin(d, S.Capsule(p, new Vector3(t.x, -0.15f, t.y), new Vector3(t.x * 1.08f, -0.34f, t.y * 1.08f), 0.045f, 0.032f), 0.05f);
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

            // Skelett (Weltlage = Modellraum, Wurzel im Ursprung)
            var spine = Bone(t, "Spine", CowSpine);
            var neck = Bone(spine, "Neck", CowNeckPos);
            var head = Bone(neck, "Head", CowHeadPos);
            var tail = new Transform[3];
            for (int i = 0; i < 3; i++) tail[i] = Bone(i == 0 ? spine : tail[i - 1], "Tail" + (i + 1), CowTailPos[i]);
            var legs = new Transform[4];
            var shins = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                legs[i] = Bone(t, "Leg" + i, CowHips[i]);
                shins[i] = Bone(legs[i], "Shin" + i, CowHips[i] + (i >= 2 ? KneeRear : KneeFront));
            }
            var bones = new List<Transform> { spine, neck, head };
            bones.AddRange(tail);
            bones.AddRange(legs);
            bones.AddRange(shins);
            var boneArr = bones.ToArray();

            // Körperteile für die Gewichte
            var parts = new List<SkinPart>
            {
                new SkinPart(CowTrunk, CSpine), new SkinPart(CowNeck, CNeck), new SkinPart(p => CowHead(p - CowHeadPos), CHead)
            };
            for (int i = 0; i < 3; i++)
            {
                int seg = i;
                parts.Add(new SkinPart(p => CowTailSeg(p, seg), CTail + seg));
            }
            for (int i = 0; i < 4; i++)
            {
                int leg = i;
                parts.Add(new SkinPart(p => CowLegUpper(p - CowHips[leg], leg >= 2), CLeg + leg));
                parts.Add(new SkinPart(p => CowLegLower(p - CowHips[leg], leg >= 2), CShin + leg));
            }
            // Aufblähen: Rumpf um die Wirbelsäule, Hals quer zu seiner Achse; Kopf, Schwanz und Euter wandern
            // über ihre Knochen mit (AnimalVisuals). Atmen: Rumpf entlang der Normalen.
            var shapes = new List<(string, SkinMesher.ShapeDelta)>
            {
                (AnimalVisuals.InflateShape, (p, n, w) => Vector3.Scale(p - CowSpine, CowInflAxis) * w[CSpine] + Across(p, CowNeckA, CowNeckB) * (AnimalVisuals.NeckSwell * w[CNeck])),
                (AnimalVisuals.BreatheShape, (p, n, w) => n * 0.035f * (w[CSpine] + 0.4f * w[CNeck]))
            };

            var bounds = new Bounds(new Vector3(0.35f, 0.95f, 0f), new Vector3(3.5f, 1.95f, 1.3f));
            var skinMesh = SkinMesh("Cow_Skin", CowSkin, bounds, 0.016f, parts, boneArr, shapes);
            // nur die sichtbare Kappe auf der Haut, nicht die Fleckkugeln im Inneren
            var spotMesh = SkinMesh("Cow_SkinSpots", CowSpotShell, bounds, 0.016f, parts, boneArr, shapes, c => CowSkin(c) > -0.012f);
            var skins = new List<SkinnedMeshRenderer>
            {
                SkinMesher.Renderer(t, "Skin", skinMesh, boneArr, spine, body, true),
                SkinMesher.Renderer(t, "Spots", spotMesh, boneArr, spine, spot, false)
            };
            var bodyRenderers = new List<Renderer> { skins[0] };
            var spots = new List<Renderer> { skins[1] };

            // Kopf: Maul, Nüstern, Augen, Hörner, Ohren mit Marke
            Part(head, "Muzzle", Vector3.zero, BuildMesh("Cow_Muzzle", p => S.Ellipsoid(p, new Vector3(0.6f, -0.2f, 0f), new Vector3(0.12f, 0.12f, 0.16f)),
                new Bounds(new Vector3(0.6f, -0.2f, 0f), new Vector3(0.32f, 0.32f, 0.4f)), 0.014f, Vector3.zero), pink, null);
            foreach (float z in new[] { 0.07f, -0.07f })
                Part(head, "Nostril", Vector3.zero, BuildMesh("Cow_Nostril" + (z > 0 ? "L" : "R"), p => S.Ellipsoid(p, new Vector3(0.71f, -0.17f, z), new Vector3(0.022f, 0.036f, 0.026f)),
                    new Bounds(new Vector3(0.71f, -0.17f, z), Vector3.one * 0.12f), 0.006f, Vector3.zero), nost, null);
            var ears = new List<Transform>();
            foreach (float zz in new[] { 1f, -1f })
            {
                LookDevKit.Obj(head, "Eye", SphereMesh(0.045f), new Vector3(0.15f, 0.07f, 0.19f * zz), eyeW, null, false);
                LookDevKit.Obj(head, "Pupil", SphereMesh(0.03f), new Vector3(0.18f, 0.075f, 0.215f * zz), eyeB, null, false);
                var earPos = new Vector3(-0.07f, 0.1f, 0.15f * zz);
                var ear = BlockoutKit.Group(head, zz > 0 ? "EarL" : "EarR", earPos);
                var earRot = Quaternion.Euler(-22f * zz, -12f * zz, 0f);
                Part(ear, "Ear", Vector3.zero, BuildMesh("Cow_Ear" + (zz > 0 ? "L" : "R"), p => S.Ellipsoid(p, new Vector3(-0.08f, 0.1f, 0.27f * zz), new Vector3(0.065f, 0.032f, 0.15f), earRot),
                    new Bounds(new Vector3(-0.08f, 0.1f, 0.27f * zz), Vector3.one * 0.42f), 0.008f, earPos), spot, spots);
                if (zz > 0) LookDevKit.Box(ear, "EarTag", new Vector3(-0.07f, 0.03f, 0.39f) - earPos, new Vector3(0.05f, 0.06f, 0.01f), tag, new Vector3(-22f, -12f, 0f), false);
                ears.Add(ear);
                LookDevKit.Obj(head, "Horn", LookDevKit.Lathe("CowHorn", new[] { new Vector2(0.045f, 0f), new Vector2(0.042f, 0.06f), new Vector2(0.032f, 0.13f), new Vector2(0.016f, 0.19f), new Vector2(0f, 0.22f) }, 16),
                    new Vector3(-0.06f, 0.2f, 0.15f * zz), horn, new Vector3(-38f * zz, 0f, -10f), false);
            }
            var collarGo = LookDevKit.Obj(neck, "Collar", LookDevKit.Ring(0.26f, 0.31f, 0.1f, 32), new Vector3(1.02f, 1.2f, 0f) - CowNeckPos, collar, new Vector3(0f, 0f, -76f), false);

            // Hufe am Unterschenkel, Quaste am Schwanzende
            for (int i = 0; i < 4; i++)
            {
                var floor = new Vector3(CowHips[i].x + (i >= 2 ? 0f : 0.01f), 0f, CowHips[i].z);
                LookDevKit.Obj(shins[i], "Hoof", LookDevKit.Lathe("CowHoof", new[] { new Vector2(0f, 0f), new Vector2(0.075f, 0f), new Vector2(0.074f, 0.03f), new Vector2(0.066f, 0.1f), new Vector2(0.058f, 0.12f), new Vector2(0f, 0.12f) }, 20),
                    floor - shins[i].position, hoof, null, false);
            }
            Part(tail[2], "TailTuft", Vector3.zero, BuildMesh("Cow_TailTuft", p => S.Ellipsoid(p, new Vector3(-1.21f, 0.56f, 0f), new Vector3(0.07f, 0.15f, 0.07f)) + Wobble(p * 6f) * 0.02f,
                new Bounds(new Vector3(-1.21f, 0.56f, 0f), new Vector3(0.3f, 0.45f, 0.3f)), 0.01f, CowTailPos[2]), spot, spots);

            // Euter (Knochen, wächst mit B) und Zusatz-Euter
            var udder = Bone(spine, "Udder", CowUdderPos);
            Part(udder, "Bag", Vector3.zero, BuildMesh("Cow_Udder", CowUdder, new Bounds(new Vector3(0f, -0.12f, 0f), new Vector3(0.75f, 0.6f, 0.65f)), 0.014f, Vector3.zero), udderM, null);
            var extra = new List<GameObject>();
            var small = BuildMesh("Cow_UdderSmall", SmallUdder, new Bounds(new Vector3(0f, -0.08f, 0f), new Vector3(0.5f, 0.5f, 0.45f)), 0.016f, Vector3.zero);
            foreach (var p in new[] { new Vector2(0.25f, 0f), new Vector2(0.55f, 0f), new Vector2(-0.65f, 0f), new Vector2(0.05f, 0.3f),
                         new Vector2(0.05f, -0.3f), new Vector2(-0.35f, 0.33f), new Vector2(-0.35f, -0.33f), new Vector2(0.4f, 0.3f) })
            {
                var eu = Bone(spine, "ExtraUdder", new Vector3(p.x, 0.62f, p.y));
                Part(eu, "Bag", Vector3.zero, small, udderM, null);
                eu.gameObject.SetActive(false);
                extra.Add(eu.gameObject);
            }

            SetupVisuals(root, AnimalKind.Cow, new Rig
            {
                spine = spine, neck = neck, head = head, ears = ears, tail = new List<Transform>(tail), legs = new List<Transform>(legs),
                shins = new List<Transform>(shins), udder = udder, extra = extra, wings = new List<Transform>(), skins = skins, inflAxis = CowInflAxis,
                neckWear = new List<Transform> { collarGo.transform },
                bodyRenderers = bodyRenderers, spots = spots
            });
            return root;
        }

        // ================= Huhn =================

        static readonly Vector3 HenBodyPos = new Vector3(0f, 0.95f, 0f);
        static readonly Vector3 HenNeckPos = new Vector3(0.36f, 1.22f, 0f);
        static readonly Vector3 HenHeadPos = new Vector3(0.55f, 1.55f, 0f);
        static readonly Vector3 HenTailPos = new Vector3(-0.42f, 1.2f, 0f);
        static readonly Vector3 HenNeckA = new Vector3(0.3f, 1.15f, 0f), HenNeckB = new Vector3(0.52f, 1.48f, 0f);
        static readonly Vector3 HenInflAxis = Vector3.one;
        const int HBody = 0, HNeck = 1, HHead = 2, HTail = 3;

        static float HenCore(Vector3 p)
        {
            float d = S.Ellipsoid(p, new Vector3(0f, 0.95f, 0f), new Vector3(0.58f, 0.48f, 0.44f));
            d = S.SMin(d, S.Ellipsoid(p, new Vector3(0.28f, 0.9f, 0f), new Vector3(0.38f, 0.42f, 0.38f)), 0.12f);
            return S.SMin(d, S.Ellipsoid(p, new Vector3(-0.25f, 1.1f, 0f), new Vector3(0.4f, 0.35f, 0.36f)), 0.12f);
        }

        static float HenNeck(Vector3 p) => S.Capsule(p, HenNeckA, HenNeckB, 0.24f, 0.15f);

        /// <summary>Schwanzfedern als Fächer.</summary>
        static float HenTail(Vector3 p)
        {
            float d = float.MaxValue;
            for (int i = 0; i < 5; i++)
            {
                float z = (i - 2) * 0.075f;
                var rot = Quaternion.Euler((i - 2) * 10f, 0f, 40f - Mathf.Abs(i - 2) * 6f);
                var e = S.Ellipsoid(p, new Vector3(-0.62f, 1.38f - Mathf.Abs(i - 2) * 0.04f, z), new Vector3(0.09f, 0.3f, 0.045f), rot);
                d = i == 0 ? e : S.SMin(d, e, 0.06f);
            }
            return d;
        }

        /// <summary>Kopf (Kopfraum).</summary>
        static float HenHead(Vector3 p)
        {
            float d = S.Sphere(p, Vector3.zero, 0.2f);
            return S.SMin(d, S.Ellipsoid(p, new Vector3(0.07f, -0.06f, 0f), new Vector3(0.17f, 0.14f, 0.15f)), 0.06f);
        }

        static float HenSkin(Vector3 p)
        {
            float d = S.SMin(S.SMin(HenCore(p), HenNeck(p), 0.12f), HenTail(p), 0.06f);
            d = S.SMin(d, HenHead(p - HenHeadPos), 0.06f);
            return d + Wobble(p * 5f) * 0.008f;
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

            var spine = Bone(t, "Body", HenBodyPos);
            var neck = Bone(spine, "Neck", HenNeckPos);
            var head = Bone(neck, "Head", HenHeadPos);
            var tail = Bone(spine, "Tail", HenTailPos);
            var boneArr = new[] { spine, neck, head, tail };
            var parts = new[]
            {
                new SkinPart(HenCore, HBody), new SkinPart(HenNeck, HNeck), new SkinPart(p => HenHead(p - HenHeadPos), HHead), new SkinPart(HenTail, HTail)
            };
            var shapes = new List<(string, SkinMesher.ShapeDelta)>
            {
                (AnimalVisuals.InflateShape, (p, n, w) => Vector3.Scale(p - HenBodyPos, HenInflAxis) * w[HBody] + Across(p, HenNeckA, HenNeckB) * (AnimalVisuals.NeckSwell * w[HNeck])),
                (AnimalVisuals.BreatheShape, (p, n, w) => n * 0.03f * w[HBody])
            };
            var skinMesh = SkinMesh("Hen_Skin", HenSkin, new Bounds(new Vector3(-0.05f, 1.15f, 0f), new Vector3(1.75f, 1.3f, 1.0f)), 0.014f, new List<SkinPart>(parts), boneArr, shapes);
            var skins = new List<SkinnedMeshRenderer> { SkinMesher.Renderer(t, "Skin", skinMesh, boneArr, spine, body, true) };
            var bodyRenderers = new List<Renderer> { skins[0] };

            Part(head, "Comb", Vector3.zero, BuildMesh("Hen_Comb", HenComb, new Bounds(new Vector3(0f, 0.05f, 0f), new Vector3(0.5f, 0.6f, 0.2f)), 0.008f, Vector3.zero), comb, null);
            LookDevKit.Obj(head, "Beak", LookDevKit.Lathe("HenBeak", new[] { new Vector2(0.065f, 0f), new Vector2(0.06f, 0.05f), new Vector2(0.035f, 0.12f), new Vector2(0f, 0.18f) }, 16),
                new Vector3(0.17f, -0.01f, 0f), beakM, new Vector3(0f, 0f, -98f), false);
            foreach (float zz in new[] { 1f, -1f })
            {
                LookDevKit.Obj(head, "Eye", SphereMesh(0.045f), new Vector3(0.13f, 0.05f, 0.135f * zz), eyeW, null, false);
                LookDevKit.Obj(head, "Pupil", SphereMesh(0.028f), new Vector3(0.155f, 0.055f, 0.16f * zz), eyeB, null, false);
            }

            // Flügel als Knochen am Körper
            var wings = new List<Transform>();
            var wingMesh = BuildMesh("Hen_Wing", HenWing, new Bounds(new Vector3(-0.25f, -0.06f, 0f), new Vector3(0.95f, 0.55f, 0.25f)), 0.014f, Vector3.zero);
            foreach (float zz in new[] { 0.44f, -0.44f })
            {
                var w = Part(spine, "Wing", new Vector3(0.14f, 1.05f, zz) - HenBodyPos, wingMesh, body, bodyRenderers);
                wings.Add(w.transform);
            }

            var legs = new List<Transform>();
            var legMesh = BuildMesh("Hen_Leg", HenLeg, new Bounds(new Vector3(0.04f, -0.12f, 0f), new Vector3(0.4f, 0.5f, 0.3f)), 0.008f, Vector3.zero);
            foreach (float zz in new[] { 0.15f, -0.15f })
            {
                var pv = Bone(t, "Leg", new Vector3(0.05f, 0.5f, zz));
                Part(pv, "LegMesh", Vector3.zero, legMesh, beakM, null);
                legs.Add(pv);
            }

            var extra = new List<GameObject>();
            for (int i = 0; i < 6; i++)
            {
                // Wucherung auf die Haut setzen: vom Körpermittelpunkt in Richtung der Prototyp-Lage bis zur Oberfläche
                var dir = (new Vector3(-0.5f + (i % 3) * 0.14f, 0.6f + (i / 3) * 0.1f, i % 2 == 1 ? 0.2f : -0.2f) - HenBodyPos).normalized;
                float r = 0f;
                while (r < 1f && HenSkin(HenBodyPos + dir * r) < 0f) r += 0.005f;
                var eu = LookDevKit.Obj(spine, "Growth", SphereMesh(0.075f), dir * (r - 0.015f), bump, null, false);
                eu.SetActive(false);
                extra.Add(eu);
            }
            SetupVisuals(root, AnimalKind.Hen, new Rig
            {
                spine = spine, neck = neck, head = head, ears = new List<Transform>(), tail = new List<Transform> { tail }, legs = legs,
                shins = new List<Transform>(), udder = null, extra = extra, wings = wings, skins = skins, bodyRenderers = bodyRenderers, inflAxis = HenInflAxis,
                neckWear = new List<Transform>(),
                spots = new List<Renderer>()
            });
            return root;
        }

        // ================= Hilfen =================

        /// <summary>Knochen an einer Modellraum-Position (der Eltern-Knochen ist ungedreht, also nur Versatz).</summary>
        static Transform Bone(Transform parent, string name, Vector3 modelPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = modelPos;
            return go.transform;
        }

        /// <summary>Abstand von der Achse a–b (senkrecht dazu), für das Aufblähen eines Halses.</summary>
        static Vector3 Across(Vector3 p, Vector3 a, Vector3 b)
        {
            var ab = b - a;
            float h = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
            return p - (a + ab * h);
        }

        static Mesh SkinMesh(string name, SdfMesher.Field f, Bounds b, float cell, List<SkinPart> parts, Transform[] bones,
            List<(string, SkinMesher.ShapeDelta)> shapes, System.Func<Vector3, bool> keep = null)
        {
            var mesh = SdfMesher.Build(name, f, b, cell, Vector3.zero, keep);
            SkinMesher.Skin(mesh, parts.ToArray(), bones, SkinSigma, shapes);
            return SkinMesher.Save(mesh, $"{MeshDir}/{name}.asset");
        }

        static Mesh BuildMesh(string name, SdfMesher.Field f, Bounds b, float cell, Vector3 origin) =>
            SdfMesher.Save(SdfMesher.Build(name, f, b, cell, origin), $"{MeshDir}/{name}.asset");

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

        static GameObject Part(Transform parent, string name, Vector3 pos, Mesh mesh, Material mat, List<Renderer> list)
        {
            var go = LookDevKit.Obj(parent, name, mesh, pos, mat, null, false, true);
            list?.Add(go.GetComponent<Renderer>());
            return go;
        }

        /// <summary>Verweise des Skeletts für AnimalVisuals.</summary>
        class Rig
        {
            public Transform spine, neck, head, udder;
            public Vector3 inflAxis;
            public List<Transform> ears, tail, legs, shins, wings, neckWear;
            public List<GameObject> extra;
            public List<SkinnedMeshRenderer> skins;
            public List<Renderer> bodyRenderers, spots;
        }

        static void SetupVisuals(GameObject root, AnimalKind kind, Rig r)
        {
            var v = root.AddComponent<AnimalVisuals>();
            var so = new SerializedObject(v);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("spine").objectReferenceValue = r.spine;
            so.FindProperty("neck").objectReferenceValue = r.neck;
            so.FindProperty("head").objectReferenceValue = r.head;
            so.FindProperty("udder").objectReferenceValue = r.udder;
            so.FindProperty("inflateAxis").vector3Value = r.inflAxis;
            Fill(so.FindProperty("ears"), r.ears);
            Fill(so.FindProperty("tail"), r.tail);
            Fill(so.FindProperty("legs"), r.legs);
            Fill(so.FindProperty("shins"), r.shins);
            Fill(so.FindProperty("extraUdders"), r.extra);
            Fill(so.FindProperty("wings"), r.wings);
            Fill(so.FindProperty("neckWear"), r.neckWear);
            Fill(so.FindProperty("skins"), r.skins);
            Fill(so.FindProperty("bodyRenderers"), r.bodyRenderers);
            Fill(so.FindProperty("spotRenderers"), r.spots);
            so.ApplyModifiedPropertiesWithoutUndo();
            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true)) mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            foreach (var tr in root.GetComponentsInChildren<Transform>(true)) BlockoutKit.SetStatic(tr.gameObject, false);
        }

        static void Fill<T>(SerializedProperty p, List<T> items) where T : Object
        {
            p.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
