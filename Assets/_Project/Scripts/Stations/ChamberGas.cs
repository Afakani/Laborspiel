using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Gas in der Kammer: beim Sprühen kurz dichter und in Serumfarbe, Düsenringe leuchten mit (Prototyp sprayTank).
    /// Während der Vernichtung (ChamberFire) tritt das Gas zurück.
    /// </summary>
    public class ChamberGas : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Renderer gas;
        [SerializeField] Renderer[] nozzleRings = new Renderer[0];

        MaterialPropertyBlock mpb;
        Color color = new Color(0.43f, 1f, 0.29f);
        float flash;

        /// <summary>Feuer-Anteil 0..1 (ChamberFire): Gas tritt zurück, Düsenringe glühen orange.</summary>
        public float FireAmount { get; set; }

        void Awake() => mpb = new MaterialPropertyBlock();
        void OnEnable() => EventBus.SerumSprayed += OnSprayed;
        void OnDisable() => EventBus.SerumSprayed -= OnSprayed;

        void OnSprayed(Color c)
        {
            color = c;
            flash = GameManager.Instance.Config.animal.gasFlashSec;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var v = gm.Config.animalVisuals;
            float total = gm.Config.animal.gasFlashSec;
            flash = Mathf.Max(0f, flash - gm.Clock.DeltaTime);
            float k = total > 0f ? flash / total : 0f;
            float alpha = Mathf.Lerp(v.gasIdleAlpha, v.gasFlashAlpha, k) * (1f - FireAmount);

            ColorUtility.TryParseHtmlString(gm.Config.chamberFire.nozzleColor, out var fireColor);
            var ringColor = Color.Lerp(color, fireColor, FireAmount);
            var c = color;
            c.a = alpha;
            gas.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionId, color * (0.4f + 1.2f * k));
            gas.SetPropertyBlock(mpb);

            foreach (var r in nozzleRings)
            {
                r.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, ringColor);
                mpb.SetColor(EmissionId, ringColor * (1f + 2f * Mathf.Max(k, FireAmount)));
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
