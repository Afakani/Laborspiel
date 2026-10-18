using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kryokammer sichtbar (GDD 6.4, Prototyp kryWin/kryLamp): Fenster leuchtet beim Einfrieren, Kältenebel quillt
    /// aus der Tür, Lampe blinkt blau im Entnahmefenster (rot = gesperrt), nach verpasstem Fenster ein Eisblock.
    /// </summary>
    public class CryoFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Renderer window;
        [SerializeField] Renderer lamp;
        [SerializeField] ParticleSystem mist;
        [SerializeField] GameObject iceBlock;
        [SerializeField] Color lampColor = new Color(0.23f, 0.63f, 1f);
        [SerializeField] Color lockedColor = new Color(1f, 0.16f, 0.1f);
        [SerializeField] float mistRate = 14f;

        MaterialPropertyBlock mpb;
        Color windowEmission;
        float iceLeft;

        public void Setup(Renderer windowR, Renderer lampR, ParticleSystem mistFx, GameObject ice)
        {
            window = windowR;
            lamp = lampR;
            mist = mistFx;
            iceBlock = ice;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            windowEmission = window.sharedMaterial.GetColor(EmissionId);
            iceBlock.SetActive(false);
        }

        void OnEnable() => EventBus.Machine += OnMachine;
        void OnDisable() => EventBus.Machine -= OnMachine;

        void OnMachine(MachineEvent e)
        {
            if (e != MachineEvent.CryoIceBlock) return;
            iceLeft = GameManager.Instance.Config.machineFx.iceBlockShowSec;
            iceBlock.SetActive(true);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var fx = gm.Config.machineFx;
            var job = gm.State.cryo;
            float t = gm.Clock.Time;
            bool unlocked = gm.State.planIdx >= gm.Config.machinesFixed.cryo.unlockPlanIdx;
            bool freezing = job != null && !job.Ready;

            float w = freezing ? fx.cryoGlowFreezing + fx.cryoGlowPulse * Mathf.Sin(t * 3f) : job != null ? fx.cryoGlowFreezing : fx.cryoGlowIdle;
            window.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, windowEmission * w);
            window.SetPropertyBlock(mpb);

            float l = job != null ? (job.Ready ? (Mathf.Sin(t * 14f) > 0f ? 2f : 0.2f) : 1.2f) : unlocked ? 0.4f : 0.25f;
            lamp.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, (unlocked ? lampColor : lockedColor) * l);
            lamp.SetPropertyBlock(mpb);

            var em = mist.emission;
            em.rateOverTime = freezing && gm.Clock.DeltaTime > 0f ? mistRate : 0f;

            if (iceLeft > 0f)
            {
                iceLeft -= gm.Clock.DeltaTime;
                if (iceLeft <= 0f) iceBlock.SetActive(false);
            }
        }
    }
}
