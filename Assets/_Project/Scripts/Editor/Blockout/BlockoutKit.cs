using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Bausteine für den Labor-Blockout: Materialien (URP Lit), Grundkörper ohne Kollider, 3D-Texte.
    /// Alle Maße hier sind Modellierdaten des Platzhalters, keine Spielwerte.
    /// </summary>
    public static class BlockoutKit
    {
        public const string MaterialDir = "Assets/_Project/Art/Materials/Blockout";
        public const string FontDir = "Assets/_Project/Art/UI/Fonts";

        static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();
        static TMP_FontAsset signFont;
        static TextDatabase texts;
        /// <summary>Texte des laufenden Baus (für Bauteile, die Text selbst aufbereiten).</summary>
        public static TextDatabase Texts => texts;
        /// <summary>Schrift der 3D-Schilder (Oswald Bold).</summary>
        public static TMP_FontAsset SignFont => signFont;

        /// <summary>Vor jedem Bau aufrufen (leert Zwischenspeicher, lädt Texte).</summary>
        public static void Begin(TextDatabase textDb)
        {
            Cache.Clear();
            texts = textDb;
            signFont = FontAssetCreator.Ensure($"{FontDir}/Oswald/Oswald-Bold.ttf", $"{FontDir}/Oswald/Oswald-Bold SDF.asset");
        }

        // ---------- Materialien ----------

        /// <summary>Undurchsichtiges URP-Lit-Material, wird als Asset gespeichert und wiederverwendet.</summary>
        public static Material Mat(string name, Color color, float metallic = 0f, float smoothness = 0.3f)
        {
            var m = LoadOrCreateMat(name);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Metallic", metallic);
            m.SetFloat("_Smoothness", smoothness);
            m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Leuchtendes Material. bakedEmission = trägt zum gebackenen Licht bei.</summary>
        public static Material Emissive(string name, Color color, float intensity, bool bakedEmission)
        {
            var m = LoadOrCreateMat(name);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_Smoothness", 0.5f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", color * intensity);
            m.globalIlluminationFlags = bakedEmission ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Transparentes Material (Glas, Gas). Optional leuchtend.</summary>
        public static Material Transparent(string name, Color color, float smoothness, Color? emission = null)
        {
            var m = LoadOrCreateMat(name);
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Metallic", 0f);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            m.SetFloat("_ZWrite", 0f);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
            if (emission.HasValue)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", emission.Value);
            }
            else m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            EditorUtility.SetDirty(m);
            return m;
        }

        static Material LoadOrCreateMat(string name)
        {
            if (Cache.TryGetValue(name, out var cached)) return cached;
            EnsureFolder(MaterialDir);
            string path = $"{MaterialDir}/M_{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            Cache[name] = m;
            return m;
        }

        // ---------- Grundkörper ----------

        /// <summary>Quader (Mittelpunkt, Größe), ohne Kollider.</summary>
        public static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat, bool isStatic = true, Vector3? euler = null)
        {
            var go = Prim(PrimitiveType.Cube, parent, name, center, size, mat, isStatic);
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            return go;
        }

        /// <summary>Zylinder (Mittelpunkt, Radius, Höhe), ohne Kollider.</summary>
        public static GameObject Cyl(Transform parent, string name, Vector3 center, float radius, float height, Material mat, bool isStatic = true, Vector3? euler = null)
        {
            var go = Prim(PrimitiveType.Cylinder, parent, name, center, new Vector3(radius * 2f, height * 0.5f, radius * 2f), mat, isStatic);
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            return go;
        }

        /// <summary>Kugel (Mittelpunkt, Radius), ohne Kollider.</summary>
        public static GameObject Sphere(Transform parent, string name, Vector3 center, float radius, Material mat, bool isStatic = true)
        {
            return Prim(PrimitiveType.Sphere, parent, name, center, Vector3.one * radius * 2f, mat, isStatic);
        }

        /// <summary>Flaches Rechteck, sichtbar von -Z (Vorderseite zum Spieler).</summary>
        public static GameObject Quad(Transform parent, string name, Vector3 center, Vector2 size, Material mat, bool isStatic = true, Vector3? euler = null)
        {
            var go = Prim(PrimitiveType.Quad, parent, name, center, new Vector3(size.x, size.y, 1f), mat, isStatic);
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            return go;
        }

        /// <summary>Objekt mit eigenem Mesh (aus MeshFactory).</summary>
        public static GameObject MeshObj(Transform parent, string name, Mesh mesh, Vector3 pos, Material mat, bool isStatic = true, Vector3? euler = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            if (euler.HasValue) go.transform.localEulerAngles = euler.Value;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
            SetStatic(go, isStatic);
            return go;
        }

        /// <summary>Leeres Kind-Objekt.</summary>
        public static Transform Group(Transform parent, string name, Vector3 localPos, float yaw = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = new Vector3(0f, yaw, 0f);
            return go.transform;
        }

        static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 center, Vector3 scale, Material mat, bool isStatic)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            SetStatic(go, isStatic);
            return go;
        }

        /// <summary>Statische Flags für gebackenes Licht und Batching setzen oder entfernen.</summary>
        public static void SetStatic(GameObject go, bool isStatic)
        {
            var flags = isStatic
                ? StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic | StaticEditorFlags.OccludeeStatic
                : 0;
            GameObjectUtility.SetStaticEditorFlags(go, flags);
        }

        /// <summary>Schatten werfen ein/aus (z. B. Decke aus, damit das Hauptlicht hereinfällt).</summary>
        public static void CastShadows(GameObject go, bool on)
        {
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                r.shadowCastingMode = on ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        // ---------- Texte ----------

        /// <summary>
        /// 3D-Text aus der Text-JSON. Mit Hintergrundplatte; mit billboard dreht sich das Schild zur Kamera.
        /// </summary>
        public static GameObject Sign(Transform parent, string name, string textKey, Vector3 localPos, float fontSize, Color textColor,
            Material plateMat, bool billboard, float yaw = 0f)
        {
            var root = Group(parent, name, localPos, yaw);
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(root, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.font = signFont;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.text = texts != null ? texts.Get(textKey) : textKey;
            // Schilder sind einzeilig: kein Umbruch (sonst bricht z. B. „WUT“ um, wenn die Breite zu knapp gemessen wird)
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.rectTransform.sizeDelta = new Vector2(20f, 5f);
            tmp.ForceMeshUpdate();
            var pref = tmp.GetPreferredValues(tmp.text);
            tmp.rectTransform.sizeDelta = new Vector2(pref.x + 0.05f, pref.y + 0.05f);

            if (plateMat != null)
                Quad(root, "Plate", Vector3.zero, new Vector2(pref.x + fontSize * 0.08f, pref.y + fontSize * 0.04f), plateMat, !billboard);

            var sign = root.gameObject.AddComponent<StationSign>();
            sign.Setup(textKey, tmp, billboard);
            return root.gameObject;
        }

        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
