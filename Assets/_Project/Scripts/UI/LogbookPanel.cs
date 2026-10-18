using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Forschungslogbuch (GDD 12), Taste L: Notizbuch mit Karten (Skizze, Name, Dosis, Vor- und Nachteile),
    /// Abschnitte Geheimakte, Kuriositäten, Rekordtafel. Nur Entdecktes. Karten werden aus einer Vorlage gebaut.
    /// </summary>
    public class LogbookPanel : PanelBase
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text intro;
        [SerializeField] TMP_Text closeLabel;
        [SerializeField] Button closeButton;
        [SerializeField] RectTransform content;
        [SerializeField] LogbookCard cardTemplate;
        [SerializeField] TMP_Text sectionTemplate;
        [SerializeField] Color proColor = new Color(0.16f, 0.45f, 0.15f);
        [SerializeField] Color conColor = new Color(0.62f, 0.12f, 0.1f);

        readonly List<GameObject> spawned = new List<GameObject>();
        string shownKey;

        protected override void Awake()
        {
            base.Awake();
            cardTemplate.gameObject.SetActive(false);
            sectionTemplate.gameObject.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        protected override void Update()
        {
            base.Update();
            var dc = DayCycle.Instance;
            var gm = GM;
            if (gm == null || dc == null) return;
            if (!InputReader.LogbookPressed) return;
            if (IsOpen) Close();
            else if (dc.Mode == DayMode.Play && !gm.PanelOpen && !gm.Clock.Paused && (BuildMode.Instance == null || !BuildMode.Instance.Active)) Open();
        }

        protected override void OnOpened()
        {
            var t = GM.Texts;
            title.text = t.Get("ui.logbook.title");
            intro.text = t.Get("ui.logbook.intro");
            closeLabel.text = t.Get("ui.logbook.close");
        }

        public override void Refresh(bool force)
        {
            var gm = GM;
            var s = gm.State;
            string key = $"{s.discoveredRecipes.Count}|{s.foundFunRecipes.Count}|{RecordsKey(s)}";
            if (!force && key == shownKey) return;
            shownKey = key;
            foreach (var go in spawned) Destroy(go);
            spawned.Clear();

            var t = gm.Texts;
            var entries = LogbookLogic.Build(s, gm.Config, t);
            bool anyCombo = entries.Exists(e => e.section == LogbookSection.Combo);
            if (!anyCombo) Section(t.Get("ui.logbook.empty"));
            var current = LogbookSection.Combo;
            foreach (var e in entries)
            {
                if (e.section != current)
                {
                    current = e.section;
                    Section(t.Get(current == LogbookSection.Secret ? "ui.logbook.secretFile" : current == LogbookSection.Curiosity ? "ui.logbook.curiosities" : "ui.logbook.records"));
                }
                var card = Instantiate(cardTemplate, content);
                card.gameObject.SetActive(true);
                card.Fill(e, Lists(e), gm.Config);
                spawned.Add(card.gameObject);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }

        static string RecordsKey(GameState s)
        {
            float sum = 0f;
            foreach (var v in s.records.Values) sum += v;
            return sum.ToString("0.0");
        }

        void Section(string text)
        {
            var h = Instantiate(sectionTemplate, content);
            h.gameObject.SetActive(true);
            h.text = text;
            spawned.Add(h.gameObject);
        }

        string Lists(LogbookEntry e)
        {
            var sb = new StringBuilder();
            string pc = ColorUtility.ToHtmlStringRGB(proColor), cc = ColorUtility.ToHtmlStringRGB(conColor);
            foreach (var p in e.pros) sb.Append("<color=#").Append(pc).Append(">+ ").Append(p).Append("</color>\n");
            foreach (var c in e.cons) sb.Append("<color=#").Append(cc).Append(">− ").Append(c).Append("</color>\n");
            return sb.ToString();
        }
    }
}
