using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Probenmikroskop sichtbar (Prototyp micDish): Die Schale leuchtet, während die Probe untersucht wird.</summary>
    public class MicroscopeFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Renderer dish;
        [SerializeField] float activeBoost = 5f;

        MaterialPropertyBlock mpb;
        Color baseEmission;

        public void Setup(Renderer dishR) => dish = dishR;

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            baseEmission = dish.sharedMaterial.GetColor(EmissionId);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float t = gm.Clock.Time;
            float k = gm.State.probeLeft > 0f ? (0.6f + 0.4f * Mathf.Sin(t * gm.Config.machineFx.probeGlowPulse)) * activeBoost : 0.25f;
            dish.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, baseEmission * k);
            dish.SetPropertyBlock(mpb);
        }
    }
}
