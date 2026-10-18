using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Bestellterminal zu Schichtbeginn (GDD 6.9, Prototyp openOrder/renderOrder/confirmOrder): sechs Artikel mit −/+,
    /// Lagerbestand, Preis, Summe und Kasse; Info mit Soll und Infektionsfrist. Bestätigen startet die Schicht.
    /// </summary>
    public class OrderTerminalUI : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");
        static readonly string[] ItemKeys = { "A", "B", "C", "Antidote", "Vodka", "Caviar" };

        [System.Serializable]
        public class Row
        {
            public TMP_Text name;
            public TMP_Text desc;
            public TMP_Text stock;
            public TMP_Text count;
            public TMP_Text price;
            public Button minus;
            public Button plus;
        }

        [SerializeField] GameObject root;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text info;
        [SerializeField] TMP_Text sum;
        [SerializeField] Button confirm;
        [SerializeField] TMP_Text confirmLabel;
        [SerializeField] Row[] rows = new Row[6];

        readonly int[] amounts = new int[6];
        readonly string[] names = new string[6];
        bool shown;

        void Awake()
        {
            for (int i = 0; i < rows.Length; i++)
            {
                int idx = i;
                rows[i].minus.onClick.AddListener(() => Change(idx, -1));
                rows[i].plus.onClick.AddListener(() => Change(idx, 1));
            }
            confirm.onClick.AddListener(OnConfirm);
            root.SetActive(false);
        }

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.Order;
            if (want != shown)
            {
                shown = want;
                root.SetActive(want);
                if (want) Open();
            }
        }

        void Open()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var s = gm.State;
            var cfg = gm.Config;
            for (int i = 0; i < amounts.Length; i++) amounts[i] = 0;
            int nextDay = s.dayNum + 1;
            title.text = t.Format("ui.order.title", ("n", nextDay));
            int p = Mathf.Min(s.planIdx, cfg.day.targets.Length - 1);
            string text = t.Format("ui.order.info", ("plan", s.planIdx + 1), ("max", cfg.day.plans),
                ("text", DayCycle.Instance.PlanText(cfg.day.targets[p])), ("min", Mathf.RoundToInt(cfg.day.dayLengthSec / 60f)));
            if (s.infected) text += t.Format("ui.order.infoInfected", ("n", s.infectedUntilDay));
            info.text = text;
            confirmLabel.text = t.Get("ui.order.confirm");
            for (int i = 0; i < rows.Length; i++)
            {
                names[i] = t.Get("ui.order.item" + ItemKeys[i]);
                rows[i].name.text = names[i];
                rows[i].desc.text = t.Get("ui.order.desc" + ItemKeys[i]);
                rows[i].price.text = Money(DayCycle.Instance.Price(i));
            }
            Refresh();
        }

        void Change(int i, int delta)
        {
            amounts[i] = Mathf.Max(0, amounts[i] + delta);
            if (DayCycle.Instance.OrderSum(amounts) > GameManager.Instance.State.Money) amounts[i] = Mathf.Max(0, amounts[i] - delta);
            Refresh();
        }

        void Refresh()
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var s = gm.State;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].stock.text = t.Format("ui.order.stock", ("n", Stock(s, i)));
                rows[i].count.text = amounts[i].ToString();
                rows[i].minus.interactable = amounts[i] > 0;
            }
            float total = DayCycle.Instance.OrderSum(amounts);
            sum.text = t.Format("ui.order.sum", ("sum", Money(total)), ("cash", Money(s.Money)));
            confirm.interactable = total <= s.Money;
        }

        static int Stock(GameState s, int i) => i < 3 ? s.substances[i] : i == 3 ? s.antidote : i == 4 ? s.vodka : s.caviar;

        void OnConfirm()
        {
            if (DayCycle.Instance.ConfirmOrder(amounts, names)) root.SetActive(false);
        }

        string Money(float v) => GameManager.Instance.Texts.Format("ui.common.money", ("n", Mathf.RoundToInt(v).ToString("N0", German)));
    }
}
