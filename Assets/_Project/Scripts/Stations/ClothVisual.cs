using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Laken über der Kammer (GDD 6.8, Prototyp clothK): fällt beim Werfen von oben herab und wird beim Abnehmen
    /// wieder hochgezogen. Es verrät das Tier: wackelt je nach Mutationsgrad, zittert bei INSTABIL, leuchtet golden bei Ω.
    /// Der Drehpunkt des Laken-Objekts liegt oben am Kopfring.
    /// </summary>
    public class ClothVisual : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform cloth;
        [SerializeField] Renderer clothRenderer;
        [SerializeField] Chamber chamber;

        MaterialPropertyBlock mpb;
        Color omegaColor;
        float k;

        public void Setup(Transform clothTransform, Renderer renderer, Chamber chamberRef)
        {
            cloth = clothTransform;
            clothRenderer = renderer;
            chamber = chamberRef;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            cloth.gameObject.SetActive(false);
        }

        void Start() => ColorUtility.TryParseHtmlString(GameManager.Instance.Config.clothFx.omegaColor, out omegaColor);

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var c = gm.Config.clothFx;
            var s = gm.State;
            k = Mathf.Lerp(k, s.chamberCovered ? 1f : 0f, Mathf.Min(1f, Time.deltaTime * c.speed));
            bool on = k > 0.02f;
            if (cloth.gameObject.activeSelf != on) cloth.gameObject.SetActive(on);
            if (!on) return;

            var sc = cloth.localScale;
            cloth.localScale = new Vector3(sc.x, Mathf.Max(0.02f, k), sc.z);

            // Verräterische Bewegung
            float t = s.playTime;
            var v = chamber != null ? chamber.View : default;
            float roll = 0f;
            bool omega = false;
            if (s.chamberCovered && v.mutated)
            {
                float lvl = v.Level(gm.Config.inspection.susp);
                roll = Mathf.Sin(t * (c.wobbleBaseFreq + lvl)) * c.wobbleDegPerLevel * lvl;
                if (v.unstable) roll += Mathf.Sin(t * c.unstableFreq) * c.unstableDeg;
                omega = v.IsOmega;
            }
            cloth.localRotation = Quaternion.Euler(0f, 0f, roll);

            clothRenderer.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, omega ? omegaColor * (c.omegaGlow + c.omegaPulse * Mathf.Sin(t * c.omegaFreq)) : Color.black);
            clothRenderer.SetPropertyBlock(mpb);
        }
    }
}
