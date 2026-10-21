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

        /// <summary>
        /// Kopie der Config mit den Formelwerten des Prototyps für Wachstum A (Stabilität −14, Qualität −8, Ertrag +0,15,
        /// Fleisch +0,7 je Dosis). Die Referenztests (Prompt 4, GDD) prüfen damit die Umsetzung der Formeln; das
        /// Spiel nutzt das stärkere Balancing aus der JSON (D116).
        /// </summary>
        public static GameConfig PrototypeConfig()
        {
            var c = Object.Instantiate(Config());
            var f = c.formulas;
            f.stability.perA = 14f;
            f.quality.perA = 8f;
            f.rate.perA = 0.15f;
            f.meat.perA = 0.7f;
            // im Prototyp wirkt B bei Kuh und Huhn gleich
            f.ratePerBHen = f.rate.perB;
            return c;
        }

        public static TextDatabase Texts(int seed = 1)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(TextsPath);
            return new TextDatabase(asset.text, Config().anger.tiers, seed);
        }
    }
}
