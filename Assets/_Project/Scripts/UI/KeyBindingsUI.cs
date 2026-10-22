using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Tastenbelegung (aus den Einstellungen): je belegbarer Taste (settings.rebind) Name, aktuelle Taste und „Ändern“,
    /// dazu „Standard“ und „Zurück“. Esc schließt das Fenster bzw. bricht eine laufende Abfrage ab.
    /// </summary>
    public class KeyBindingsUI : MonoBehaviour
    {
        public static KeyBindingsUI Instance { get; private set; }

        [System.Serializable]
        public class Row
        {
            public TMP_Text name;
            public TMP_Text key;
            public Button change;
        }

        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text hint;
        [SerializeField] Row[] rows = new Row[0];
        [SerializeField] Button reset, back;
        [SerializeField] Color waitingColor = new Color(1f, 0.82f, 0.29f);
        [SerializeField] Color keyColor = new Color(0.55f, 1f, 0.45f);

        int waiting = -1;
        int endedFrame = -10;

        public bool IsOpen => root.activeSelf;
        /// <summary>Esc hat das Fenster in diesem Bild geschlossen (die Einstellungen bleiben dann offen).</summary>
        public bool ClosedThisFrame => closedFrame == Time.frameCount;

        int closedFrame = -10;

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
            for (int i = 0; i < rows.Length; i++)
            {
                int idx = i;
                rows[i].change.onClick.AddListener(() => Ask(idx));
            }
            reset.onClick.AddListener(() =>
            {
                KeyBindings.ResetAll();
                Refresh();
            });
            back.onClick.AddListener(Close);
        }

        public void Open()
        {
            var t = GameManager.Instance.Texts;
            title.text = t.Get("ui.settings.keysTitle");
            hint.text = t.Get("ui.settings.keyHint");
            Label(reset, t.Get("ui.settings.keyReset"));
            Label(back, t.Get("ui.settings.back"));
            root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (KeyBindings.Rebinding) return;
            root.SetActive(false);
            closedFrame = Time.frameCount;
        }

        void Update()
        {
            // Esc, mit dem eine Abfrage abgebrochen wurde, schließt das Fenster nicht gleich mit
            if (IsOpen && !KeyBindings.Rebinding && Time.frameCount > endedFrame + 1 && InputReader.PausePressed) Close();
        }

        void Ask(int i)
        {
            var entries = GameManager.Instance.Config.settings.rebind;
            if (i >= entries.Length || KeyBindings.Rebinding) return;
            waiting = i;
            Refresh();
            KeyBindings.Rebind(entries[i].action, entries[i].part, _ =>
            {
                waiting = -1;
                endedFrame = Time.frameCount;
                Refresh();
            });
        }

        void Refresh()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var entries = gm.Config.settings.rebind;
            for (int i = 0; i < rows.Length; i++)
            {
                bool on = i < entries.Length;
                rows[i].name.transform.parent.gameObject.SetActive(on);
                if (!on) continue;
                rows[i].name.text = t.Get("ui.settings.keyNames." + entries[i].id);
                bool wait = i == waiting;
                rows[i].key.text = wait ? t.Get("ui.settings.keyPress") : KeyBindings.Display(entries[i].action, entries[i].part);
                rows[i].key.color = wait ? waitingColor : keyColor;
                Label(rows[i].change, t.Get("ui.settings.keyChange"));
                rows[i].change.interactable = waiting < 0;
            }
            reset.interactable = back.interactable = waiting < 0;
        }

        static void Label(Button b, string text) => b.GetComponentInChildren<TMP_Text>().text = text;
    }
}
