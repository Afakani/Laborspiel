using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Quoten-Panel links (GDD 7.6, Prototyp quotaEl/currentHint): Plan und Status, Milch/Eier/Fleisch mit Balken,
    /// Sonderwunsch mit Countdown, Sonderauftrag, dazu der passende Hinweis (Einführung, Krisen, nächster Schritt).
    /// </summary>
    public class QuotaPanel : MonoBehaviour
    {
        [System.Serializable]
        public class GoodRow
        {
            public TMP_Text label;
            public TMP_Text value;
            public Image fill;
        }

        [SerializeField] GameObject root;
        [SerializeField] Image frame;
        [SerializeField] TMP_Text title;
        [SerializeField] GoodRow[] goods = new GoodRow[3];
        [SerializeField] TMP_Text wish;
        [SerializeField] TMP_Text contract;
        [SerializeField] TMP_Text hint;
        [SerializeField] Chamber chamber;
        [SerializeField] Color frameNormal = new Color(0.48f, 0.35f, 0.23f);
        [SerializeField] Color frameMet = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color frameLate = new Color(0.9f, 0.15f, 0.1f);
        [SerializeField] Color fillOk = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color fillOpen = new Color(1f, 0.82f, 0.29f);

        static readonly char[] Keys = { 'm', 'e', 'f' };
        static readonly string[] LabelKeys = { "ui.common.goodMilk", "ui.common.goodEggs", "ui.common.goodMeat" };

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var q = s.quota;
            bool show = q != null && DayCycle.Instance != null && DayCycle.Instance.Mode == DayMode.Play;
            if (root.activeSelf != show) root.SetActive(show);
            if (!show) return;
            var t = gm.Texts;
            var cfg = gm.Config;

            title.text = t.Format(q.met ? "ui.quota.titleMet" : "ui.quota.title", ("n", s.planIdx + 1), ("max", cfg.day.plans))
                         + "  ·  " + Clock(q.left);
            bool late = !q.met && q.left < cfg.day.lateWarningSec;
            frame.color = q.met ? frameMet : late ? Color.Lerp(frameNormal, frameLate, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 8f)) : frameNormal;

            for (int i = 0; i < 3; i++)
            {
                float v = Quota.Get(q.prod, Keys[i]), target = Quota.Get(q.target, Keys[i]);
                goods[i].label.text = t.Get(LabelKeys[i]);
                goods[i].value.text = $"{Mathf.FloorToInt(v)} / {target:0} t";
                goods[i].fill.fillAmount = target > 0f ? Mathf.Clamp01(v / target) : 1f;
                goods[i].fill.color = v >= target ? fillOk : fillOpen;
            }

            var w = s.wish;
            wish.gameObject.SetActive(w != null);
            if (w != null)
            {
                string text = DayCycle.Instance.WishText(w);
                int sec = Mathf.CeilToInt(Mathf.Max(0f, w.left));
                wish.text = w.Id == "meat" || w.Id == "eggs"
                    ? t.Format("ui.quota.wishProgress", ("text", text), ("got", Mathf.FloorToInt(w.got)), ("need", Mathf.RoundToInt(w.need)), ("sec", sec))
                    : t.Format("ui.quota.wish", ("text", text), ("sec", sec));
            }

            var c = s.contract;
            contract.gameObject.SetActive(c != null);
            if (c != null)
            {
                contract.text = c.got > 0f
                    ? t.Format("ui.quota.contractProgress", ("who", c.data.who), ("what", c.data.what), ("q", c.data.minQ), ("got", Mathf.FloorToInt(c.got)))
                    : t.Format("ui.quota.contract", ("who", c.data.who), ("what", c.data.what), ("q", c.data.minQ));
            }

            hint.text = Hint(gm, s);
        }

        static string Clock(float sec)
        {
            int s = Mathf.Max(0, Mathf.CeilToInt(sec));
            return $"{s / 60}:{s % 60:00}";
        }

        /// <summary>Wichtigster Hinweis in der Reihenfolge des Prototyps (currentHint).</summary>
        string Hint(GameManager gm, GameState s)
        {
            var t = gm.Texts;
            if (chamber != null && chamber.State == AnimalState.Unstable) return t.Get("ui.hints.unstable");
            if (s.infected) return t.Get(s.antidote > 0 ? "ui.hints.infectedHasAntidote" : "ui.hints.infectedNoAntidote");
            if (s.tutorialStep >= 0 && s.tutorialStep < DayLogic.TutorialSteps)
                return t.Format("ui.quota.tutorial", ("n", s.tutorialStep + 1), ("max", DayLogic.TutorialSteps),
                    ("text", t.Get($"director.tutorial[{s.tutorialStep}].step")));
            if (s.inspection != null && !s.rackHidden && InspectionLogic.RackSuspicious(s, gm.Config)) return t.Get("ui.hints.inspectRack");
            if (s.inspection != null && chamber != null && chamber.HasMutatedAnimal && !s.chamberCovered) return t.Get("ui.hints.inspectCloth");
            if (s.beltBroken) return t.Get("ui.hints.beltBroken");
            if (s.contaminated) return t.Get("ui.hints.contaminated");
            if (chamber == null || chamber.State == AnimalState.Empty) return t.Get("ui.hints.emptyChamber");
            if (chamber.State == AnimalState.Ok && chamber.Stats.dose.Total == 0 && !chamber.IsUltimate)
                return t.Get(s.serums.Count > 0 ? "ui.hints.spraySerum" : "ui.hints.mixSerum");
            if (chamber.State == AnimalState.Ok && s.quota != null && s.quota.prod.f < s.quota.target.f) return t.Get("ui.hints.meatOnlyProc");
            return t.Get("ui.hints.default");
        }
    }
}
