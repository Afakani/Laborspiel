using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>Startbildschirm und Hauptmenü vor Tag 1 (Texte ui.intro): Titel, Auftrag, Steuerung, Knöpfe Start, Einstellungen, Beenden.</summary>
    public class IntroUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text subtitle;
        [SerializeField] TMP_Text body;
        [SerializeField] Button start;
        [SerializeField] TMP_Text startLabel;
        [SerializeField] Button settings;
        [SerializeField] Button quit;

        bool shown;

        void Awake()
        {
            start.onClick.AddListener(() => DayCycle.Instance.StartFromIntro());
            settings.onClick.AddListener(() => SettingsUI.Instance.Open());
            quit.onClick.AddListener(Quit);
            root.SetActive(false);
        }

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.Intro;
            if (want == shown) return;
            shown = want;
            root.SetActive(want);
            if (!want) return;
            var t = GameManager.Instance.Texts;
            title.text = t.Get("ui.intro.title");
            subtitle.text = t.Get("ui.intro.subtitle");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(t.Get("ui.intro.text1")).AppendLine();
            sb.AppendLine(t.Get("ui.intro.text2")).AppendLine();
            foreach (var b in t.GetList("ui.intro.bullets")) sb.Append("• ").AppendLine(b);
            body.text = sb.ToString();
            startLabel.text = t.Get("ui.intro.start");
            settings.GetComponentInChildren<TMP_Text>().text = t.Get("ui.intro.settings");
            quit.GetComponentInChildren<TMP_Text>().text = t.Get("ui.intro.quit");
        }

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
