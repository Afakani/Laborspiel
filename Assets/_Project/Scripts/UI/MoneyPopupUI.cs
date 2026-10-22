using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Geld-Zahlen neben der Kasse (Gruppe 2): „+120 SM“ grün für Einnahmen (gesammelt, MoneyPopupLogic), „−40 SM“ rot
    /// für Ausgaben; steigen auf und verblassen (feel.moneyRiseSec, feel.moneyRisePx). Nur während der Schicht.
    /// </summary>
    public class MoneyPopupUI : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [SerializeField] TMP_Text template;
        [SerializeField] Color gain = new Color(0.45f, 1f, 0.4f);
        [SerializeField] Color loss = new Color(1f, 0.4f, 0.35f);

        class Popup
        {
            public TMP_Text text;
            public float age;
        }

        readonly MoneyPopupLogic logic = new MoneyPopupLogic();
        readonly List<Popup> active = new List<Popup>();
        readonly Stack<TMP_Text> pool = new Stack<TMP_Text>();
        Vector2 start;
        float lastMoney = float.NaN;

        void Awake()
        {
            start = template.rectTransform.anchoredPosition;
            template.gameObject.SetActive(false);
        }

        void OnEnable() => EventBus.MoneyChanged += OnMoney;
        void OnDisable() => EventBus.MoneyChanged -= OnMoney;

        static bool Playing => DayCycle.Instance != null && DayCycle.Instance.Mode == DayMode.Play;

        void OnMoney(float money)
        {
            // Sprünge außerhalb der Schicht (Laden, neues Spiel, Lieferung am Morgen) nicht zeigen
            if (!Playing || float.IsNaN(lastMoney))
            {
                lastMoney = money;
                logic.Clear();
                return;
            }
            float spent = logic.Add(money - lastMoney);
            lastMoney = money;
            if (spent < 0f) Show(spent);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var f = gm.Config.feel;
            float dt = Time.unscaledDeltaTime;
            if (Playing)
            {
                float got = logic.Tick(gm.Clock.DeltaTime, f);
                if (got > 0f) Show(got);
            }
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var p = active[i];
                p.age += dt;
                float k = p.age / Mathf.Max(0.1f, f.moneyRiseSec);
                if (k >= 1f)
                {
                    p.text.gameObject.SetActive(false);
                    pool.Push(p.text);
                    active.RemoveAt(i);
                    continue;
                }
                p.text.rectTransform.anchoredPosition = start + Vector2.up * (f.moneyRisePx * k);
                p.text.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            }
        }

        void Show(float amount)
        {
            var t = GameManager.Instance.Texts;
            var text = pool.Count > 0 ? pool.Pop() : Instantiate(template, template.transform.parent);
            text.gameObject.SetActive(true);
            string n = Mathf.RoundToInt(Mathf.Abs(amount)).ToString("N0", German);
            text.text = t.Format(amount >= 0f ? "ui.hud.moneyPlus" : "ui.hud.moneyMinus", ("n", n));
            text.color = amount >= 0f ? gain : loss;
            text.rectTransform.anchoredPosition = start;
            active.Add(new Popup { text = text });
        }
    }
}
