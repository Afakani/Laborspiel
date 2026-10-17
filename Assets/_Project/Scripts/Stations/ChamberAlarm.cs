using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Rote Rundum-Warnleuchte auf dem Kopfring: dreht sich und leuchtet, solange das Tier INSTABIL ist
    /// (sichtbarer Teil der Sirene; der Ton folgt in Phase 12).
    /// </summary>
    public class ChamberAlarm : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Chamber chamber;
        [SerializeField] Transform rotor;
        [SerializeField] Light beam;
        [SerializeField] Renderer dome;
        [SerializeField] Color color = new Color(1f, 0.12f, 0.08f);
        [SerializeField] float beamIntensity = 18f;

        MaterialPropertyBlock mpb;
        float k;

        public void Setup(Chamber c, Transform rotorTransform, Light beamLight, Renderer domeRenderer)
        {
            chamber = c;
            rotor = rotorTransform;
            beam = beamLight;
            dome = domeRenderer;
        }

        void Awake() => mpb = new MaterialPropertyBlock();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float dt = gm.Clock.DeltaTime;
            bool on = chamber.State == AnimalState.Unstable;
            k = Mathf.MoveTowards(k, on ? 1f : 0f, dt * 4f);
            if (on) rotor.Rotate(0f, gm.Config.fx.alarmTurnDegPerSec * dt, 0f, Space.Self);
            beam.intensity = k * beamIntensity;
            beam.enabled = k > 0.01f;
            dome.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, color * (0.1f + k * 4f));
            dome.SetPropertyBlock(mpb);
        }
    }
}
