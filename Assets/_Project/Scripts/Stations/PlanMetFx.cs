using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Plan erfüllt (Gruppe 2): Konfetti regnet von der Hallendecke, so lange der Direktor jubelt (feel.cheerSec).</summary>
    public class PlanMetFx : MonoBehaviour
    {
        [SerializeField] ParticleSystem confetti;

        float left;

        public void Setup(ParticleSystem ps) => confetti = ps;

        void OnEnable() => EventBus.QuotaMet += OnPlanMet;
        void OnDisable() => EventBus.QuotaMet -= OnPlanMet;

        void OnPlanMet()
        {
            var gm = GameManager.Instance;
            left = gm != null ? gm.Config.feel.cheerSec : 5f;
            confetti.Play(true);
        }

        void Update()
        {
            if (left <= 0f) return;
            left -= Time.unscaledDeltaTime;
            if (left <= 0f) confetti.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
