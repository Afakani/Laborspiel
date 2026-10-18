using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Direktor-Untertitel über dem Prompt (GDD 7.6): „DIREKTOR: …“, Dauer nach Textlänge (ui.subtitle*).</summary>
    public class SubtitleUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text text;

        float left;

        void Awake() => root.SetActive(false);
        void OnEnable() => EventBus.DirectorSpeak += OnSpeak;
        void OnDisable() => EventBus.DirectorSpeak -= OnSpeak;

        void OnSpeak(string line)
        {
            var gm = GameManager.Instance;
            var ui = gm.Config.ui;
            text.text = gm.Texts.Get("ui.hud.subtitleSpeaker") + line;
            left = Mathf.Max(ui.subtitleMinSec, line.Length * ui.subtitlePerChar);
            root.SetActive(true);
        }

        void Update()
        {
            if (!root.activeSelf) return;
            var gm = GameManager.Instance;
            if (gm.Clock.Paused) return;
            left -= Time.unscaledDeltaTime;
            if (left <= 0f || (DayCycle.Instance != null && DayCycle.Instance.Mode == DayMode.GameOver)) root.SetActive(false);
        }
    }
}
