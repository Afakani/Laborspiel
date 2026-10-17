using System.Globalization;
using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Obere Leiste (GDD 7.6): Marke, Kasse, Rate, Tag und Plan. Setzt Texte nur bei Änderung.</summary>
    public class HudTopBar : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [SerializeField] TMP_Text logo;
        [SerializeField] TMP_Text brand;
        [SerializeField] TMP_Text cashLabel;
        [SerializeField] TMP_Text cash;
        [SerializeField] TMP_Text rate;
        [SerializeField] TMP_Text day;
        [SerializeField] TMP_Text plan;

        string lastCash, lastRate, lastDay, lastPlan;

        void Start()
        {
            var t = GameManager.Instance.Texts;
            logo.text = t.Get("ui.hud.brandLogo");
            brand.text = t.Get("ui.hud.brandName");
            cashLabel.text = t.Get("ui.hud.cash");
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var t = gm.Texts;
            var s = gm.State;
            Set(cash, ref lastCash, t.Format("ui.common.money", ("n", Mathf.RoundToInt(s.Money).ToString("N0", German))));
            Set(rate, ref lastRate, t.Format("ui.common.ratePerSec", ("n", s.currentRate.ToString("0.0", German))));
            Set(day, ref lastDay, t.Format("ui.hud.day", ("n", Mathf.Max(1, s.dayNum))));
            Set(plan, ref lastPlan, t.Format("ui.hud.plan", ("n", Mathf.Min(gm.Config.day.plans, s.planIdx + 1)), ("max", gm.Config.day.plans)));
        }

        static void Set(TMP_Text field, ref string last, string value)
        {
            if (value == last) return;
            last = value;
            field.text = value;
        }
    }
}
