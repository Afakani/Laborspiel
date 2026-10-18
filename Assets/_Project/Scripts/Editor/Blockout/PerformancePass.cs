using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Performance-Pass nach GDD 15 am Ende des Laborbaus: kleine Teile (Schrauben, Gläser, Bücher …) werfen keinen
    /// Schatten mehr („Schatten nur für große Objekte“), statisches Batching für den Build ist an.
    /// Tiere, Spieler und bewegte Teile bleiben unverändert.
    /// </summary>
    public static class PerformancePass
    {
        /// <summary>Teile mit kleinerer Bounds-Diagonale werfen keinen Schatten (m, eigener Wert).</summary>
        const float MinShadowSize = 0.6f;

        public static string Apply(params Transform[] roots)
        {
            int off = 0, kept = 0;
            foreach (var root in roots)
            {
                if (root == null) continue;
                foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (r.shadowCastingMode == ShadowCastingMode.Off || r.GetComponentInParent<AnimalVisuals>(true) != null) continue;
                    if (r.bounds.size.magnitude < MinShadowSize)
                    {
                        r.shadowCastingMode = ShadowCastingMode.Off;
                        off++;
                    }
                    else kept++;
                }
            }
            PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.StandaloneWindows64, true);
            return $"Schatten aus: {off}, Schattenwerfer: {kept}";
        }
    }
}
