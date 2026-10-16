using TMPro;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>Erzeugt dynamische TextMeshPro-Font-Assets aus den TTF-Dateien (einmalig).</summary>
    public static class FontAssetCreator
    {
        [MenuItem("Plan erfüllt/Schriften für TextMeshPro erzeugen")]
        public static void CreateAll()
        {
            const string dir = BlockoutKit.FontDir;
            Ensure($"{dir}/Oswald/Oswald-Bold.ttf", $"{dir}/Oswald/Oswald-Bold SDF.asset");
            Ensure($"{dir}/Oswald/Oswald-Regular.ttf", $"{dir}/Oswald/Oswald-Regular SDF.asset");
            Ensure($"{dir}/JetBrainsMono/JetBrainsMono-Regular.ttf", $"{dir}/JetBrainsMono/JetBrainsMono-Regular SDF.asset");
            Ensure($"{dir}/JetBrainsMono/JetBrainsMono-Bold.ttf", $"{dir}/JetBrainsMono/JetBrainsMono-Bold SDF.asset");
            Ensure($"{dir}/Caveat/Caveat-Regular.ttf", $"{dir}/Caveat/Caveat-Regular SDF.asset");
            AssetDatabase.SaveAssets();
        }

        /// <summary>Lädt das Font-Asset oder legt es aus der TTF an.</summary>
        public static TMP_FontAsset Ensure(string ttfPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                Debug.LogError($"FontAssetCreator: Schrift fehlt: {ttfPath}");
                return null;
            }

            var fa = TMP_FontAsset.CreateFontAsset(font);
            fa.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(fa, assetPath);
            foreach (var tex in fa.atlasTextures)
            {
                tex.name = fa.name + " Atlas";
                AssetDatabase.AddObjectToAsset(tex, fa);
            }
            fa.material.name = fa.name + " Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            return fa;
        }
    }
}
