using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Einstellungen (Phase 12): Lautstärke Gesamt/Musik/Effekte, Mausempfindlichkeit, Kamerawackeln, Grafik Hoch/Niedrig, Vollbild/Fenster, Ton an/aus.
    /// Geöffnet aus Hauptmenü und Pause; Werte sofort in GameSettings. Esc/P schließt nur das Fenster.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }

        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text[] labels = new TMP_Text[7];
        [SerializeField] Slider master, music, sfx, mouse, shake;
        [SerializeField] Button graphicsHigh, graphicsLow, fullscreen, windowed, sound, back;
        [SerializeField] Color selectedColor = new Color(0.55f, 1f, 0.45f);
        [SerializeField] Color normalColor = new Color(0.96f, 0.93f, 0.82f);

        public bool IsOpen => root.activeSelf;

        void Awake()
        {
            Instance = this;
            root.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Start()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var s = gm.Config.settings;
            title.text = t.Get("ui.settings.title");
            string[] keys = { "master", "music", "sfx", "mouse", "shake", "graphics", "display" };
            for (int i = 0; i < labels.Length && i < keys.Length; i++) labels[i].text = t.Get("ui.settings." + keys[i]);
            Label(graphicsHigh, t.Get("ui.settings.high"));
            Label(graphicsLow, t.Get("ui.settings.low"));
            Label(fullscreen, t.Get("ui.settings.fullscreen"));
            Label(windowed, t.Get("ui.settings.windowed"));
            Label(back, t.Get("ui.settings.back"));
            foreach (var sl in new[] { master, music, sfx, shake }) Range(sl, 0f, 1f);
            Range(mouse, s.mouseMin, s.mouseMax);
            master.onValueChanged.AddListener(v => GameSettings.Master = v);
            music.onValueChanged.AddListener(v => GameSettings.Music = v);
            sfx.onValueChanged.AddListener(v => GameSettings.Sfx = v);
            mouse.onValueChanged.AddListener(v => GameSettings.Mouse = v);
            shake.onValueChanged.AddListener(v => GameSettings.Shake = v);
            graphicsHigh.onClick.AddListener(() => GameSettings.HighQuality = true);
            graphicsLow.onClick.AddListener(() => GameSettings.HighQuality = false);
            fullscreen.onClick.AddListener(() => GameSettings.Fullscreen = true);
            windowed.onClick.AddListener(() => GameSettings.Fullscreen = false);
            sound.onClick.AddListener(() => GameSettings.Muted = !GameSettings.Muted);
            back.onClick.AddListener(Close);
            GameSettings.Changed += Refresh;
        }

        void OnDisable() => GameSettings.Changed -= Refresh;

        static void Range(Slider s, float min, float max)
        {
            s.wholeNumbers = false;
            s.minValue = min;
            s.maxValue = max;
        }

        static void Label(Button b, string text) => b.GetComponentInChildren<TMP_Text>().text = text;

        public void Open()
        {
            root.SetActive(true);
            Refresh();
        }

        public void Close() => root.SetActive(false);

        void Update()
        {
            if (IsOpen && InputReader.PausePressed) Close();
        }

        void Refresh()
        {
            if (!IsOpen) return;
            master.SetValueWithoutNotify(GameSettings.Master);
            music.SetValueWithoutNotify(GameSettings.Music);
            sfx.SetValueWithoutNotify(GameSettings.Sfx);
            mouse.SetValueWithoutNotify(GameSettings.Mouse);
            shake.SetValueWithoutNotify(GameSettings.Shake);
            // gewählte Grafikstufe farbig hervorheben
            bool high = GameSettings.HighQuality;
            graphicsHigh.GetComponentInChildren<TMP_Text>().color = high ? selectedColor : normalColor;
            graphicsLow.GetComponentInChildren<TMP_Text>().color = high ? normalColor : selectedColor;
            bool full = GameSettings.Fullscreen;
            fullscreen.GetComponentInChildren<TMP_Text>().color = full ? selectedColor : normalColor;
            windowed.GetComponentInChildren<TMP_Text>().color = full ? normalColor : selectedColor;
            Label(sound, GameManager.Instance.Texts.Get(GameSettings.Muted ? "ui.hud.btnSoundOff" : "ui.hud.btnSoundOn"));
        }
    }
}
