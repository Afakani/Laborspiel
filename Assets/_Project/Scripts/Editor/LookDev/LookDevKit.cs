using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Bausteine für den Look-Pass (Bild 07/08): Meshes mit Welt-UVs (1 UV-Einheit = 1 m, damit Texturen auf allen
    /// Größen gleich dicht sitzen), Röhren entlang von Kurven, Ringe, PBR-Materialien aus den Poly-Haven-Texturen.
    /// </summary>
    public static class LookDevKit
    {
        const string MeshDir = "Assets/_Project/Art/Models/LookDev";
        const string MatDir = "Assets/_Project/Art/Materials/LookDev";
        public const string PolyDir = "Assets/_Project/Art/Textures/PolyHaven";
        public const string OwnTexDir = "Assets/_Project/Art/Textures/LookDev";

        static readonly Dictionary<string, Mesh> MeshCache = new Dictionary<string, Mesh>();
        static readonly Dictionary<string, Material> MatCache = new Dictionary<string, Material>();

        public static void Begin()
        {
            MeshCache.Clear();
            MatCache.Clear();
            PrepareTextures();
        }

        // ---------- Texturen ----------

        /// <summary>Importeinstellungen: Normal-Maps als Normal Map, Masken linear, eigene Texturen mit Alpha.</summary>
        static void PrepareTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PolyDir, OwnTexDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = (TextureImporter)AssetImporter.GetAtPath(path);
                bool changed = false;
                if (path.EndsWith("_Normal.png") && imp.textureType != TextureImporterType.NormalMap)
                {
                    imp.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }
                if (path.EndsWith("_Mask.png") && imp.sRGBTexture)
                {
                    imp.sRGBTexture = false;
                    changed = true;
                }
                if (path.StartsWith(OwnTexDir) && !imp.alphaIsTransparency)
                {
                    imp.alphaIsTransparency = true;
                    imp.wrapMode = path.Contains("Hazard") || path.Contains("Paper") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                    changed = true;
                }
                if (changed) imp.SaveAndReimport();
            }
        }

        public static Texture2D Tex(string path) => AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        // ---------- Materialien ----------

        /// <summary>
        /// PBR-Material aus einer Poly-Haven-Textur. tint färbt die Grundfarbe, metersPerTile = Kachelgröße in Metern,
        /// smooth skaliert die Glätte aus der Maske.
        /// </summary>
        public static Material Pbr(string name, string polyId, Color tint, float metersPerTile, float smooth = 1f, float normal = 1f)
        {
            var m = LoadOrCreate(name, "Universal Render Pipeline/Lit");
            string b = $"{PolyDir}/{polyId}/{polyId}";
            m.SetTexture("_BaseMap", Tex(b + "_Albedo.jpg"));
            m.SetColor("_BaseColor", tint);
            m.SetTexture("_BumpMap", Tex(b + "_Normal.png"));
            m.SetFloat("_BumpScale", normal);
            m.EnableKeyword("_NORMALMAP");
            var mask = Tex(b + "_Mask.png");
            m.SetTexture("_MetallicGlossMap", mask);
            m.EnableKeyword("_METALLICSPECGLOSSMAP");
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_SmoothnessTextureChannel", 0f);
            m.SetTexture("_OcclusionMap", mask);
            m.SetFloat("_OcclusionStrength", 1f);
            m.EnableKeyword("_OCCLUSIONMAP");
            float k = 1f / Mathf.Max(0.01f, metersPerTile);
            m.SetTextureScale("_BaseMap", new Vector2(k, k));
            m.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Material mit eigener Textur (Warnstreifen, Papier, Direktor, Plantafel …), optional leuchtend.</summary>
        public static Material Textured(string name, string texFile, Color tint, float metersPerTile, float smooth, float emission = 0f, bool cutout = false)
        {
            var m = LoadOrCreate(name, "Universal Render Pipeline/Lit");
            var tex = Tex($"{OwnTexDir}/{texFile}");
            m.SetTexture("_BaseMap", tex);
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", smooth);
            m.SetFloat("_Metallic", 0f);
            float k = metersPerTile > 0f ? 1f / metersPerTile : 1f;
            m.SetTextureScale("_BaseMap", new Vector2(k, k));
            if (emission > 0f)
            {
                m.EnableKeyword("_EMISSION");
                m.SetTexture("_EmissionMap", tex);
                m.SetColor("_EmissionColor", Color.white * emission);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else m.DisableKeyword("_EMISSION");
            if (cutout)
            {
                m.SetFloat("_AlphaClip", 1f);
                m.SetFloat("_Cutoff", 0.5f);
                m.EnableKeyword("_ALPHATEST_ON");
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Additives, beidseitiges Material für Lichtkegel und Nebelpartikel (URP Particles/Unlit).</summary>
        public static Material Additive(string name, string texFile, Color color, bool additive)
        {
            var m = LoadOrCreate(name, "Universal Render Pipeline/Particles/Unlit");
            m.SetTexture("_BaseMap", Tex($"{OwnTexDir}/{texFile}"));
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f);
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", additive ? (float)BlendMode.One : (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.SetFloat("_Cull", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent + 10;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Weicher Lichtkegel mit eigenem Shader (Fake-Volumetrik, siehe LightShaft.shader).</summary>
        public static Material Shaft(string name, Color color, float intensity, float falloff)
        {
            var m = LoadOrCreate(name, "PlanErfuellt/LightShaft");
            m.SetColor("_Color", color);
            m.SetFloat("_Intensity", intensity);
            m.SetFloat("_Falloff", falloff);
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Klares Glas mit starker Spiegelung (Reflexionssonde).</summary>
        public static Material Glass(string name, Color tint, float smoothness)
        {
            var m = BlockoutKit.Transparent(name, tint, smoothness);
            m.SetFloat("_Metallic", 0.1f);
            m.SetFloat("_EnvironmentReflections", 1f);
            m.SetFloat("_SpecularHighlights", 1f);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material LoadOrCreate(string name, string shader)
        {
            if (MatCache.TryGetValue(name, out var cached)) return cached;
            BlockoutKit.EnsureFolder(MatDir);
            string path = $"{MatDir}/M_{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null || m.shader.name != shader)
            {
                if (m != null) AssetDatabase.DeleteAsset(path);
                m = new Material(Shader.Find(shader));
                AssetDatabase.CreateAsset(m, path);
            }
            MatCache[name] = m;
            return m;
        }

        // ---------- Meshes mit Welt-UVs ----------

        /// <summary>Quader mit Welt-UVs je Fläche.</summary>
        public static Mesh Box(Vector3 size)
        {
            string key = "Box_" + K(size.x) + "_" + K(size.y) + "_" + K(size.z);
            if (TryGet(key, out var mesh)) return mesh;
            var b = new MeshBuilder();
            Vector3 h = size * 0.5f;
            // +X, -X, +Y, -Y, +Z, -Z
            b.Quad(new Vector3(h.x, -h.y, -h.z), new Vector3(0, 0, size.z), new Vector3(0, size.y, 0), Vector3.right);
            b.Quad(new Vector3(-h.x, -h.y, h.z), new Vector3(0, 0, -size.z), new Vector3(0, size.y, 0), Vector3.left);
            b.Quad(new Vector3(-h.x, h.y, -h.z), new Vector3(size.x, 0, 0), new Vector3(0, 0, size.z), Vector3.up);
            b.Quad(new Vector3(-h.x, -h.y, h.z), new Vector3(size.x, 0, 0), new Vector3(0, 0, -size.z), Vector3.down);
            b.Quad(new Vector3(h.x, -h.y, h.z), new Vector3(-size.x, 0, 0), new Vector3(0, size.y, 0), Vector3.forward);
            b.Quad(new Vector3(-h.x, -h.y, -h.z), new Vector3(size.x, 0, 0), new Vector3(0, size.y, 0), Vector3.back);
            return Save(key, b);
        }

        /// <summary>Zylinder um Y (Mitte im Ursprung), Mantel-UV = Bogenlänge × Höhe, mit Deckeln.</summary>
        public static Mesh Cylinder(float radius, float height, int segments = 24, bool caps = true)
        {
            string key = $"Cyl_{K(radius)}_{K(height)}_{segments}_{(caps ? 1 : 0)}";
            if (TryGet(key, out var mesh)) return mesh;
            var b = new MeshBuilder();
            b.Wall(radius, radius, -height / 2f, height / 2f, segments, true);
            if (caps)
            {
                b.Disc(radius, 0f, height / 2f, segments, true);
                b.Disc(radius, 0f, -height / 2f, segments, false);
            }
            return Save(key, b);
        }

        /// <summary>Dicker Ring (Rohrstück) um Y: Außen- und Innenwand, oben und unten Kreisringe.</summary>
        public static Mesh Ring(float innerRadius, float outerRadius, float height, int segments = 48)
        {
            string key = $"Ring_{K(innerRadius)}_{K(outerRadius)}_{K(height)}_{segments}";
            if (TryGet(key, out var mesh)) return mesh;
            var b = new MeshBuilder();
            b.Wall(outerRadius, outerRadius, -height / 2f, height / 2f, segments, true);
            b.Wall(innerRadius, innerRadius, -height / 2f, height / 2f, segments, false);
            b.Disc(outerRadius, innerRadius, height / 2f, segments, true);
            b.Disc(outerRadius, innerRadius, -height / 2f, segments, false);
            return Save(key, b);
        }

        /// <summary>
        /// Drehkörper um Y aus einem Profil (x = Radius, y = Höhe), weich schattiert. doubleSided für offene Schalen (Horn, Trichter).
        /// </summary>
        public static Mesh Lathe(string name, Vector2[] profile, int segments = 32, bool doubleSided = false)
        {
            string key = "Lathe_" + name;
            var b = new MeshBuilder();
            b.Lathe(profile, segments, false);
            if (doubleSided) b.Lathe(profile, segments, true);
            return Save(key, b, true);
        }

        /// <summary>Ringbogen von startDeg bis endDeg (gegen den Uhrzeigersinn von +X zu +Z) mit Stirnflächen.</summary>
        public static Mesh RingArc(float innerRadius, float outerRadius, float height, float startDeg, float endDeg, int segments = 32)
        {
            string key = $"RingArc_{K(innerRadius)}_{K(outerRadius)}_{K(height)}_{K(startDeg)}_{K(endDeg)}_{segments}";
            if (TryGet(key, out var mesh)) return mesh;
            float a0 = startDeg * Mathf.Deg2Rad, a1 = endDeg * Mathf.Deg2Rad, h = height / 2f;
            var b = new MeshBuilder();
            b.Wall(outerRadius, outerRadius, -h, h, segments, true, a0, a1);
            b.Wall(innerRadius, innerRadius, -h, h, segments, false, a0, a1);
            b.Disc(outerRadius, innerRadius, h, segments, true, a0, a1);
            b.Disc(outerRadius, innerRadius, -h, segments, false, a0, a1);
            foreach (var (a, sign) in new[] { (a0, -1f), (a1, 1f) })
            {
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var tangent = new Vector3(-dir.z, 0f, dir.x) * sign;
                b.Quad(dir * innerRadius + Vector3.down * h, dir * (outerRadius - innerRadius), Vector3.up * height, tangent);
            }
            return Save(key, b);
        }

        /// <summary>Röhre (Schlauch, Rohr) entlang einer Bézier-Kurve durch 4 Kontrollpunkte (lokal).</summary>
        public static Mesh Tube(string name, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float radius, int steps = 24, int sides = 10)
        {
            string key = "Tube_" + name;
            var b = new MeshBuilder();
            var pts = new List<Vector3>();
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps, u = 1f - t;
                pts.Add(u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3);
            }
            b.Sweep(pts, radius, sides);
            return Save(key, b, true);
        }

        static bool TryGet(string key, out Mesh mesh)
        {
            if (MeshCache.TryGetValue(key, out mesh)) return true;
            mesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{MeshDir}/{key}.asset");
            if (mesh != null) MeshCache[key] = mesh;
            return mesh != null;
        }

        static Mesh Save(string key, MeshBuilder b, bool overwrite = false)
        {
            BlockoutKit.EnsureFolder(MeshDir);
            string path = $"{MeshDir}/{key}.asset";
            var mesh = b.Build(key);
            Unwrapping.GenerateSecondaryUVSet(mesh);
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null && overwrite)
            {
                SdfMesher.CopyInto(mesh, existing);
                Object.DestroyImmediate(mesh);
                mesh = existing;
            }
            else if (existing == null) AssetDatabase.CreateAsset(mesh, path);
            else mesh = existing;
            MeshCache[key] = mesh;
            return mesh;
        }

        static string K(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);

        // ---------- Platzieren ----------

        public static GameObject Obj(Transform parent, string name, Mesh mesh, Vector3 pos, Material mat, Vector3? euler = null, bool isStatic = true, bool shadows = true)
        {
            var go = BlockoutKit.MeshObj(parent, name, mesh, pos, mat, isStatic, euler);
            go.GetComponent<MeshRenderer>().shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            return go;
        }

        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, Vector3? euler = null, bool isStatic = true) =>
            Obj(parent, name, Box(size), center, mat, euler, isStatic);

        public static GameObject Cyl(Transform parent, string name, Vector3 center, float r, float h, Material mat, Vector3? euler = null, bool isStatic = true, int seg = 24) =>
            Obj(parent, name, Cylinder(r, h, seg), center, mat, euler, isStatic);

        /// <summary>Schraubenköpfe entlang einer Strecke (kleine Zylinder, Achse = normal).</summary>
        public static void Bolts(Transform parent, Vector3 from, Vector3 to, int count, Vector3 normal, Material mat, float radius = 0.014f)
        {
            var mesh = Cylinder(radius, 0.012f, 8);
            var rot = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                Obj(parent, "Bolt", mesh, Vector3.Lerp(from, to, t) + normal * 0.006f, mat, rot, true, false);
            }
        }

        /// <summary>Schraubenkranz auf einem Kreis (Ringe, Deckel).</summary>
        public static void BoltCircle(Transform parent, float radius, float y, int count, Material mat, bool onTop, float boltRadius = 0.025f)
        {
            var mesh = Cylinder(boltRadius, 0.03f, 8);
            for (int i = 0; i < count; i++)
            {
                float a = i / (float)count * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(a) * radius, y, Mathf.Sin(a) * radius);
                if (onTop) Obj(parent, "Bolt", mesh, p, mat, null, true, false);
                else
                {
                    var n = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    Obj(parent, "Bolt", mesh, p + n * 0.012f, mat, Quaternion.FromToRotation(Vector3.up, n).eulerAngles, true, false);
                }
            }
        }

        /// <summary>Flache Fläche mit Textur (Schild, Bild), sichtbar von -Z.</summary>
        public static GameObject Plane(Transform parent, string name, Vector3 pos, Vector2 size, Material mat, Vector3? euler = null, bool isStatic = true)
        {
            var go = BlockoutKit.Quad(parent, name, pos, size, mat, isStatic, euler);
            go.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            return go;
        }

        /// <summary>Fester 3D-Text aus der Text-JSON (kein Drehen zur Kamera).</summary>
        public static GameObject Label(Transform parent, string name, string key, Vector3 pos, float size, Color color, Vector3? euler = null, TMPro.TextAlignmentOptions align = TMPro.TextAlignmentOptions.Center)
        {
            var go = BlockoutKit.Sign(parent, name, key, pos, size, color, null, false);
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            var tmp = go.GetComponentInChildren<TMPro.TMP_Text>();
            tmp.alignment = align;
            return go;
        }

        // ---------- Mesh-Bau ----------

        class MeshBuilder
        {
            readonly List<Vector3> v = new List<Vector3>();
            readonly List<Vector3> n = new List<Vector3>();
            readonly List<Vector2> uv = new List<Vector2>();
            readonly List<int> t = new List<int>();

            /// <summary>Rechteck ab origin mit Kanten a (U) und b (V), Normale normal; UV in Metern.</summary>
            public void Quad(Vector3 origin, Vector3 a, Vector3 b, Vector3 normal)
            {
                int i = v.Count;
                v.Add(origin); v.Add(origin + a); v.Add(origin + a + b); v.Add(origin + b);
                for (int k = 0; k < 4; k++) n.Add(normal);
                float la = a.magnitude, lb = b.magnitude;
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(la, 0)); uv.Add(new Vector2(la, lb)); uv.Add(new Vector2(0, lb));
                // Unity: Vorderseite, wenn Cross(b - a, c - a) in Richtung der Normalen zeigt
                if (Vector3.Dot(Vector3.Cross(a, b), normal) > 0f) t.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
                else t.AddRange(new[] { i, i + 2, i + 1, i, i + 3, i + 2 });
            }

            /// <summary>Zylindermantel; outward = Normale nach außen.</summary>
            public void Wall(float r0, float r1, float y0, float y1, int seg, bool outward, float from = 0f, float to = Mathf.PI * 2f)
            {
                int start = v.Count;
                for (int s = 0; s <= seg; s++)
                {
                    float a = Mathf.Lerp(from, to, s / (float)seg);
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    var nn = outward ? dir : -dir;
                    v.Add(dir * r0 + Vector3.up * y0); n.Add(nn); uv.Add(new Vector2(a * r0, 0f));
                    v.Add(dir * r1 + Vector3.up * y1); n.Add(nn); uv.Add(new Vector2(a * r1, y1 - y0));
                }
                for (int s = 0; s < seg; s++)
                {
                    int i = start + s * 2;
                    if (outward) t.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                    else t.AddRange(new[] { i, i + 3, i + 1, i, i + 2, i + 3 });
                }
            }

            /// <summary>Kreisscheibe bzw. Kreisring auf Höhe y, Normale nach oben (up) oder unten.</summary>
            public void Disc(float rOut, float rIn, float y, int seg, bool up, float from = 0f, float to = Mathf.PI * 2f)
            {
                int start = v.Count;
                var nn = up ? Vector3.up : Vector3.down;
                for (int s = 0; s <= seg; s++)
                {
                    float a = Mathf.Lerp(from, to, s / (float)seg);
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    var po = dir * rOut + Vector3.up * y;
                    var pi = dir * rIn + Vector3.up * y;
                    v.Add(po); n.Add(nn); uv.Add(new Vector2(po.x, po.z));
                    v.Add(pi); n.Add(nn); uv.Add(new Vector2(pi.x, pi.z));
                }
                for (int s = 0; s < seg; s++)
                {
                    int i = start + s * 2;
                    if (up) t.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                    else t.AddRange(new[] { i, i + 3, i + 1, i, i + 2, i + 3 });
                }
            }

            /// <summary>Drehkörper; inside = Innenseite (Normalen umgedreht).</summary>
            public void Lathe(Vector2[] prof, int seg, bool inside)
            {
                int start = v.Count, rows = prof.Length;
                float len = 0f;
                for (int i = 0; i < rows; i++)
                {
                    if (i > 0) len += Vector2.Distance(prof[i], prof[i - 1]);
                    var tan = (prof[Mathf.Min(i + 1, rows - 1)] - prof[Mathf.Max(i - 1, 0)]).normalized;
                    var n2 = new Vector2(tan.y, -tan.x);
                    for (int s = 0; s <= seg; s++)
                    {
                        float a = s / (float)seg * Mathf.PI * 2f;
                        var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                        v.Add(dir * prof[i].x + Vector3.up * prof[i].y);
                        var nn = (dir * n2.x + Vector3.up * n2.y).normalized;
                        n.Add(inside ? -nn : nn);
                        uv.Add(new Vector2(a * Mathf.Max(0.05f, prof[i].x), len));
                    }
                }
                int row = seg + 1;
                for (int i = 0; i < rows - 1; i++)
                for (int s = 0; s < seg; s++)
                {
                    int a0 = start + i * row + s, b0 = a0 + row;
                    if (inside) t.AddRange(new[] { a0, a0 + 1, b0, a0 + 1, b0 + 1, b0 });
                    else t.AddRange(new[] { a0, b0, a0 + 1, a0 + 1, b0, b0 + 1 });
                }
            }

            /// <summary>Röhre entlang einer Punktliste.</summary>
            public void Sweep(List<Vector3> pts, float radius, int sides)
            {
                int start = v.Count;
                float len = 0f;
                Vector3 prevNormal = Vector3.zero;
                for (int i = 0; i < pts.Count; i++)
                {
                    var fwd = (i < pts.Count - 1 ? pts[i + 1] - pts[i] : pts[i] - pts[i - 1]).normalized;
                    if (i > 0) len += Vector3.Distance(pts[i], pts[i - 1]);
                    var side = prevNormal == Vector3.zero ? Vector3.Cross(fwd, Mathf.Abs(fwd.y) < 0.9f ? Vector3.up : Vector3.right).normalized
                        : Vector3.Cross(fwd, Vector3.Cross(prevNormal, fwd)).normalized;
                    var up = Vector3.Cross(side, fwd).normalized;
                    prevNormal = side;
                    for (int s = 0; s <= sides; s++)
                    {
                        float a = s / (float)sides * Mathf.PI * 2f;
                        var dir = side * Mathf.Cos(a) + up * Mathf.Sin(a);
                        v.Add(pts[i] + dir * radius);
                        n.Add(dir);
                        uv.Add(new Vector2(a * radius, len));
                    }
                }
                int row = sides + 1;
                for (int i = 0; i < pts.Count - 1; i++)
                for (int s = 0; s < sides; s++)
                {
                    int a0 = start + i * row + s, b0 = a0 + row;
                    t.AddRange(new[] { a0, b0, a0 + 1, a0 + 1, b0, b0 + 1 });
                }
            }

            public Mesh Build(string name)
            {
                var m = new Mesh { name = name };
                if (v.Count > 65000) m.indexFormat = IndexFormat.UInt32;
                m.SetVertices(v);
                m.SetNormals(n);
                m.SetUVs(0, uv);
                m.SetTriangles(t, 0);
                m.RecalculateBounds();
                m.RecalculateTangents();
                return m;
            }
        }
    }
}
