using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Auftragskarte zu Schichtbeginn (auf Wunsch nach Spieltest): Die Zeit steht, bis man „Schicht beginnen“ drückt
    /// (Knopf, F, Leertaste oder Esc). Zeigt das Tagessoll groß (Milch, Eier, Fleisch, Schichtlänge), den Ablauf in
    /// vier Schritten und die Hinweise des Tages (Kommissar ab Tag 2, Laken im Lager, neue Maschinen).
    /// </summary>
    public class DayStartUI : PanelBase
    {
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text goalHead;
        [SerializeField] TMP_Text[] goods = new TMP_Text[3];
        [SerializeField] TMP_Text time;
        [SerializeField] TMP_Text steps;
        [SerializeField] TMP_Text notes;
        [SerializeField] Button startButton;
        [SerializeField] TMP_Text startLabel;

        bool pausedByCard;

        protected override void Awake()
        {
            base.Awake();
            startButton.onClick.AddListener(Close);
        }

        protected override void OnOpened()
        {
            var gm = GM;
            var t = gm.Texts;
            var s = gm.State;
            var cfg = gm.Config;
            var q = s.quota;
            title.text = t.Format("ui.dayStart.title", ("day", s.dayNum), ("plan", s.planIdx + 1), ("max", cfg.day.plans));
            goalHead.text = t.Get("ui.dayStart.goalHead");
            if (q != null)
            {
                goods[0].text = t.Format("ui.dayStart.milk", ("n", Mathf.RoundToInt(q.target.m)));
                goods[1].text = t.Format("ui.dayStart.eggs", ("n", Mathf.RoundToInt(q.target.e)));
                goods[2].text = t.Format("ui.dayStart.meat", ("n", Mathf.RoundToInt(q.target.f)));
            }
            time.text = t.Format("ui.dayStart.time", ("min", Mathf.RoundToInt(cfg.day.dayLengthSec / 60f)));
            steps.text = t.Get("ui.dayStart.steps");
            notes.text = Notes(gm);
            startLabel.text = t.Format("ui.dayStart.start", ("key", KeyBindings.Display("Player/Interact")));
            // Zeit anhalten, solange die Karte offen ist (das Pausenmenü bleibt zu)
            if (!gm.Clock.Paused)
            {
                gm.Clock.SetPaused(true);
                pausedByCard = true;
            }
        }

        protected override void OnClosed()
        {
            if (pausedByCard && GM != null) GM.Clock.SetPaused(false);
            pausedByCard = false;
        }

        /// <summary>Hinweise des Tages je nach Stand.</summary>
        static string Notes(GameManager gm)
        {
            var t = gm.Texts;
            var s = gm.State;
            var cfg = gm.Config;
            var sb = new StringBuilder();
            void Line(string text) => sb.Append("• ").AppendLine(text);
            if (s.dayNum >= cfg.inspection.fromDay)
                Line(t.Format(s.cloths > 0 ? "ui.dayStart.inspection" : "ui.dayStart.inspectionNoCloth", ("n", s.cloths)));
            else
                Line(t.Format("ui.dayStart.inspectionLater", ("n", cfg.inspection.fromDay)));
            var m = cfg.machinesFixed;
            if (s.planIdx == m.centrifuge.unlockPlanIdx && s.dayNum >= 2) Line(t.Get("ui.dayStart.centrifuge"));
            if (s.planIdx == m.cryo.unlockPlanIdx) Line(t.Get("ui.dayStart.cryo"));
            if (s.planIdx == m.ultimate.unlockPlanIdx) Line(t.Get("ui.dayStart.ultimate"));
            if (s.failedDays > 0 && s.lastDayResult < 0) Line(t.Get("ui.dayStart.repeat"));
            Line(t.Get("ui.dayStart.order"));
            return sb.ToString();
        }

        public override void Refresh(bool force) { }
    }
}
