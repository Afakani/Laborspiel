using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Hauptmenü als eigener Vollbild-Bildschirm (rotes Band mit Titel und Knöpfen, Direktor-Plakat mit Parole):
    /// Weiterspielen (nur mit Spielstand), Neues Spiel, Einstellungen, Beenden. „Neues Spiel“ öffnet die
    /// Einsatzbesprechung (Texte ui.intro: Auftrag, Steuerung) mit „Für den Plan!“ und „Zurück“.
    /// </summary>
    public class IntroUI : MonoBehaviour
    {
        [Header("Hauptmenü")]
        [SerializeField] GameObject menuRoot;
        [SerializeField] TMP_Text menuTitle;
        [SerializeField] TMP_Text menuSubtitle;
        [SerializeField] TMP_Text slogan;
        [SerializeField] TMP_Text caption;
        [SerializeField] TMP_Text footer;
        [SerializeField] Button cont;
        [SerializeField] Button newGame;
        [SerializeField] Button settings;
        [SerializeField] Button quit;

        [Header("Einsatzbesprechung")]
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text subtitle;
        [SerializeField] TMP_Text body;
        [SerializeField] Button start;
        [SerializeField] TMP_Text startLabel;
        [SerializeField] Button back;

        bool shown, briefing;

        void Awake()
        {
            start.onClick.AddListener(() => DayCycle.Instance.StartFromIntro());
            back.onClick.AddListener(() => ShowBriefing(false));
            newGame.onClick.AddListener(() => ShowBriefing(true));
            cont.onClick.AddListener(() =>
            {
                if (!DayCycle.Instance.ContinueSaved()) cont.gameObject.SetActive(false);
            });
            settings.onClick.AddListener(() => SettingsUI.Instance.Open());
            quit.onClick.AddListener(Quit);
            menuRoot.SetActive(false);
            root.SetActive(false);
        }

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.Intro;
            if (want != shown)
            {
                shown = want;
                if (want)
                {
                    Fill();
                    ShowBriefing(false);
                }
                else
                {
                    menuRoot.SetActive(false);
                    root.SetActive(false);
                }
            }
            // Esc in der Besprechung: zurück ins Hauptmenü
            if (shown && briefing && InputReader.PausePressed && (SettingsUI.Instance == null || !SettingsUI.Instance.IsOpen))
                ShowBriefing(false);
        }

        void ShowBriefing(bool on)
        {
            briefing = on;
            menuRoot.SetActive(!on);
            root.SetActive(on);
        }

        void Fill()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            menuTitle.text = t.Get("ui.intro.title");
            menuSubtitle.text = t.Get("ui.intro.subtitle");
            slogan.text = t.Get("ui.signs.wallSlogan");
            caption.text = t.Get("ui.signs.directorCaption");
            footer.text = t.Get("ui.menu.footer");
            Label(newGame, t.Get("ui.menu.newGame"));
            Label(settings, t.Get("ui.intro.settings"));
            Label(quit, t.Get("ui.intro.quit"));
            // Weiterspielen nur mit gültigem Spielstand, mit Tag und Plan
            var save = SaveGame.Load();
            cont.gameObject.SetActive(save != null);
            if (save != null)
                Label(cont, t.Format("ui.intro.continue", ("day", save.state.dayNum + 1), ("plan", save.state.planIdx + 1), ("max", gm.Config.day.plans)));

            title.text = t.Get("ui.intro.title");
            subtitle.text = t.Get("ui.intro.subtitle");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(t.Get("ui.intro.text1")).AppendLine();
            sb.AppendLine(t.Get("ui.intro.text2")).AppendLine();
            foreach (var b in t.GetList("ui.intro.bullets")) sb.Append("• ").AppendLine(b);
            body.text = sb.ToString();
            startLabel.text = t.Get("ui.intro.start");
            Label(back, t.Get("ui.menu.back"));
        }

        static void Label(Button b, string text) => b.GetComponentInChildren<TMP_Text>().text = text;

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
