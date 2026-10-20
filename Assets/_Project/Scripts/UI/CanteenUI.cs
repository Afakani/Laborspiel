using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Speisekarte der Kantine: drei Gerichte (sideRooms.canteen.meals) mit Preis und Serumstärke bis Schichtende.
    /// Ein besseres Gericht ersetzt ein schlechteres; billigere als das gegessene sind gesperrt.
    /// </summary>
    public class CanteenUI : PanelBase
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text info;
        [SerializeField] TMP_Text current;
        [SerializeField] Button[] meals = new Button[0];
        [SerializeField] TMP_Text[] mealNames = new TMP_Text[0];
        [SerializeField] TMP_Text[] mealDescs = new TMP_Text[0];
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Text closeLabel;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < meals.Length; i++)
            {
                int idx = i;
                meals[i].onClick.AddListener(() => Eat(idx));
            }
            closeButton.onClick.AddListener(Close);
        }

        protected override void OnOpened()
        {
            var t = GM.Texts;
            var cfg = GM.Config.sideRooms.canteen;
            title.text = t.Get("ui.canteen.title");
            info.text = t.Get("ui.canteen.info");
            closeLabel.text = t.Get("ui.canteen.close");
            for (int i = 0; i < meals.Length; i++)
            {
                bool has = i < cfg.meals.Length;
                meals[i].gameObject.SetActive(has);
                if (!has) continue;
                var m = cfg.meals[i];
                mealNames[i].text = t.Get($"ui.canteen.meals[{i}]");
                mealDescs[i].text = t.Format("ui.canteen.mealDesc", ("money", Money(m.price)), ("x", X(m.boost)));
            }
        }

        public override void Refresh(bool force)
        {
            var gm = GM;
            var s = gm.State;
            var cfg = gm.Config.sideRooms.canteen;
            for (int i = 0; i < meals.Length && i < cfg.meals.Length; i++)
                meals[i].interactable = cfg.meals[i].boost > s.mealBoost && s.Money >= cfg.meals[i].price;
            current.text = s.mealBoost > 1f
                ? gm.Texts.Format("ui.canteen.current", ("x", X(s.mealBoost)))
                : gm.Texts.Get("ui.canteen.hungry");
        }

        void Eat(int i)
        {
            var gm = GM;
            if (!RoomLogic.Eat(gm.State, gm.Config, i).Report(gm)) return;
            gm.Toast("ui.toasts.meal", ToastKind.Good, ("name", gm.Texts.Get($"ui.canteen.meals[{i}]")), ("x", X(gm.State.mealBoost)));
            EventBus.RaiseStation("canteen");
            Close();
        }

        static string X(float v) => v.ToString("0.0", German);
        string Money(float v) => GM.Texts.Format("ui.common.money", ("n", Mathf.RoundToInt(v).ToString("N0", German)));
    }
}
