using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kamerawackeln wie im Prototyp (shake): Explosion, Explosion in der Verarbeitung, Bandriss, Kabelriss.
    /// Stärke und Abbau aus fx; der Versatz geht über FirstPersonController.ShakeOffset an die Kamera.
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    public class CameraShake : MonoBehaviour
    {
        FirstPersonController fpc;
        float shake;

        void Awake() => fpc = GetComponent<FirstPersonController>();
        void OnEnable() => EventBus.Risk += OnRisk;
        void OnDisable() => EventBus.Risk -= OnRisk;

        void OnRisk(RiskEvent e)
        {
            var fx = GameManager.Instance.Config.fx;
            float s = e == RiskEvent.Explosion ? fx.shakeExplosion
                : e == RiskEvent.ProcessingExplosion ? fx.shakeProcExplosion
                : e == RiskEvent.BeltBroken ? fx.shakeBelt
                : e == RiskEvent.CableTorn ? fx.shakeCable
                : 0f;
            shake = Mathf.Max(shake, s);
        }

        /// <summary>Wackeln von außen auslösen (z. B. Zentrifuge platzt, Plan verfehlt).</summary>
        public void Add(float amount) => shake = Mathf.Max(shake, amount);

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.Clock.DeltaTime <= 0f) return;
            var fx = gm.Config.fx;
            shake = Mathf.Max(0f, shake - gm.Clock.DeltaTime * fx.shakeDecay);
            fpc.ShakeOffset = shake > 0f
                ? new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0f) * (shake * fx.shakeScale)
                : Vector3.zero;
        }
    }
}
