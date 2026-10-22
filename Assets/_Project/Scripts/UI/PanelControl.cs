using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Pult (GDD 6.1): Serum-Regal mit Auswahl und Infobox, Hebel GAS / KLON Kuh / KLON Huhn / FÖRDERBAND,
    /// Statuslampen, Aktionen (Gegenmittel, anleinen, reinigen, NOT-STOPP). Knöpfe sind nur aktiv, wenn
    /// die Aktion möglich ist (wie Prototyp refreshPult). Jeder Hebel-Knopf hat links einen kleinen Hebel, der beim
    /// Ziehen umklappt; in der Einführung wackelt der gesuchte Hebel und sein Knauf leuchtet gelb.
    /// </summary>
    public class PanelControl : PanelBase
    {
        [System.Serializable]
        public class Lamp
        {
            public Image light;
            public TMP_Text label;
        }

        [SerializeField] Chamber chamber;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text animalStatus;
        [SerializeField] TMP_Text closeHint;
        [SerializeField] TMP_Text rackHeader, leverHeader, statusHeader, actionHeader;
        [SerializeField] RectTransform rackList;
        [SerializeField] GameObject vialTemplate;
        [SerializeField] TMP_Text rackEmpty;
        [SerializeField] SerumInfoBox info;
        [SerializeField] Button gas, cloneCow, cloneHen, belt, antidote, leash, clean, stop, closeButton;
        [SerializeField] Lamp[] lamps = new Lamp[4];
        [SerializeField] Color lampOk = new Color(0.27f, 0.82f, 0.31f);
        [SerializeField] Color lampWarn = new Color(1f, 0.69f, 0.13f);
        [SerializeField] Color lampBad = new Color(1f, 0.23f, 0.18f);
        [SerializeField] Color lampOff = new Color(0.27f, 0.27f, 0.27f);
        [SerializeField] Color vialSelected = new Color(0.2f, 0.33f, 0.16f);
        [SerializeField] Color vialNormal = new Color(0.18f, 0.15f, 0.11f);
        [Header("Hebel-Symbole (GAS, KLON Kuh, KLON Huhn, FÖRDERBAND)")]
        [SerializeField] RectTransform[] leverSticks = new RectTransform[4];
        [SerializeField] Image[] leverKnobs = new Image[4];
        [SerializeField] Color knobNormal = new Color(0.85f, 0.15f, 0.1f);
        [SerializeField] Color knobHint = new Color(1f, 0.85f, 0.2f);
        [SerializeField] float pullDeg = 50f;
        [SerializeField] float hintWobbleDeg = 14f;

        /// <summary>Hebel-Nummer am 3D-Pult (EventBus.LeverPulled) je Symbol.</summary>
        static readonly int[] LeverOf = { 0, 1, 1, 2 };
        /// <summary>Einführungsschritt, in dem das Symbol hervorgehoben wird.</summary>
        static readonly string[] HintStep = { "spray", "clone", "", "process" };
        readonly float[] pullTimers = new float[3];

        readonly List<GameObject> vials = new List<GameObject>();
        string rackKey;

        public void SetChamber(Chamber c) => chamber = c;

        protected override void Awake()
        {
            base.Awake();
            gas.onClick.AddListener(() => Act(chamber.Spray(GM.State.selectedSerum)));
            cloneCow.onClick.AddListener(() => Act(chamber.Clone(AnimalKind.Cow)));
            cloneHen.onClick.AddListener(() => Act(chamber.Clone(AnimalKind.Hen)));
            belt.onClick.AddListener(() => Act(chamber.SendToProcessing()));
            antidote.onClick.AddListener(() => Act(chamber.AntidoteToAnimal()));
            leash.onClick.AddListener(() => Act(chamber.Leash()));
            clean.onClick.AddListener(() => Act(chamber.Clean()));
            stop.onClick.AddListener(() => Act(chamber.NotStop()));
            closeButton.onClick.AddListener(Close);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus.LeverPulled += OnLever;
        }

        protected override void OnDisable()
        {
            EventBus.LeverPulled -= OnLever;
            base.OnDisable();
        }

        void OnLever(int i)
        {
            if (i >= 0 && i < pullTimers.Length && GM != null) pullTimers[i] = GM.Config.pult.leverPullSec;
        }

        /// <summary>Hebel-Symbole: beim Ziehen umklappen, in der Einführung wackeln.</summary>
        void LateUpdate()
        {
            if (!IsOpen || GM == null) return;
            var cfg = GM.Config;
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < pullTimers.Length; i++) pullTimers[i] = Mathf.Max(0f, pullTimers[i] - dt);
            var step = TutorialLogic.Current(GM.State, cfg);
            for (int i = 0; i < leverSticks.Length; i++)
            {
                if (leverSticks[i] == null) continue;
                float t = pullTimers[LeverOf[i]];
                bool hint = step != null && step.id == HintStep[i];
                float a = t > 0f ? -pullDeg * Mathf.Sin(t / cfg.pult.leverPullSec * Mathf.PI)
                        : hint ? hintWobbleDeg * Mathf.Sin(Time.unscaledTime * 6f) : 0f;
                leverSticks[i].localRotation = Quaternion.Euler(0f, 0f, a);
                leverKnobs[i].color = hint ? Color.Lerp(knobNormal, knobHint, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f)) : knobNormal;
            }
        }

        void Act(Feedback fb)
        {
            fb.Report(GM);
            Refresh(true);
        }

        protected override void OnOpened()
        {
            var t = GM.Texts;
            title.text = t.Get("ui.pult.title");
            closeHint.text = t.Get("ui.common.close");
            rackHeader.text = t.Get("ui.pult.rack");
            leverHeader.text = t.Get("ui.pult.levers");
            statusHeader.text = t.Get("ui.pult.status");
            actionHeader.text = t.Get("ui.pult.actions");
            Label(gas, t.Get("ui.pult.leverGas"));
            Label(leash, t.Get("ui.pult.leash"));
            Label(stop, t.Get("ui.pult.notStop"));
            string[] lampKeys = { "statusGas", "statusTemp", "statusBelt", "statusVitals" };
            for (int i = 0; i < lamps.Length; i++) lamps[i].label.text = t.Get("ui.pult." + lampKeys[i]);
            rackKey = null;
        }

        public override void Refresh(bool force)
        {
            var s = GM.State;
            var cfg = GM.Config;
            var t = GM.Texts;
            var c = chamber;
            if (s.selectedSerum >= s.serums.Count) s.selectedSerum = Mathf.Max(0, s.serums.Count - 1);

            RefreshRack(force, s, cfg, t);
            var sel = s.selectedSerum < s.serums.Count ? s.serums[s.selectedSerum] : null;
            if (sel == null) info.Clear();
            else if (sel.IsUltimate && SerumTexts.IsKnown(sel, s)) info.ShowUltimate(cfg.UltimateById(sel.ultimateId));
            else if (SerumTexts.IsKnown(sel, s)) info.ShowCombo(sel.dose, cfg, t);
            else info.ShowUnknown(t);

            animalStatus.text = AnimalStatus(c, t);

            var st = c.State;
            bool alive = c.HasLivingAnimal;
            gas.interactable = st == AnimalState.Ok && sel != null && !s.chamberCovered && !c.IsUltimate;
            bool canClone = st == AnimalState.Empty && !s.beltBroken && !s.contaminated && s.blackoutTimer <= 0f;
            // Klonkosten stehen auf dem Hebel; ohne Geld bleibt er gesperrt
            float costCow = OrderLogic.CloneCost(cfg, AnimalKind.Cow), costHen = OrderLogic.CloneCost(cfg, AnimalKind.Hen);
            Label(cloneCow, t.Format("ui.pult.leverCloneCow", ("cost", Mathf.RoundToInt(costCow))));
            Label(cloneHen, t.Format("ui.pult.leverCloneHen", ("cost", Mathf.RoundToInt(costHen))));
            cloneCow.interactable = canClone && s.Money >= costCow;
            cloneHen.interactable = canClone && s.Money >= costHen;
            belt.interactable = alive && !s.beltBroken;
            Label(belt, t.Get(st == AnimalState.Unstable ? "ui.pult.leverBeltBrave" : "ui.pult.leverBelt"));
            antidote.interactable = alive && s.antidote > 0;
            Label(antidote, t.Format("ui.pult.antidote", ("n", s.antidote)));
            leash.interactable = alive && c.FloatHeight > cfg.pult.leashMinFloat;
            clean.interactable = s.contaminated && s.cleanTimer <= 0f;
            Label(clean, s.cleanTimer > 0f ? t.Format("ui.pult.cleaning", ("sec", Mathf.CeilToInt(s.cleanTimer))) : t.Get("ui.pult.clean"));
            stop.interactable = alive;

            SetLamp(0, c.GasLamp);
            SetLamp(1, c.TempLamp);
            SetLamp(2, c.BeltLamp);
            SetLamp(3, c.VitalLamp);
        }

        void RefreshRack(bool force, GameState s, GameConfig cfg, TextDatabase t)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < s.serums.Count; i++) sb.Append(SerumTexts.Name(s.serums[i], cfg, t, s)).Append('|');
            sb.Append('#').Append(s.selectedSerum);
            string key = sb.ToString();
            if (!force && key == rackKey) return;
            rackKey = key;

            rackEmpty.gameObject.SetActive(s.serums.Count == 0);
            rackEmpty.text = t.Get("ui.pult.rackEmpty");
            while (vials.Count < s.serums.Count)
            {
                var v = Instantiate(vialTemplate, rackList);
                int idx = vials.Count;
                v.GetComponent<Button>().onClick.AddListener(() => { GM.State.selectedSerum = idx; Refresh(true); });
                vials.Add(v);
            }
            for (int i = 0; i < vials.Count; i++)
            {
                bool on = i < s.serums.Count;
                vials[i].SetActive(on);
                if (!on) continue;
                var serum = s.serums[i];
                var texts = vials[i].GetComponentsInChildren<TMP_Text>(true);
                texts[0].text = SerumTexts.Name(serum, cfg, t, s);
                texts[1].text = t.Format("ui.pult.vialDoses", ("a", serum.dose.A), ("b", serum.dose.B), ("c", serum.dose.C))
                                + (serum.stage == SerumStage.Raw ? t.Get("ui.pult.vialRaw") : "");
                var images = vials[i].GetComponentsInChildren<Image>(true);
                images[0].color = i == s.selectedSerum ? vialSelected : vialNormal;
                images[1].color = SerumTexts.Color(serum, cfg);
            }
        }

        static string AnimalStatus(Chamber c, TextDatabase t)
        {
            switch (c.State)
            {
                case AnimalState.Empty: return t.Get("ui.pult.animalEmpty");
                case AnimalState.Unstable: return Status(c, t, t.Format("ui.pult.stateUnstable", ("sec", Mathf.CeilToInt(c.UnstableTimer))));
                case AnimalState.Ok: return Status(c, t, t.Format("ui.pult.stateOk", ("n", Formula.RoundHalfUp(c.Stability))));
                case AnimalState.Enter: return Status(c, t, t.Get("ui.pult.stateEnter"));
                case AnimalState.Exit: return Status(c, t, t.Get("ui.pult.stateExit"));
                default: return Status(c, t, t.Get("ui.pult.stateOther"));
            }
        }

        static string Status(Chamber c, TextDatabase t, string state) =>
            t.Format("ui.pult.animalStatus",
                ("kind", t.Get(c.Kind == AnimalKind.Hen ? "ui.common.kindHen" : "ui.common.kindCow")),
                ("id", t.Format("ui.common.animalId", ("n", c.ExperimentNo.ToString("00")))),
                ("state", state));

        void SetLamp(int i, LampState s) =>
            lamps[i].light.color = s == LampState.Ok ? lampOk : s == LampState.Warn ? lampWarn : s == LampState.Bad ? lampBad : lampOff;

        static void Label(Button b, string text) => b.GetComponentInChildren<TMP_Text>(true).text = text;
    }
}
