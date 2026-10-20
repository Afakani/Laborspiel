using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt
{
    /// <summary>
    /// Grafik Hoch/Niedrig: wechselt zwischen zwei URP-Einstellungen (Niedrig = geringere Auflösung, kürzere und
    /// gröbere Schatten; Werte in settings der Balancing-JSON, angelegt vom Editor-Werkzeug).
    /// Dazu Vollbild (randlos, Bildschirmauflösung) oder Fenster (Größe aus settings); nur bei Änderung umgeschaltet.
    /// </summary>
    public class GraphicsQuality : MonoBehaviour
    {
        [SerializeField] RenderPipelineAsset high;
        [SerializeField] RenderPipelineAsset low;

        bool? appliedFullscreen;

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
            ApplyDisplay();
        }

        bool startChecked;

        void ApplyDisplay()
        {
            bool full = GameSettings.Fullscreen;
            if (appliedFullscreen == full) return;
            appliedFullscreen = full;
            if (Application.isEditor) return; // im Editor bestimmt die Game View die Größe
            // Startoption -screen-width/-screen-height (z. B. zum Testen von 4:3 oder 16:10) gilt beim Start vor der Einstellung
            if (!startChecked)
            {
                startChecked = true;
                if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-screen-width") >= 0) return;
            }
            if (full)
            {
                var r = Screen.currentResolution;
                Screen.SetResolution(r.width, r.height, FullScreenMode.FullScreenWindow);
            }
            else
            {
                var gm = GameManager.Instance;
                if (gm == null) return;
                var s = gm.Config.settings;
                Screen.SetResolution(s.windowedWidth, s.windowedHeight, FullScreenMode.Windowed);
            }
        }
    }
}
