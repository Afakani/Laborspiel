using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Bestellterminal im Lager (GDD 6.9, auf Wunsch umgebaut): während der Schicht jederzeit; sieben Artikel mit −/+,
    /// Lagerbestand, Preis, Summe und Kasse. Bestellt wird für den nächsten Morgen (sofort bezahlt), ohne Bestellung
    /// kommt die Standardbestellung. Zeigt, was für morgen schon bestellt ist. „Wie gestern“ übernimmt die zuletzt
    /// gelieferte eigene Bestellung (so weit die Kasse reicht), „Empfohlen“ die Bestellung nach dem Plan von morgen
    /// (orderHelp, in der Einführung Pflicht).
    /// </summary>
    public class OrderTerminalUI : PanelBase
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");
        /// <summary>Text-Schlüssel der Artikel in Bestellreihenfolge (OrderLogic.Items).</summary>
        public static readonly string[] ItemKeys = { "A", "B", "C", "Antidote", "Vodka", "Caviar", "Cloth" };

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

        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text info;
        [SerializeField] TMP_Text pending;
        [SerializeField] TMP_Text sum;
        [SerializeField] Button confirm;
        [SerializeField] TMP_Text confirmLabel;
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Text closeLabel;
        [SerializeField] Button repeat;
        [SerializeField] TMP_Text repeatLabel;
        [SerializeField] Button recommend;
        [SerializeField] TMP_Text recommendLabel;
        [SerializeField] Row[] rows = new Row[OrderLogic.Items];

        readonly int[] amounts = new int[OrderLogic.Items];

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < rows.Length; i++)
            {
                int idx = i;
                rows[i].minus.onClick.AddListener(() => Change(idx, -1));
                rows[i].plus.onClick.AddListener(() => Change(idx, 1));
            }
            confirm.onClick.AddListener(OnConfirm);
            closeButton.onClick.AddListener(Close);
            repeat.onClick.AddListener(Repeat);
            recommend.onClick.AddListener(Recommend);
        }

        protected override void OnOpened()
        {
            var gm = GM;
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
            confirmLabel.text = t.Get("ui.order.confirm");
            closeLabel.text = t.Get("ui.order.close");
            repeatLabel.text = t.Get(s.lastOrder != null ? "ui.order.repeat" : "ui.order.repeatNone");
            recommendLabel.text = t.Get("ui.order.recommend");
            var need = TutorialLogic.RequiredOrder(s, cfg);
            if (need != null) text += t.Format("ui.order.infoTutorial", ("list", OrderLogic.List(need, DayCycle.Instance.ItemNames())));
            info.text = text;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].name.text = t.Get("ui.order.item" + ItemKeys[i]);
                rows[i].desc.text = t.Get("ui.order.desc" + ItemKeys[i]);
                rows[i].price.text = Money(OrderLogic.Price(cfg, i));
            }
        }

        void Change(int i, int delta)
        {
            amounts[i] = Mathf.Max(0, amounts[i] + delta);
            if (OrderLogic.Sum(GM.Config, amounts) > GM.State.Money) amounts[i] = Mathf.Max(0, amounts[i] - delta);
            Refresh(false);
        }

        public override void Refresh(bool force)
        {
            var gm = GM;
            var t = gm.Texts;
            var s = gm.State;
            for (int i = 0; i < rows.Length; i++)
            {
                rows[i].stock.text = t.Format("ui.order.stock", ("n", OrderLogic.Stock(s, i)));
                rows[i].count.text = amounts[i].ToString();
                rows[i].minus.interactable = amounts[i] > 0;
            }
            float total = OrderLogic.Sum(gm.Config, amounts);
            sum.text = t.Format("ui.order.sum", ("sum", Money(total)), ("cash", Money(s.Money)));
            confirm.interactable = total > 0f && total <= s.Money;
            repeat.interactable = s.lastOrder != null;
            pending.text = s.pendingOrder != null
                ? t.Format("ui.order.pending", ("list", OrderLogic.List(s.pendingOrder, DayCycle.Instance.ItemNames())))
                : t.Get("ui.order.pendingNone");
        }

        /// <summary>Letzte eigene Bestellung übernehmen, von hinten gekürzt, bis sie in die Kasse passt.</summary>
        void Repeat()
        {
            var s = GM.State;
            if (s.lastOrder == null) return;
            for (int i = 0; i < amounts.Length; i++) amounts[i] = i < s.lastOrder.Length ? s.lastOrder[i] : 0;
            while (OrderLogic.Sum(GM.Config, amounts) > s.Money)
            {
                int last = System.Array.FindLastIndex(amounts, a => a > 0);
                if (last < 0) break;
                amounts[last]--;
            }
            Refresh(false);
        }

        /// <summary>Empfohlene Bestellung für morgen übernehmen (gekürzt auf die Kasse).</summary>
        void Recommend()
        {
            var r = OrderLogic.Recommended(GM.State, GM.Config, true);
            if (r == null) return;
            for (int i = 0; i < amounts.Length; i++) amounts[i] = r[i];
            Refresh(false);
        }

        void OnConfirm()
        {
            if (!DayCycle.Instance.PlaceOrder(amounts)) return;
            for (int i = 0; i < amounts.Length; i++) amounts[i] = 0;
            Refresh(true);
        }

        string Money(float v) => GM.Texts.Format("ui.common.money", ("n", Mathf.RoundToInt(v).ToString("N0", German)));
    }
}
