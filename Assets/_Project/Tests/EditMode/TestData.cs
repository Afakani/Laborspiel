using PlanErfuellt.EditorTools;
using UnityEditor;
using UnityEngine;

namespace PlanErfuellt.Tests
{
    /// <summary>Lädt GameConfig und Texte für die Tests (Config wird bei Bedarf importiert).</summary>
    public static class TestData
    {
        public const string TextsPath = "Assets/_Project/Data/Json/Texte_de.json";

        public static GameConfig Config()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>(BalancingImporter.ConfigPath);
            return cfg != null ? cfg : BalancingImporter.Import();
        }

        public static TextDatabase Texts(int seed = 1)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(TextsPath);
            return new TextDatabase(asset.text, Config().anger.tiers, seed);
        }
    }
}
