using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>Roter Bildschirmrand (GDD 7.6): pulsiert bei cholerischem Direktor; wenn die Schicht gleich endet, sanfter und langsamer.</summary>
    public class RageBorder : MonoBehaviour
    {
        [SerializeField] RawImage border;
        [SerializeField] Color color = new Color(0.9f, 0.08f, 0.05f, 1f);
        [SerializeField] float maxAlpha = 0.55f;
        [SerializeField] float pulseFreq = 2.2f;
        [Header("Schichtende (sanfter)")]
        [SerializeField] float lateMaxAlpha = 0.22f;
        [SerializeField] float latePulseFreq = 0.8f;

        float k, phase;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var cfg = gm.Config;
            bool play = DayCycle.Instance != null && DayCycle.Instance.Mode == DayMode.Play;
            bool rage = s.anger >= cfg.anger.tiers[1];
            bool late = s.quota != null && !s.quota.met && s.quota.left < cfg.day.lateWarningSec;
            k = Mathf.MoveTowards(k, play && (rage || late) ? 1f : 0f, Time.unscaledDeltaTime * 2f);
            float a = rage ? maxAlpha : lateMaxAlpha, f = rage ? pulseFreq : latePulseFreq;
            phase += Time.unscaledDeltaTime * f * Mathf.PI * 2f;
            var c = color;
            c.a = k * a * (0.55f + 0.45f * Mathf.Sin(phase));
            border.color = c;
            border.enabled = c.a > 0.005f;
        }
    }
}
