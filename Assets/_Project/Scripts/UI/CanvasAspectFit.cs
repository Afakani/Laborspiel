using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Passt die UI-Skalierung an das Bildformat an: schmaler als die Referenz (16:10, 4:3, Laptop) richtet sich die
    /// Skalierung nach der Breite, damit nichts seitlich überlappt; breiter (21:9) nach der Höhe, damit nichts oben
    /// oder unten abgeschnitten wird. Bei 16:9 wie bisher.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasAspectFit : MonoBehaviour
    {
        CanvasScaler scaler;
        int lastW, lastH;

        void Awake() => scaler = GetComponent<CanvasScaler>();

        void Update()
        {
            if (Screen.width == lastW && Screen.height == lastH) return;
            lastW = Screen.width;
            lastH = Screen.height;
            var r = scaler.referenceResolution;
            float refAspect = r.x / Mathf.Max(1f, r.y);
            float aspect = lastW / Mathf.Max(1f, (float)lastH);
            scaler.matchWidthOrHeight = Match(aspect, refAspect);
        }

        /// <summary>0 = nach Breite, 1 = nach Höhe.</summary>
        public static float Match(float aspect, float refAspect) => aspect < refAspect - 0.01f ? 0f : 1f;
    }
}
