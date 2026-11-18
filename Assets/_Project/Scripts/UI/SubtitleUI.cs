using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Untertitel über dem Prompt (GDD 7.6): „DIREKTOR: …“ bzw. „RADIO: …“ (Propaganda vom Grammophon), „DURCHSAGE: …“ (Partei, D157), Dauer nach
    /// Textlänge (ui.subtitle*).
    /// </summary>
    public class SubtitleUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text text;

        float left;

        void Awake() => root.SetActive(false);
        void OnEnable()
        {
            EventBus.DirectorSpeak += OnSpeak;
            EventBus.RadioSpeak += OnRadio;
            EventBus.AnnounceLine += OnAnnounce;
        }

        void OnDisable()
        {
            EventBus.DirectorSpeak -= OnSpeak;
            EventBus.RadioSpeak -= OnRadio;
            EventBus.AnnounceLine -= OnAnnounce;
        }

        void OnSpeak(string line) => Show("ui.hud.subtitleSpeaker", line);
        void OnRadio(string line) => Show("ui.hud.radioSpeaker", line);
        void OnAnnounce(string line) => Show("ui.hud.announceSpeaker", line);

        void Show(string speakerKey, string line)
        {
            var gm = GameManager.Instance;
            var ui = gm.Config.ui;
            text.text = gm.Texts.Get(speakerKey) + line;
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
