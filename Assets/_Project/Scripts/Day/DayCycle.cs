using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Spielablauf (GDD 7, Prototyp startDay/endDay/openOrder/confirmOrder/gameOver): Intro → Bestellterminal →
    /// Schicht (180 s) → Schichtende → nächster Tag bzw. Spielende. Steuert Quote, Sonderwünsche, Sonderauftrag,
    /// Einführung, Leerlauf-Wut und Direktor. Overlays halten die Spielzeit an und geben die Maus frei.
    /// Die Regeln stehen in DayLogic.
    /// </summary>
    public class DayCycle : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        public static DayCycle Instance { get; private set; }

        Director director;
        Chamber chamber;
        InfectionSystem infection;
        FirstPersonController player;
        InteractionSystem interaction;
        readonly List<DayEvent> events = new List<DayEvent>();
        float idleTimer, dayOverTimer, contractTimer, newsTime;
        bool finalAfterDay, newsAfterDay;

        public DayMode Mode { get; private set; }
        /// <summary>Laufende Sendung im Staats-TV (gültig in Mode News).</summary>
        public NewsCopy CurrentNews { get; private set; }
        /// <summary>Sekunden seit Beginn der Sendung.</summary>
        public float NewsTime => newsTime;
        /// <summary>Darf die Sendung schon übersprungen werden?</summary>
        public bool CanSkipNews => Mode == DayMode.News && newsTime >= GM.Config.news.skipAfterSec;
        /// <summary>Grund des Spielendes (gültig in Mode GameOver).</summary>
        public GameOverKind EndKind { get; private set; }

        GameManager GM => GameManager.Instance;

        void Awake()
        {
            Instance = this;
            director = GetComponent<Director>();
            chamber = FindAnyObjectByType<Chamber>();
            infection = FindAnyObjectByType<InfectionSystem>();
            player = FindAnyObjectByType<FirstPersonController>();
            interaction = FindAnyObjectByType<InteractionSystem>();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable() => EventBus.Overfulfilled += OnOverfulfilled;
        void OnDisable() => EventBus.Overfulfilled -= OnOverfulfilled;

        void Start() => ShowIntro();

        // ---------- Abschnitte ----------

        void SetMode(DayMode m)
        {
            Mode = m;
            bool overlay = m != DayMode.Play && m != DayMode.DayOver;
            if (overlay) EventBus.RaisePanelCloseRequested();
            GM.Clock.SetHeld(overlay);
            GM.PanelOpen = overlay;
            director.Active = m == DayMode.Play;
        }

        public void ShowIntro() => SetMode(DayMode.Intro);

        /// <summary>Intro bestätigt → erstes Bestellterminal.</summary>
        public void StartFromIntro() => OpenOrder();

        public void OpenOrder() => SetMode(DayMode.Order);

        /// <summary>Preise der sechs Artikel (A, B, C, Gegenmittel, Wodka, Kaviar).</summary>
        public float Price(int i)
        {
            var p = GM.Config.prices;
            return i switch { 0 => p.A, 1 => p.B, 2 => p.C, 3 => p.antidote, 4 => p.vodka, _ => p.caviar };
        }

        public float OrderSum(int[] amounts)
        {
            float sum = 0f;
            for (int i = 0; i < amounts.Length; i++) sum += amounts[i] * Price(i);
            return sum;
        }

        /// <summary>Bestellung bezahlen, liefern und die Schicht beginnen. false = nicht genug Geld.</summary>
        public bool ConfirmOrder(int[] amounts, string[] itemNames)
        {
            var s = GM.State;
            float sum = OrderSum(amounts);
            if (!s.TrySpend(sum)) return false;
            var parts = new List<string>();
            for (int i = 0; i < amounts.Length; i++)
            {
                if (amounts[i] <= 0) continue;
                if (i < 3) s.substances[i] += amounts[i];
                else if (i == 3) s.antidote += amounts[i];
                else if (i == 4) s.vodka += amounts[i];
                else s.caviar += amounts[i];
                parts.Add($"{amounts[i]}× {itemNames[i]}");
            }
            s.NotifyInventoryChanged();
            if (parts.Count > 0) GM.Toast("ui.toasts.delivery", ToastKind.Good, ("list", string.Join(", ", parts)));
            StartDay();
            return true;
        }

        void StartDay()
        {
            var gm = GM;
            var s = gm.State;
            DayLogic.StartDay(s, gm.Config, () => Random.value);
            InspectionLogic.OnDayStart(s, gm.Config);
            idleTimer = 0f;
            contractTimer = gm.Config.ui.contractAnnounceDelaySec;
            director.ResetTimer(s.dayNum == 1);
            SetMode(DayMode.Play);

            string plan = PlanText(s.quota.target);
            if (s.dayNum == 1 && s.tutorialStep == 0) director.Say(gm.Texts.Get("director.tutorial[0].say"));
            else director.SayTier("director.start", ("plan", plan));
            gm.Toast("ui.toasts.dayStart", ToastKind.Warn, ("day", s.dayNum), ("plan", s.planIdx + 1), ("max", gm.Config.day.plans), ("text", plan));
            if (s.infected)
                gm.Toast(s.infectedUntilDay <= s.dayNum ? "ui.toasts.infectedLastDay" : "ui.toasts.infectedDeadline", ToastKind.Bad, ("n", s.infectedUntilDay));
        }

        void EndDay()
        {
            var gm = GM;
            var s = gm.State;
            // Tagesrekorde und Lob für den Sonderauftrag (Prototyp endDay)
            var q = s.quota;
            if (q != null)
            {
                Records.Report(RecordLogic.DayMilk, q.prod.m);
                Records.Report(RecordLogic.DayEggs, q.prod.e);
                Records.Report(RecordLogic.DayMeat, q.prod.f);
            }
            if (s.contract != null && s.contract.got > 0f)
                s.newsFacts.Add(gm.Texts.Format("newsFacts.contract", ("who", s.contract.data.who), ("what", s.contract.data.what)));
            bool collapse = infection != null && infection.CheckCollapse();
            var r = DayLogic.EndDay(s, gm.Config, collapse);
            s.lastPlanLine = r.met
                ? gm.Texts.Format("ui.news.planMetLine", ("n", s.dayNum), ("plan", s.planIdx), ("max", gm.Config.day.plans))
                : gm.Texts.Format("ui.news.planFailedLine", ("n", s.dayNum));
            if (collapse) director.SayAny("director.collapse");
            if (!r.met)
            {
                if (!collapse) director.SayTier("director.fail");
                gm.Toast("ui.toasts.planFailed", ToastKind.Bad);
            }
            else
            {
                foreach (var (key, name) in DayLogic.Unlocks(s.planIdx, gm.Config))
                    gm.Toast(key, ToastKind.Good, ("name", name));
            }
            if (r.fired)
            {
                GameOver(GameOverKind.Fired);
                return;
            }
            finalAfterDay = r.final;
            dayOverTimer = gm.Config.ui.dayEndDelaySec;
            SetMode(DayMode.DayOver);
        }

        /// <summary>
        /// Staats-TV (GDD 11): nach jedem Tag automatisch (afterDay), sonst als Sondersendung oder über den Knopf.
        /// Spielzeit steht; danach Bestellterminal, Spielende oder zurück in die Schicht.
        /// </summary>
        public void ShowNews(bool afterDay)
        {
            var gm = GM;
            CurrentNews = NewsLogic.Compose(gm.State, gm.Config, gm.Texts, () => Random.value);
            newsAfterDay = afterDay;
            newsTime = 0f;
            SetMode(DayMode.News);
            if (NewsStudio.Instance != null) NewsStudio.Instance.Play(CurrentNews);
        }

        /// <summary>Sendung überspringen (Knopf „Weiter“ oder Leertaste), sobald erlaubt.</summary>
        public void SkipNews()
        {
            if (CanSkipNews) EndNews();
        }

        void EndNews()
        {
            if (NewsStudio.Instance != null) NewsStudio.Instance.Stop();
            CurrentNews = null;
            if (!newsAfterDay) SetMode(DayMode.Play);
            else if (finalAfterDay) GameOver(GameOverKind.Final);
            else OpenOrder();
        }

        public void GameOver(GameOverKind kind)
        {
            EndKind = kind;
            SetMode(DayMode.GameOver);
            EventBus.RaiseGameOver(kind);
        }

        /// <summary>Endbildschirm: neues Spiel mit Intro.</summary>
        public void NewGame()
        {
            GM.NewGame();
            if (chamber != null) chamber.Clear();
            ShowIntro();
        }

        // ---------- Schicht ----------

        void Update()
        {
            var gm = GM;
            if (gm == null) return;
            if (Mode == DayMode.News)
            {
                newsTime += Time.unscaledDeltaTime;
                if (InputReader.InteractPressed) SkipNews();
                if (Mode == DayMode.News && newsTime >= gm.Config.news.durationSec) EndNews();
                return;
            }
            if (Mode == DayMode.DayOver)
            {
                dayOverTimer -= Time.unscaledDeltaTime;
                if (dayOverTimer > 0f) return;
                ShowNews(true);
                return;
            }
            if (Mode != DayMode.Play) return;

            var s = gm.State;
            var cfg = gm.Config;
            float dt = gm.Clock.DeltaTime;
            if (dt <= 0f) return;

            events.Clear();
            DayLogic.Tick(s, cfg, dt, events);
            bool crisis = chamber != null && (chamber.InCrisis || chamber.State == AnimalState.Unstable);
            DayLogic.TickWish(s, cfg, dt, crisis, () => Random.value, events);
            if (chamber != null)
                DayLogic.TickTutorial(s, chamber.State != AnimalState.Empty, chamber.Stats.dose.Total > 0 || chamber.IsUltimate, events);

            bool busy = gm.PanelOpen || (player != null && player.IsMoving) || (interaction != null && interaction.IsHoldingActive);
            if (DayLogic.TickIdle(ref idleTimer, busy, s, cfg, dt)) director.SayTier("director.idle");

            if (s.contract != null && contractTimer > 0f)
            {
                contractTimer -= dt;
                if (contractTimer <= 0f)
                {
                    var c = s.contract.data;
                    gm.Toast("ui.toasts.contractAnnounce", ToastKind.Good, ("who", c.who), ("what", c.what), ("q", c.minQ));
                }
            }

            foreach (var e in events)
            {
                Handle(e);
                if (Mode != DayMode.Play) return;
            }
            if (s.anger >= cfg.anger.max) GameOver(GameOverKind.Fired);
        }

        void Handle(DayEvent e)
        {
            var gm = GM;
            var s = gm.State;
            switch (e)
            {
                case DayEvent.PlanMet:
                    director.SayTier("director.met");
                    var prize = s.quota.prize;
                    if (prize > 0f) gm.Toast("ui.toasts.planMetPrize", ToastKind.Good, ("money", Money(prize)));
                    else gm.Toast("ui.toasts.planMet", ToastKind.Good);
                    break;
                case DayEvent.TimeUp:
                    EndDay();
                    break;
                case DayEvent.WishNew:
                    var w = s.wish;
                    string text = WishText(w);
                    director.Say(gm.Texts.Format("director.wishes.announce", ("wish", text), ("sec", Mathf.RoundToInt(w.left))));
                    gm.Toast("ui.toasts.wishAnnounce", ToastKind.Warn, ("text", text));
                    break;
                case DayEvent.WishDone:
                    director.Say(gm.Texts.Get("director.wishes.done"));
                    gm.Toast("ui.toasts.wishDone", ToastKind.Good, ("money", Money(DayLogic.WishReward(s, gm.Config))));
                    break;
                case DayEvent.WishFailed:
                    director.Say(gm.Texts.Get("director.wishes.failed"));
                    gm.Toast("ui.toasts.wishFailed", ToastKind.Bad);
                    break;
                case DayEvent.TutorialStep:
                    director.Say(gm.Texts.Get($"director.tutorial[{s.tutorialStep}].say"));
                    break;
                case DayEvent.TutorialDone:
                    director.Say(gm.Texts.Get($"director.tutorial[{DayLogic.TutorialSteps}].say"));
                    gm.Toast("ui.toasts.tutorialDone", ToastKind.Good);
                    break;
            }
        }

        void OnOverfulfilled(char good)
        {
            var gm = GM;
            gm.Toast("ui.toasts.overfulfil", ToastKind.Good, ("good", GoodName(good)));
        }

        // ---------- Texte ----------

        public string PlanText(GoodsValues t) =>
            GM.Texts.Format("ui.common.planText", ("m", t.m), ("e", t.e), ("f", t.f));

        public string GoodName(char good) =>
            GM.Texts.Get(good == 'm' ? "ui.common.goodMilk" : good == 'e' ? "ui.common.goodEggs" : "ui.common.goodMeat");

        public string WishText(Wish w) =>
            GM.Texts.Format("director.wishes." + w.Id, ("n", Mathf.RoundToInt(w.need)));

        string Money(float v) => GM.Texts.Format("ui.common.money", ("n", Mathf.RoundToInt(v).ToString("N0", German)));
    }
}
