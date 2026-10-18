using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Bewegung der gebauten Maschinen (Prototyp userData.anim): Kolben der Melkpumpe arbeitet bei stehender Kuh,
    /// Lüfter des Kühlaggregats dreht, Lampe des Auto-Sprühers und Bildschirm des Notfall-Automaten pulsieren.
    /// </summary>
    public class MachineFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform piston;
        [SerializeField] Transform fan;
        [SerializeField] Renderer glow;
        [SerializeField] float pistonLift = 0.3f;
        [SerializeField] float pistonFreq = 3f;
        [SerializeField] float fanRadPerSec = 9f;
        [SerializeField] float glowFreq = 4f;

        MaterialPropertyBlock mpb;
        Color glowBase;
        Vector3 pistonRest;
        Chamber chamber;

        public void Setup(Transform pistonT, Transform fanT, Renderer glowR)
        {
            piston = pistonT;
            fan = fanT;
            glow = glowR;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            if (piston != null) pistonRest = piston.localPosition;
            if (glow != null) glowBase = glow.sharedMaterial.GetColor(EmissionId);
        }

        void Start() => chamber = FindAnyObjectByType<Chamber>();

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float t = gm.Clock.Time;
            float dt = gm.Clock.DeltaTime;
            if (piston != null)
            {
                bool cowOk = chamber != null && chamber.State == AnimalState.Ok && chamber.Kind == AnimalKind.Cow;
                piston.localPosition = pistonRest + Vector3.up * (cowOk ? Mathf.Abs(Mathf.Sin(t * pistonFreq)) * pistonLift : 0f);
            }
            if (fan != null) fan.Rotate(0f, 0f, dt * fanRadPerSec * Mathf.Rad2Deg, Space.Self);
            if (glow != null)
            {
                glow.GetPropertyBlock(mpb);
                mpb.SetColor(EmissionId, glowBase * (0.6f + 0.4f * Mathf.Sin(t * glowFreq)));
                glow.SetPropertyBlock(mpb);
            }
        }
    }
}
