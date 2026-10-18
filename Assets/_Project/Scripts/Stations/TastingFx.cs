using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Verkostungsautomat sichtbar (Prototyp tasteLamp): Lampe pulsiert, wenn ein ungeprüftes Tier bereitsteht,
    /// der Stempelarm schlägt beim Siegel einmal nach unten.
    /// </summary>
    public class TastingFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Transform stampArm;
        [SerializeField] Renderer lamp;
        [SerializeField] float stampDeg = 35f;
        [SerializeField] Color lampColor = new Color(1f, 0.7f, 0.15f);

        MaterialPropertyBlock mpb;
        Chamber chamber;
        Quaternion armRest;
        float stampLeft;

        public void Setup(Transform arm, Renderer lampR)
        {
            stampArm = arm;
            lamp = lampR;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            armRest = stampArm.localRotation;
        }

        void Start() => chamber = FindAnyObjectByType<Chamber>();

        void OnEnable() => EventBus.Station += OnStation;
        void OnDisable() => EventBus.Station -= OnStation;

        void OnStation(string id)
        {
            if (id == "tasting") stampLeft = GameManager.Instance.Config.machineFx.stampSec;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var cfg = gm.Config;
            float t = gm.Clock.Time;
            if (stampLeft > 0f) stampLeft = Mathf.Max(0f, stampLeft - Time.deltaTime);
            float k = stampLeft > 0f ? Mathf.Sin(stampLeft / cfg.machineFx.stampSec * Mathf.PI) : 0f;
            stampArm.localRotation = armRest * Quaternion.Euler(stampDeg * k, 0f, 0f);

            bool unlocked = gm.State.planIdx >= cfg.machinesFixed.tasting.unlockPlanIdx;
            bool ready = chamber != null && chamber.State == AnimalState.Ok && !chamber.Stats.stamped;
            float e = !unlocked ? 0.05f : ready ? 0.6f + 0.5f * Mathf.Sin(t * 5f) : 0.3f;
            lamp.GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, lampColor * e);
            lamp.SetPropertyBlock(mpb);
        }
    }
}
