using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Pause mit P oder Esc (GDD 2). Ist ein Panel offen, schließt Esc/P zuerst das Panel (ab Phase 4a über PanelCloseRequested).
    /// Knöpfe Logbuch und Staats-TV (GDD 11: nur nach der ersten Mutation) führen aus der Pause dorthin;
    /// dazu Ton an/aus (M) und Einstellungen.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text hint;
        [SerializeField] Button logbook;
        [SerializeField] Button news;
        [SerializeField] Button sound;
        [SerializeField] Button settings;

        void Start()
        {
            var gm = GameManager.Instance;
            title.text = gm.Texts.Get("ui.hud.pause");
            hint.text = gm.Texts.Get("ui.hud.pauseHint");
            logbook.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnLogbook");
            news.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnNews");
            logbook.onClick.AddListener(() =>
            {
                gm.Clock.SetPaused(false);
                EventBus.RaisePanelRequested("logbook");
            });
            news.onClick.AddListener(() =>
            {
                gm.Clock.SetPaused(false);
                DayCycle.Instance.ShowNews(false);
            });
            settings.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnSettings");
            settings.onClick.AddListener(() => SettingsUI.Instance.Open());
            sound.onClick.AddListener(() => GameSettings.Muted = !GameSettings.Muted);
            SoundLabel();
            overlay.SetActive(gm.Clock.Paused);
            EventBus.PauseChanged += OnPauseChanged;
            GameSettings.Changed += SoundLabel;
        }

        void OnDestroy()
        {
            EventBus.PauseChanged -= OnPauseChanged;
            GameSettings.Changed -= SoundLabel;
        }

        void SoundLabel() =>
            sound.GetComponentInChildren<TMP_Text>().text = GameManager.Instance.Texts.Get(GameSettings.Muted ? "ui.hud.btnSoundOff" : "ui.hud.btnSoundOn");

        void Update()
        {
            if (!InputReader.PausePressed) return;
            // Esc schließt zuerst das Einstellungsfenster (SettingsUI)
            if (SettingsUI.Instance != null && SettingsUI.Instance.IsOpen) return;
            var gm = GameManager.Instance;
            if (gm.PanelOpen)
            {
                EventBus.RaisePanelCloseRequested();
                return;
            }
            gm.Clock.TogglePause();
        }

        void OnPauseChanged(bool paused)
        {
            overlay.SetActive(paused);
            if (!paused) return;
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            bool inShift = dc != null && dc.Mode == DayMode.Play;
            logbook.gameObject.SetActive(inShift);
            news.gameObject.SetActive(inShift && (gm.State.totalDoses > 0 || gm.State.ultimateSeen));
        }
    }
}
