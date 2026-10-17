using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Tier-Meter links unten (GDD 7.6): Menge, Qualität, Stabilität in Segmenten, darüber Tier-Kennung
    /// (Prototyp setMeter/updateHud).
    /// </summary>
    public class AnimalMeter : MonoBehaviour
    {
        [SerializeField] Chamber chamber;
        [SerializeField] TMP_Text info;
        [SerializeField] TMP_Text[] labels = new TMP_Text[3];
        [SerializeField] Image[] amount = new Image[0];
        [SerializeField] Image[] quality = new Image[0];
        [SerializeField] Image[] stability = new Image[0];
        [SerializeField] Color off = new Color(0.16f, 0.15f, 0.13f);
        [SerializeField] Color amountColor = new Color(0.42f, 0.82f, 1f);
        [SerializeField] Color qualityColor = new Color(1f, 0.8f, 0.16f);
        [SerializeField] Color stabGood = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color stabMid = new Color(1f, 0.69f, 0.13f);
        [SerializeField] Color stabBad = new Color(1f, 0.23f, 0.18f);

        readonly int[] shown = { -1, -1, -1 };
        Color shownStabColor;
        string shownInfo;

        public void SetChamber(Chamber c) => chamber = c;

        void Start()
        {
            var t = GameManager.Instance.Texts;
            labels[0].text = t.Get("ui.hud.meterAmount");
            labels[1].text = t.Get("ui.hud.meterQuality");
            labels[2].text = t.Get("ui.hud.meterStability");
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null || chamber == null) return;
            var cfg = gm.Config;
            var a = cfg.animal;
            var st = chamber.Stats;
            bool alive = chamber.HasLivingAnimal;

            float rateMax = st.kind == AnimalKind.Cow ? a.meterRateMaxCow : a.meterRateMaxHen;
            SetMeter(0, amount, alive ? st.currentRate / rateMax : 0f, amountColor);
            SetMeter(1, quality, alive ? st.Quality(cfg) / cfg.formulas.statMax : 0f, qualityColor);
            float s = alive ? st.stability : 0f;
            var sc = s > a.meterStabGood ? stabGood : s > a.meterStabMid ? stabMid : stabBad;
            if (sc != shownStabColor) { shownStabColor = sc; shown[2] = -1; }
            SetMeter(2, stability, s / cfg.formulas.statMax, sc);

            var t = gm.Texts;
            string text;
            if (alive)
            {
                text = t.Format("ui.hud.meterAnimal",
                    ("kind", t.Get(st.kind == AnimalKind.Cow ? "ui.common.kindCow" : "ui.common.kindHen").ToUpperInvariant()),
                    ("id", t.Format("ui.common.animalId", ("n", chamber.ExperimentNo.ToString("00")))));
                if (st.IsUltimate) text += t.Format("ui.hud.meterUlt", ("name", st.ultimate.displayName.ToUpperInvariant()));
                if (chamber.State == AnimalState.Unstable) text += t.Get("ui.hud.meterUnstable");
            }
            else text = t.Get(chamber.State == AnimalState.Empty ? "ui.hud.chamberEmpty" : "ui.hud.meterChamber");
            if (text != shownInfo)
            {
                shownInfo = text;
                info.text = text;
            }
        }

        void SetMeter(int index, Image[] segs, float frac, Color color)
        {
            int n = Mathf.RoundToInt(Mathf.Clamp01(frac) * segs.Length);
            if (n == shown[index]) return;
            shown[index] = n;
            for (int i = 0; i < segs.Length; i++) segs[i].color = i < n ? color : off;
        }
    }
}
