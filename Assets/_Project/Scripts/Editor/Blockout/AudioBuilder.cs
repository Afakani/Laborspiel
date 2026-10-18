using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Phase 12: Ton und Einstellungen in die Szene bringen. Importiert die selbst erzeugten Klänge
    /// (Tools/gen_audio.py), legt den Knoten „Audio“ mit AudioService und GraphicsQuality an und erzeugt
    /// die URP-Einstellung „Niedrig“ als Kopie der Projekt-Einstellung mit den Werten aus settings.
    /// </summary>
    public static class AudioBuilder
    {
        public const string ClipDir = "Assets/_Project/Audio/Generated";
        const string HighAsset = "Assets/Settings/PC_RPAsset.asset";
        const string LowAsset = "Assets/Settings/PC_RPAsset_Low.asset";

        public static void Build(Transform systems, GameConfig cfg)
        {
            var old = systems.Find("Audio");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject("Audio", typeof(AudioService), typeof(GraphicsQuality));
            go.transform.SetParent(systems, false);
            go.GetComponent<AudioService>().Setup(LoadClips());
            go.GetComponent<GraphicsQuality>().Setup(AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(HighAsset), BuildLowAsset(cfg.settings));
        }

        /// <summary>Alle Klänge laden; Schleifen und kurze Klänge werden ungestreamt im Speicher gehalten.</summary>
        static AudioClip[] LoadClips()
        {
            var list = new List<AudioClip>();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { ClipDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var imp = (AudioImporter)AssetImporter.GetAtPath(path);
                var s = imp.defaultSampleSettings;
                if (s.loadType != AudioClipLoadType.DecompressOnLoad || !imp.forceToMono)
                {
                    s.loadType = AudioClipLoadType.DecompressOnLoad;
                    s.compressionFormat = AudioCompressionFormat.Vorbis;
                    s.quality = 0.7f;
                    imp.defaultSampleSettings = s;
                    imp.forceToMono = true;
                    imp.SaveAndReimport();
                }
                list.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(path));
            }
            return list.ToArray();
        }

        /// <summary>Kopie der URP-Einstellung mit geringerer Auflösung und gröberen Schatten.</summary>
        static RenderPipelineAsset BuildLowAsset(SettingsConfig s)
        {
            if (AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(LowAsset) == null) AssetDatabase.CopyAsset(HighAsset, LowAsset);
            var low = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(LowAsset);
            var so = new SerializedObject(low);
            so.FindProperty("m_RenderScale").floatValue = s.lowRenderScale;
            so.FindProperty("m_ShadowDistance").floatValue = s.lowShadowDistance;
            so.FindProperty("m_MainLightShadowmapResolution").intValue = s.lowShadowResolution;
            so.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = s.lowShadowResolution;
            so.FindProperty("m_ShadowCascadeCount").intValue = s.lowShadowCascades;
            so.FindProperty("m_SoftShadowsSupported").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            return low;
        }
    }
}
