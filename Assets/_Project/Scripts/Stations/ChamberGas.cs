using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Gas in der Kammer: beim Sprühen kurz dichter und in Serumfarbe, Düsenringe leuchten mit (Prototyp sprayTank).
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
            float alpha = Mathf.Lerp(v.gasIdleAlpha, v.gasFlashAlpha, k);

            var c = color;
            c.a = alpha;
            gas.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            mpb.SetColor(EmissionId, color * (0.4f + 1.2f * k));
            gas.SetPropertyBlock(mpb);

            foreach (var r in nozzleRings)
            {
                r.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, color);
                mpb.SetColor(EmissionId, color * (1f + 2f * k));
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
