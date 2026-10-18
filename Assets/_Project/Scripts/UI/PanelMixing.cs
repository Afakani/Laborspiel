using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Mischstation (GDD 6.2): 3 Regler 0..sliderMax, begrenzt durch das Lager, Infobox (bekannt/unbekannt),
    /// MISCHEN mit Restzeit, Regalanzeige, Probenmikroskop für unbekannte Mischungen (grobe Tendenzen).
    /// </summary>
    public class PanelMixing : PanelBase
    {
        [System.Serializable]
        public class SliderRow
        {
            public TMP_Text label;
            public Slider slider;
            public TMP_Text value;
            public TMP_Text stock;
            public Image dot;
        }

        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text rackCount;
        [SerializeField] TMP_Text closeHint;
        [SerializeField] TMP_Text dosingHeader;
        [SerializeField] SliderRow[] rows = new SliderRow[3];
        [SerializeField] SerumInfoBox info;
        [SerializeField] Button mixButton;
        [SerializeField] TMP_Text mixLabel;
        [SerializeField] Button closeButton;
        [SerializeField] Button probeButton;
        [SerializeField] TMP_Text probeLabel;
        [SerializeField] TMP_Text probeText;

        bool updatingSliders;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < rows.Length; i++)
            {
                int idx = i;
                rows[i].slider.onValueChanged.AddListener(v => OnSlider(idx, v));
            }
            mixButton.onClick.AddListener(OnMix);
            probeButton.onClick.AddListener(OnProbe);
            closeButton.onClick.AddListener(Close);
        }

        protected override void OnOpened()
        {
            var t = GM.Texts;
            var cfg = GM.Config;
            title.text = t.Get("ui.mixing.title");
            closeHint.text = t.Get("ui.common.close");
            dosingHeader.text = t.Get("ui.mixing.dosing");
            for (int i = 0; i < rows.Length; i++)
            {
                var sub = cfg.substances[i];
                rows[i].label.text = t.Format("ui.mixing.row", ("name", sub.displayName.ToUpperInvariant()), ("code", sub.code));
                rows[i].slider.wholeNumbers = true;
                rows[i].slider.minValue = 0;
                rows[i].slider.maxValue = cfg.mixing.sliderMax;
                if (ColorUtility.TryParseHtmlString(sub.color, out var c)) rows[i].dot.color = c;
            }
        }

        void OnSlider(int i, float v)
        {
            if (updatingSliders) return;
            MixingLogic.SetDose(GM.State, GM.Config, i, Mathf.RoundToInt(v));
            Refresh(true);
        }

        void OnMix()
        {
            MixingSystem.StartMix();
            Refresh(true);
        }

        void OnProbe()
        {
            if (!StationLogic.TakeProbe(GM.State, GM.Config).Report(GM)) return;
            GM.Toast("ui.toasts.probeRunning", ToastKind.Info);
            EventBus.RaiseStation("probe");
            Refresh(true);
        }

        public override void Refresh(bool force)
        {
            var s = GM.State;
            var cfg = GM.Config;
            var t = GM.Texts;
            MixingLogic.ClampToStock(s, cfg);

            updatingSliders = true;
            for (int i = 0; i < rows.Length; i++)
            {
                int dose = s.mix[i];
                if (Mathf.RoundToInt(rows[i].slider.value) != dose) rows[i].slider.value = dose;
                rows[i].value.text = dose.ToString();
                rows[i].stock.text = t.Format("ui.mixing.stock", ("n", s.substances[i]));
            }
            updatingSliders = false;

            var d = s.mix;
            if (d.Total == 0) info.Clear(t.Get("ui.mixing.hintEmpty"));
            else if (s.discoveredRecipes.Contains(d.Key)) info.ShowCombo(d, cfg, t);
            else info.ShowUnknown(t);

            bool mixing = MixingLogic.IsMixing(s);
            mixButton.interactable = MixingLogic.CanMix(s, cfg);
            mixLabel.text = mixing ? t.Format("ui.mixing.mixing", ("sec", Mathf.CeilToInt(s.mixTimer))) : t.Get("ui.mixing.mix");

            // Mikroskop: nur für unbekannte Mischungen
            var m = cfg.machinesFixed.microscope;
            bool known = s.discoveredRecipes.Contains(d.Key);
            bool probed = s.probedRecipes.Contains(d.Key);
            probeLabel.text = t.Format("ui.mixing.probeButton", ("cost", Mathf.RoundToInt(m.cost)));
            probeButton.interactable = StationLogic.CanProbe(s) && s.Money >= m.cost;
            if (d.Total == 0 || known) probeText.text = "";
            else if (!probed) probeText.text = t.Format("ui.mixing.probeUnknown", ("cost", Mathf.RoundToInt(m.cost)));
            else if (s.probeLeft > 0f) probeText.text = t.Get("ui.mixing.probeRunning");
            else probeText.text = t.Format("ui.mixing.probeResult", ("text", ComboTexts.Probe(Formula.Probe(d, cfg), t)));

            string count = t.Format("ui.mixing.rackCount", ("n", s.serums.Count), ("max", cfg.mixing.rackSize));
            if (s.serums.Count > 0)
            {
                var names = new string[s.serums.Count];
                for (int i = 0; i < names.Length; i++) names[i] = SerumTexts.Name(s.serums[i], cfg, t, s);
                count += " · " + string.Join(", ", names);
            }
            rackCount.text = count;
        }
    }
}
