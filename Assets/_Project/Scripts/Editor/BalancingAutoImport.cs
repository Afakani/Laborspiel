using UnityEditor;

namespace PlanErfuellt.EditorTools
{
    /// <summary>Startet den BalancingImporter, sobald Balancing.json gespeichert wurde.</summary>
    public class BalancingAutoImport : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var path in imported)
            {
                if (path != BalancingImporter.JsonPath) continue;
                EditorApplication.delayCall += () => BalancingImporter.Import();
                return;
            }
        }
    }
}
