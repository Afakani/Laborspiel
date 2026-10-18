using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt
{
    /// <summary>
    /// Grafik Hoch/Niedrig: wechselt zwischen zwei URP-Einstellungen (Niedrig = geringere Auflösung, kürzere und
    /// gröbere Schatten; Werte in settings der Balancing-JSON, angelegt vom Editor-Werkzeug).
    /// </summary>
    public class GraphicsQuality : MonoBehaviour
    {
        [SerializeField] RenderPipelineAsset high;
        [SerializeField] RenderPipelineAsset low;

        public void Setup(RenderPipelineAsset highAsset, RenderPipelineAsset lowAsset)
        {
            high = highAsset;
            low = lowAsset;
        }

        void OnEnable()
        {
            GameSettings.Changed += Apply;
            Apply();
        }

        void OnDisable()
        {
            GameSettings.Changed -= Apply;
            // Projekt-Einstellung nicht dauerhaft überschreiben (Editor)
            QualitySettings.renderPipeline = null;
        }

        void Apply()
        {
            var want = GameSettings.HighQuality ? high : low;
            if (want != null && QualitySettings.renderPipeline != want) QualitySettings.renderPipeline = want;
        }
    }
}
