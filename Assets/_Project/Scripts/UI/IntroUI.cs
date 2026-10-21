using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Hauptmenü als eigener Vollbild-Bildschirm (rotes Band mit Titel und Knöpfen, Direktor-Plakat mit Parole):
    /// Weiterspielen (nur mit Spielstand), Neues Spiel, Einstellungen, Beenden. „Neues Spiel“ öffnet die
    /// Einsatzbesprechung in Seiten (ui.intro.pages: Auftrag, Ablauf, Kontrolle, Gefahren, Steuerung) mit „Weiter“
    /// bzw. auf der letzten Seite „Für den Plan!“ (mit Einführung), „Ohne Einführung“ und „Zurück“.
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
        [SerializeField] Button startNoTutorial;

        bool shown, briefing;
        int page;

        void Awake()
        {
            start.onClick.AddListener(() =>
            {
                if (page < PageCount - 1) ShowPage(page + 1);
                else DayCycle.Instance.StartFromIntro(true);
            });
            startNoTutorial.onClick.AddListener(() => DayCycle.Instance.StartFromIntro(false));
            back.onClick.AddListener(() =>
            {
                if (page > 0) ShowPage(page - 1);
                else ShowBriefing(false);
            });
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
            if (on) ShowPage(0);
        }

        static int PageCount => GameManager.Instance.Texts.Count("ui.intro.pages");

        /// <summary>Eine Seite der Einsatzbesprechung: Überschrift, Text, „Weiter“ bzw. „Für den Plan!“.</summary>
        void ShowPage(int p)
        {
            var t = GameManager.Instance.Texts;
            int n = PageCount;
            page = Mathf.Clamp(p, 0, n - 1);
            subtitle.text = t.Format("ui.intro.pageOf", ("n", page + 1), ("max", n), ("title", t.Get($"ui.intro.pages[{page}].title")));
            body.text = t.Get($"ui.intro.pages[{page}].text");
            startLabel.text = t.Get(page < n - 1 ? "ui.intro.next" : "ui.intro.start");
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
            Label(back, t.Get("ui.menu.back"));
            Label(startNoTutorial, t.Get("ui.intro.startNoTutorial"));
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
