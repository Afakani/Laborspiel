using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Pause mit P oder Esc (GDD 2). Ist ein Panel offen, schließt Esc/P zuerst das Panel (ab Phase 4a über PanelCloseRequested).
    /// Knöpfe Logbuch und Staats-TV (GDD 11: nur nach der ersten Mutation) führen aus der Pause dorthin;
    /// dazu Ton an/aus (M), Einstellungen, Hauptmenü (lädt die Szene neu; gespeichert wird zu Schichtbeginn) und Spiel beenden.
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
        [SerializeField] Button mainMenu;
        [SerializeField] Button quit;
        [SerializeField] TMP_Text saveHint;
        [SerializeField] Button skipTutorial;

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
            mainMenu.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnMainMenu");
            quit.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnQuit");
            mainMenu.onClick.AddListener(ToMainMenu);
            skipTutorial.GetComponentInChildren<TMP_Text>().text = gm.Texts.Get("ui.hud.btnSkipTutorial");
            skipTutorial.onClick.AddListener(() =>
            {
                TutorialLogic.Skip(GameManager.Instance.State);
                skipTutorial.gameObject.SetActive(false);
            });
            quit.onClick.AddListener(Quit);
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

        /// <summary>Zurück ins Hauptmenü: Labor neu laden (Fortschritt seit Schichtbeginn verfällt).</summary>
        static void ToMainMenu()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(scene.buildIndex);
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnPauseChanged(bool paused)
        {
            // Die Auftragskarte zu Schichtbeginn hält die Zeit an, ohne das Pausenmenü zu zeigen
            overlay.SetActive(paused && PanelBase.OpenId != "dayStart");
            if (!paused) return;
            var gm = GameManager.Instance;
            skipTutorial.gameObject.SetActive(TutorialLogic.Active(gm.State, gm.Config) || gm.State.hintsOn);
            var save = SaveGame.Load();
            saveHint.text = save != null
                ? gm.Texts.Format("ui.hud.saveHint", ("day", save.state.dayNum + 1))
                : gm.Texts.Get("ui.hud.saveHintNone");
            var dc = DayCycle.Instance;
            bool inShift = dc != null && dc.Mode == DayMode.Play;
            logbook.gameObject.SetActive(inShift);
            news.gameObject.SetActive(inShift && (gm.State.totalDoses > 0 || gm.State.ultimateSeen));
        }
    }
}
