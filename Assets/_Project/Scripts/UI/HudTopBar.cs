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
        // Zahlen des letzten Aufbaus: Texte nur bei Änderung formatieren (sonst jedes Bild Speichermüll)
        int shownMoney = int.MinValue, shownRate = int.MinValue, shownDay = int.MinValue, shownSec = int.MinValue, shownPlan = int.MinValue;

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
            int money = Mathf.RoundToInt(s.Money);
            if (money != shownMoney)
            {
                shownMoney = money;
                Set(cash, ref lastCash, t.Format("ui.common.money", ("n", money.ToString("N0", German))));
            }
            int rate10 = Mathf.RoundToInt(s.currentRate * 10f);
            if (rate10 != shownRate)
            {
                shownRate = rate10;
                Set(rate, ref lastRate, t.Format("ui.common.ratePerSec", ("n", (rate10 / 10f).ToString("0.0", German))));
            }
            int dayNum = Mathf.Max(1, s.dayNum);
            int sec = s.quota != null ? Mathf.Max(0, Mathf.CeilToInt(s.quota.left)) : -1;
            if (dayNum != shownDay || sec != shownSec)
            {
                shownDay = dayNum;
                shownSec = sec;
                string dayText = s.quota != null
                    ? t.Format("ui.hud.dayTime", ("n", dayNum), ("time", Clock(s.quota.left)))
                    : t.Format("ui.hud.day", ("n", dayNum));
                Set(day, ref lastDay, dayText);
            }
            int planNo = Mathf.Min(gm.Config.day.plans, s.planIdx + 1);
            if (planNo != shownPlan)
            {
                shownPlan = planNo;
                Set(plan, ref lastPlan, t.Format("ui.hud.plan", ("n", planNo), ("max", gm.Config.day.plans)));
            }
        }

        static string Clock(float sec)
        {
            int s = Mathf.Max(0, Mathf.CeilToInt(sec));
            return $"{s / 60}:{s % 60:00}";
        }

        static void Set(TMP_Text field, ref string last, string value)
        {
            if (value == last) return;
            last = value;
            field.text = value;
        }
    }
}
