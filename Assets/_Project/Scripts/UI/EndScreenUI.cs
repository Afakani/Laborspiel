using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Endbildschirm (GDD 13, Prototyp gameOver): Titel und Text der Endung aus endings.*, Statistik in zwei Spalten,
    /// Knopf „Neuer Fünfjahresplan“.
    /// </summary>
    public class EndScreenUI : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text text;
        [SerializeField] TMP_Text statLabels;
        [SerializeField] TMP_Text statValues;
        [SerializeField] Button again;
        [SerializeField] TMP_Text againLabel;
        [SerializeField] float lineHeightFactor = 1.45f;

        bool shown;

        void Awake()
        {
            again.onClick.AddListener(() => DayCycle.Instance.NewGame());
            root.SetActive(false);
        }

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.GameOver;
            if (want == shown) return;
            shown = want;
            root.SetActive(want);
            if (want) Fill(dc.EndKind);
        }

        void Fill(GameOverKind kind)
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var s = gm.State;
            var cfg = gm.Config;
            string key = WorldValues.EndingKey(kind, s, cfg);
            title.text = t.Get($"endings.{key}.title");
            text.text = t.Get($"endings.{key}.text");
            var w = WorldValues.Compute(s, cfg);
            var l = new StringBuilder();
            var v = new StringBuilder();
            void Row(string label, string value)
            {
                l.AppendLine(t.Get("ui.endScreen." + label));
                v.AppendLine(value);
            }
            Row("statPlans", $"{Mathf.Min(s.planIdx, cfg.day.plans)} / {cfg.day.plans}");
            Row("statShifts", s.dayNum.ToString());
            Row("statFailed", s.quotasFailed.ToString());
            Row("statPop", t.Format("ui.common.percent", ("n", Mathf.RoundToInt(w.pop))));
            Row("statGoods", $"{Mathf.RoundToInt(s.totals.m)} / {Mathf.RoundToInt(s.totals.e)} / {Mathf.RoundToInt(s.totals.f)} t");
            Row("statCash", t.Format("ui.common.money", ("n", Mathf.RoundToInt(s.Money).ToString("N0", German))));
            Row("statTries", s.experiments.ToString());
            Row("statExplosions", s.explosions.ToString());
            Row("statMachines", s.machinesBuilt.ToString());
            int sec = Mathf.RoundToInt(s.playTime);
            Row("statPlaytime", $"{sec / 60}:{sec % 60:00}");
            // Beide Spalten mit derselben festen Zeilenhöhe, sonst laufen die zwei Schriften auseinander
            string lineHeight = $"<line-height={(statLabels.fontSize * lineHeightFactor).ToString(CultureInfo.InvariantCulture)}>";
            statLabels.text = lineHeight + l;
            statValues.text = lineHeight + v;
            againLabel.text = t.Get("ui.endScreen.again");
        }
    }
}
