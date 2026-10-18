using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Zentrifuge sichtbar (GDD 6.3, Prototyp rotor/zenLamp): Rotor dreht mit der Drehzahl, Gehäuse wackelt über
    /// shakeAbove, Lampe gelb/grün/rot nach Zone; bei Überdrehzahl Glassplitter, Säurespritzer und Kamerawackeln.
    /// </summary>
    public class CentrifugeFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform rotor;
        [SerializeField] Transform housing;
        [SerializeField] Renderer lamp;
        [SerializeField] ParticleSystem shards;
        [SerializeField] ParticleSystem acid;
        [SerializeField] Color idleColor = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color yellowColor = new Color(1f, 0.8f, 0.16f);
        [SerializeField] Color greenColor = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color redColor = new Color(1f, 0.16f, 0.1f);

        MaterialPropertyBlock mpb;
        Vector3 housingPos;
        CameraShake shake;

        public void Setup(Transform rotorT, Transform housingT, Renderer lampR, ParticleSystem shardFx, ParticleSystem acidFx)
        {
            rotor = rotorT;
            housing = housingT;
            lamp = lampR;
            shards = shardFx;
            acid = acidFx;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            housingPos = housing.localPosition;
            shake = FindAnyObjectByType<CameraShake>();
        }

        void OnEnable() => EventBus.Machine += OnMachine;
        void OnDisable() => EventBus.Machine -= OnMachine;

        void OnMachine(MachineEvent e)
        {
            if (e != MachineEvent.CentrifugeOverspeed) return;
            shards.Emit(20);
            acid.Emit(14);
            if (shake != null) shake.Add(GameManager.Instance.Config.machineFx.centrifugeBreakShake);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var cfg = gm.Config;
            var fx = cfg.machineFx;
            var z = cfg.machinesFixed.centrifuge;
            var job = gm.State.centrifuge;
            float rpm = job != null ? job.rpm : 0f;
            float dt = gm.Clock.DeltaTime;
            float t = gm.Clock.Time;

            rotor.Rotate(0f, dt * rpm * fx.centrifugeRotorRadPerSec * Mathf.Rad2Deg, 0f, Space.Self);
            housing.localPosition = housingPos + (rpm > z.shakeAbove && dt > 0f
                ? new Vector3(Random.value - 0.5f, 0f, Random.value - 0.5f) * (fx.centrifugeShakeAmp * rpm)
                : Vector3.zero);

            var c = rpm >= z.zones.red[0] ? redColor : rpm >= z.zones.green[0] ? greenColor : rpm >= z.zones.tooEarly ? yellowColor : idleColor;
            float k = job != null ? 1.2f + 0.6f * Mathf.Sin(t * 12f) : 0.4f;
            lamp.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, c * k);
            lamp.SetPropertyBlock(mpb);
        }
    }
}
