using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Autopilot für Testdurchläufe (nur Editor und Development Builds, wie das Debug-Menü): spielt über die echten
    /// Spielsysteme (Bestellung, Klonen, Mischen, Sprühen, Verarbeiten, Not-Stopp, Reinigen, Laken, Sendung überspringen,
    /// auf Wunsch Kantine, Schutzkittel und Kaviar am Schreibtisch) im Zeitraffer bis zum Zielplan. Schreibt Meldungen
    /// in eine Logdatei und je Schicht eine Kennzahlen-Zeile (Kasse, Einnahmen, Ausgaben, Planerfüllung, Wut,
    /// Explosionen, Kontrollen; Ausgaben netto) in eine CSV-Datei für den Balancing-Vergleich mehrerer Varianten.
    /// Entwicklerwerkzeug, daher Strategie-Werte als Felder und Logtexte im Code.
    /// </summary>
    public class DebugAutoPlay : MonoBehaviour
    {
        /// <summary>Strategie eines Durchlaufs.</summary>
        public class Options
        {
            public string name = "Basis";
            /// <summary>Gericht in der Kantine zu Schichtbeginn (−1 = nicht essen).</summary>
            public int meal = -1;
            /// <summary>Schutzkittel ab Tag 2 anziehen.</summary>
            public bool coat;
            /// <summary>Kaviar am Schreibtisch abgeben, sobald die Wut darüber liegt (≤ 0 = nie).</summary>
            public float caviarAtAnger;
            /// <summary>Starke Mischungen aus mehreren Chemikalien, Zentrifuge, Kryo (statt je 1 Dosis A, B, C).</summary>
            public bool smart = true;
            /// <summary>Ab Plan 3 goldene Seren bauen (Ω, Fleischkoloss).</summary>
            public bool ultimates = true;
            /// <summary>Mindest-Stabilität der gemischten Dosis.</summary>
            public float minStability = 50f;
            /// <summary>Schwächere Mischungen werden nicht gemacht.</summary>
            public float minFactor = 1.4f;
            /// <summary>Höchstdosis A bzw. B (viel A reißt das Band, viel B macht Säure und Schweben).</summary>
            public int maxA = 2, maxB = 2;
            /// <summary>Höchstzahl an Schichten (Abbruch bei Endlosschleifen).</summary>
            public int maxDays = 14;
            /// <summary>Feste Dosis A, B, C für jedes Tier (Kombinationstest; ersetzt smart), null = Strategie oben.</summary>
            public int[] dose;
            /// <summary>So viele Melkpumpen bauen, sobald Geld da ist.</summary>
            public int pumps;
            /// <summary>Tier nach so vielen Sekunden verarbeiten (≤ 0 = Standard; kurz = Kühe für Fleisch durchschleusen).</summary>
            public float processAfter;
        }

        [SerializeField] float timeScale = 10f;
        [SerializeField] int stopAtPlan = 1;
        [SerializeField] int[] order = { 8, 6, 8, 1, 0, 1, 3 };
        [SerializeField] float processAfterSec = 35f;
        [SerializeField] float orderBudget = 0.75f;
        [SerializeField] int clothTarget = 3;
        /// <summary>Reihenfolge beim Kürzen der Bestellung: erst Kaviar, Wodka, Gegenmittel, dann B, C, A (Fleisch), Laken zuletzt.</summary>
        static readonly int[] TrimOrder = { 5, 4, 3, 1, 2, 0, 6 };
        [SerializeField] string logPath = "Temp/autoplay_log.txt";
        [SerializeField] string csvPath = "Temp/balance_runs.csv";

        public static DebugAutoPlay Instance { get; private set; }
        public bool Running { get; private set; }
        public bool Finished { get; private set; }
        /// <summary>Zusammenfassung des letzten Durchlaufs (eine Zeile).</summary>
        public string Summary { get; private set; } = "";

        readonly StringBuilder log = new StringBuilder();
        readonly Dictionary<string, int> toastCount = new Dictionary<string, int>();
        Options opt = new Options();
        Chamber chamber;
        Cloth cloth;
        MixingStation mixing;
        DayMode lastMode = (DayMode)(-1);
        float animalAge;
        bool sprayed, ateToday, sprayedOmega;
        Centrifuge centrifuge;
        CryoChamber cryoStation;
        BeltMotor beltMotor;
        int errors;

        // Kennzahlen der laufenden Schicht und des Durchlaufs
        float dayMoneyStart, dayEarnedStart, dayMaxAnger;
        // letzter Stand der Schicht (nach Schichtende ist die Quote weg)
        GoodsValues snapProd = new GoodsValues(), snapTarget = new GoodsValues();
        bool snapMet;
        int snapPlan, snapDay;
        float snapMeal = 1f;
        int dayExplosions, dayInspections, runMet, runFailed, runExplosions, runInspections;
        float runSpentMeals;

        void Awake()
        {
            if (!Debug.isDebugBuild) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>Durchlauf starten (Ziel: planIdx erreicht stopAtPlan).</summary>
        public void Begin(float scale, int targetPlanIdx) => Begin(scale, targetPlanIdx, new Options());

        public void Begin(float scale, int targetPlanIdx, Options options)
        {
            timeScale = scale;
            stopAtPlan = targetPlanIdx;
            opt = options ?? new Options();
            chamber = FindAnyObjectByType<Chamber>();
            cloth = FindAnyObjectByType<Cloth>();
            mixing = FindAnyObjectByType<MixingStation>();
            log.Clear();
            toastCount.Clear();
            errors = runMet = runFailed = runExplosions = runInspections = 0;
            runSpentMeals = 0f;
            lastMode = (DayMode)(-1);
            Running = true;
            Finished = false;
            Unsubscribe();
            EventBus.Toast += OnToast;
            EventBus.ToastKey += OnToastKey;
            EventBus.Explosion += OnExplosion;
            EventBus.InspectionStarted += OnInspection;
            Application.logMessageReceived += OnLog;
            if (!System.IO.File.Exists(csvPath))
                System.IO.File.WriteAllText(csvPath, "Variante;Tag;Plan;erfuellt;KasseStart;KasseEnde;Einnahmen;Ausgaben;Milch;MilchSoll;Eier;EierSoll;Fleisch;FleischSoll;WutMax;Explosionen;Kontrollen;Essen\n");
            Write($"Start Autopilot „{opt.name}“, Zeitraffer {timeScale}, Ziel Plan-Index {stopAtPlan}");
        }

        void OnDestroy() => Unsubscribe();

        void Unsubscribe()
        {
            EventBus.Toast -= OnToast;
            EventBus.ToastKey -= OnToastKey;
            EventBus.Explosion -= OnExplosion;
            EventBus.InspectionStarted -= OnInspection;
            Application.logMessageReceived -= OnLog;
        }

        void OnToast(string text, ToastKind kind) => Write("[" + kind + "] " + text);

        void OnToastKey(string key)
        {
            toastCount.TryGetValue(key, out int n);
            toastCount[key] = n + 1;
        }

        void OnExplosion()
        {
            dayExplosions++;
            runExplosions++;
        }

        void OnInspection(bool boss)
        {
            dayInspections++;
            runInspections++;
        }

        void OnLog(string msg, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception) return;
            errors++;
            Write("FEHLER " + type + ": " + msg + "\n" + stack);
        }

        void Write(string line)
        {
            var gm = GameManager.Instance;
            string t = gm != null && gm.State != null ? $"T{gm.State.dayNum}/P{gm.State.planIdx + 1} {gm.State.playTime,7:0.0}s " : "";
            log.Append(t).AppendLine(line);
            System.IO.File.WriteAllText(logPath, log.ToString());
        }

        void Update()
        {
            if (!Running) return;
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null) return;
            var s = gm.State;
            gm.Clock.SetPaused(false);
            gm.Clock.SetTimeScale(timeScale);

            if (dc.Mode != lastMode)
            {
                if (lastMode == DayMode.Play) EndOfShift(gm, s);
                lastMode = dc.Mode;
                if (dc.Mode == DayMode.Play) StartOfShift(s);
                Write($"Modus {dc.Mode} · Kasse {s.Money:0} · Milch {s.quota?.prod.m:0}/{s.quota?.target.m:0} Eier {s.quota?.prod.e:0}/{s.quota?.target.e:0} Fleisch {s.quota?.prod.f:0}/{s.quota?.target.f:0} · Wut {s.anger:0}");
            }

            if (s.planIdx >= stopAtPlan || dc.Mode == DayMode.GameOver || s.dayNum > opt.maxDays)
            {
                Finish(gm, s, dc);
                return;
            }

            switch (dc.Mode)
            {
                case DayMode.Intro:
                    dc.StartFromIntro(false);
                    break;
                case DayMode.News:
                    if (dc.CanSkipNews) dc.SkipNews();
                    break;
                case DayMode.Play:
                    // der Autopilot „arbeitet“ ständig; keine Leerlauf-Wut (Prototyp-Testschritt setzt idleT = 0)
                    dc.DebugResetIdle();
                    dayMaxAnger = Mathf.Max(dayMaxAnger, s.anger);
                    Snapshot(s);
                    UseRooms(gm, s);
                    if (s.pendingOrder == null) PlaceOrder(dc, s);
                    PlayShift(gm, s);
                    break;
            }
        }

        void StartOfShift(GameState s)
        {
            ateToday = false;
            dayMoneyStart = s.Money;
            dayEarnedStart = s.earned;
            dayMaxAnger = s.anger;
            dayExplosions = dayInspections = 0;
            snapDay = 0;
        }

        void Snapshot(GameState s)
        {
            var q = s.quota;
            if (q == null) return;
            snapProd.m = q.prod.m; snapProd.e = q.prod.e; snapProd.f = q.prod.f;
            snapTarget.m = q.target.m; snapTarget.e = q.target.e; snapTarget.f = q.target.f;
            snapMet = q.met;
            snapPlan = s.planIdx + (q.met ? 0 : 1);
            snapDay = s.dayNum;
            snapMeal = s.mealBoost;
        }

        void EndOfShift(GameManager gm, GameState s)
        {
            if (snapDay == 0) return;
            if (snapMet) runMet++; else runFailed++;
            // Ausgaben netto: Kasse vorher + Einnahmen − Kasse nachher (Prämien mindern die Ausgaben)
            float daySpent = dayMoneyStart + (s.earned - dayEarnedStart) - s.Money;
            string line = string.Join(";", opt.name, snapDay, snapPlan, snapMet ? 1 : 0, dayMoneyStart.ToString("0"), s.Money.ToString("0"),
                (s.earned - dayEarnedStart).ToString("0"), daySpent.ToString("0"), snapProd.m.ToString("0"), snapTarget.m.ToString("0"), snapProd.e.ToString("0"),
                snapTarget.e.ToString("0"), snapProd.f.ToString("0"), snapTarget.f.ToString("0"), dayMaxAnger.ToString("0"), dayExplosions, dayInspections, snapMeal.ToString("0.0"));
            snapDay = 0;
            System.IO.File.AppendAllText(csvPath, line + "\n");
            Write("Schicht-Kennzahlen: " + line);
        }

        void Finish(GameManager gm, GameState s, DayCycle dc)
        {
            Running = false;
            Finished = true;
            gm.Clock.SetTimeScale(1f);
            var top = new List<KeyValuePair<string, int>>(toastCount);
            top.Sort((a, b) => b.Value.CompareTo(a.Value));
            var sb = new StringBuilder();
            for (int i = 0; i < top.Count && i < 8; i++) sb.Append(top[i].Key.Replace("ui.toasts.", "").Replace("ui.errors.", "!")).Append('×').Append(top[i].Value).Append(' ');
            Summary = $"{opt.name}: Plan {s.planIdx}/{gm.Config.day.plans} nach {s.dayNum} Tagen · Ende {dc.Mode}{(dc.Mode == DayMode.GameOver ? " " + dc.EndKind : "")} · Kasse {s.Money:0} · erfüllt {runMet}, verfehlt {runFailed} · Explosionen {runExplosions} · Kontrollen {runInspections} · Essen {runSpentMeals:0} SM · Fehler {errors} · {sb}";
            Write("Ende: " + Summary);
        }

        /// <summary>Nebenzimmer nutzen: Essen zu Schichtbeginn, Kittel ab Tag 2, Kaviar persönlich bei hoher Wut.</summary>
        void UseRooms(GameManager gm, GameState s)
        {
            var cfg = gm.Config;
            if (!ateToday && opt.meal >= 0 && opt.meal < cfg.sideRooms.canteen.meals.Length)
            {
                ateToday = true;
                float price = cfg.sideRooms.canteen.meals[opt.meal].price;
                // nur essen, wenn danach noch Geld für Klonen und Bestellung bleibt
                if (s.Money >= price + 300f && RoomLogic.Eat(s, cfg, opt.meal).IsOk)
                {
                    runSpentMeals += price;
                    Write("Kantine: Gericht " + opt.meal + " für " + price.ToString("0"));
                }
            }
            if (opt.coat && s.dayNum >= 2 && !s.coatOn && RoomLogic.PutOnCoat(s).IsOk) Write("Schutzkittel an");
            if (opt.caviarAtAnger > 0f && s.anger >= opt.caviarAtAnger && s.caviar > 0)
            {
                RoomLogic.DeliverCaviar(s, cfg, Random.value, out bool shouted);
                Write("Kaviar persönlich" + (shouted ? " – gebrüllt" : ""));
            }
        }

        void PlaceOrder(DayCycle dc, GameState s)
        {
            var cfg = GameManager.Instance.Config;
            var amounts = (int[])order.Clone();
            // feste Dosis: Chemikalien für rund neun Tiere pro Schicht
            if (opt.dose != null)
                for (int k = 0; k < 3; k++) amounts[k] = opt.dose[k] * 9;
            // für morgen bestellen, so viel die Kasse hergibt (Rest bleibt fürs Klonen)
            // Laken auf den Bedarf auffüllen (rund zwei Kontrollen pro Schicht)
            amounts[6] = Mathf.Max(amounts[6], clothTarget - s.cloths);
            while (OrderLogic.Sum(cfg, amounts) > s.Money * orderBudget)
            {
                int i = System.Array.FindIndex(TrimOrder, k => amounts[k] > 0);
                if (i < 0) break;
                amounts[TrimOrder[i]]--;
            }
            if (OrderLogic.Sum(cfg, amounts) <= 0f) return;
            Write("Bestellung für morgen " + string.Join(",", amounts) + " für " + OrderLogic.Sum(cfg, amounts).ToString("0"));
            dc.PlaceOrder(amounts);
        }

        void PlayShift(GameManager gm, GameState s)
        {
            if (chamber == null) return;
            float dt = gm.Clock.DeltaTime;
            if (s.contaminated && chamber.State == AnimalState.Empty) Report("Reinigen", chamber.Clean());
            // gerissenes Förderband am Bandmotor reparieren (sonst kein Klonen und keine Verarbeitung)
            if (s.beltBroken)
            {
                if (beltMotor == null) beltMotor = FindAnyObjectByType<BeltMotor>();
                if (beltMotor != null)
                {
                    beltMotor.Execute();
                    if (!s.beltBroken) Write("Förderband repariert");
                }
            }

            // Kontrolle: mutiertes Tier mit dem Laken verdecken, verdächtiges Regal abdecken; danach wieder aufdecken
            bool inspect = s.inspection != null;
            if (cloth != null && (inspect && chamber.HasMutatedAnimal) != s.chamberCovered && (s.chamberCovered || s.cloths > 0))
            {
                cloth.Execute();
                Write(s.chamberCovered ? "Laken drauf" : "Laken ab");
            }
            if (mixing != null && inspect && !s.rackHidden && InspectionLogic.RackSuspicious(s, gm.Config))
            {
                mixing.Execute();
                Write("Regal abgedeckt");
            }
            // nach der Kontrolle wieder aufdecken (die Station öffnet ohne Kontrolle das Misch-Panel, daher direkt)
            if (!inspect && s.rackHidden)
            {
                s.rackHidden = false;
                Write("Regal offen");
            }
            BuildPumps(gm, s);
            if (opt.dose != null) MixFixed(s);
            else if (opt.smart) PrepareSerums(gm, s);
            if (s.chamberCovered) return;

            if (chamber.State == AnimalState.Unstable)
            {
                Report("Not-Stopp", chamber.NotStop());
                return;
            }

            if (chamber.State == AnimalState.Empty)
            {
                animalAge = 0f;
                sprayed = false;
                sprayedOmega = false;
                var q = s.quota;
                bool needEggs = q != null && q.target.e > 0f && q.prod.e / q.target.e < q.prod.m / Mathf.Max(1f, q.target.m) && !((opt.smart || opt.dose != null) && MeatBehind(s));
                var fb = chamber.Clone(needEggs ? AnimalKind.Hen : AnimalKind.Cow);
                if (fb.IsOk) Write("Klonen " + (needEggs ? "Huhn" : "Kuh"));
                return;
            }

            if (!chamber.HasLivingAnimal) return;
            animalAge += dt;

            if (!sprayed)
            {
                if (opt.dose != null)
                {
                    // ohne Dosis (0-0-0) wird nicht gesprüht
                    if (opt.dose[0] + opt.dose[1] + opt.dose[2] == 0) sprayed = true;
                    else if (s.serums.Count > 0 && chamber.State == AnimalState.Ok) sprayed = Report("Sprühen " + s.serums[0].dose.Key, chamber.Spray(0));
                }
                else if (opt.smart)
                {
                    int i = BestSerum(gm, s);
                    if (i >= 0 && chamber.State == AnimalState.Ok)
                    {
                        var serum = s.serums[i];
                        string what = "Sprühen " + serum.dose.Key + (serum.IsUltimate ? " (" + serum.ultimateId + ")" : serum.IsConcentrated ? " ×" + serum.conc.ToString("0.00") : "");
                        sprayed = Report(what, chamber.Spray(i));
                        sprayedOmega = sprayed && serum.IsUltimate && serum.ultimateId != "koloss";
                    }
                }
                // einfache Strategie: je 1 Dosis A, B, C
                else if (s.serums.Count == 0 && !MixingLogic.IsMixing(s))
                {
                    s.mix = new Dose(s.substances[0] > 0 ? 1 : 0, s.substances[1] > 0 ? 1 : 0, s.substances[2] > 0 ? 1 : 0);
                    if (s.mix.Total > 0 && MixingSystem.StartMix()) Write("Mischen " + s.mixPending);
                }
                else if (s.serums.Count > 0 && chamber.State == AnimalState.Ok)
                {
                    sprayed = Report("Sprühen", chamber.Spray(0));
                }
            }

            // Nach einer Weile verarbeiten (Fleisch); Tiere mit Ω länger melken
            float after = opt.processAfter > 0f ? opt.processAfter : processAfterSec;
            float keep = sprayedOmega ? after * 2f : after;
            if (animalAge >= keep || s.quota != null && s.quota.left < after * 0.3f && animalAge > 5f)
                Report("Verarbeiten", chamber.SendToProcessing());
        }

        /// <summary>Feste Dosis: bis zu zwei Seren vorrätig halten.</summary>
        void MixFixed(GameState s)
        {
            var d = new Dose(opt.dose[0], opt.dose[1], opt.dose[2]);
            if (d.Total == 0 || MixingLogic.IsMixing(s) || s.serums.Count >= 2) return;
            if (s.substances[0] < d.A || s.substances[1] < d.B || s.substances[2] < d.C) return;
            s.mix = d;
            if (MixingSystem.StartMix()) Write("Mischen " + d.Key);
        }

        /// <summary>Melkpumpen auf das erste freie Feld bauen, wenn danach noch Geld für Klonen und Bestellung bleibt.</summary>
        void BuildPumps(GameManager gm, GameState s)
        {
            var cfg = gm.Config;
            if (BuildLogic.Count(s, BuildLogic.Pump) >= opt.pumps) return;
            var t = BuildLogic.Type(cfg, BuildLogic.Pump);
            if (t == null || !BuildLogic.Unlocked(s, t) || s.Money < BuildLogic.Price(s, cfg, t) + 300f) return;
            var g = cfg.buildMachines.grid;
            for (int gz = 0; gz < g.rows; gz++)
            for (int gx = 0; gx < g.cols; gx++)
                if (BuildLogic.Free(s, cfg, gx, gz))
                {
                    if (Report("Melkpumpe gebaut", BuildLogic.Place(s, cfg, t, gx, gz))) EventBus.RaiseStation("buildPlaced");
                    return;
                }
        }

        // ---------- Seren-Strategie (smart) ----------

        /// <summary>Was fehlt am meisten? true = Fleisch, sonst Milch/Eier.</summary>
        static bool MeatBehind(GameState s)
        {
            var q = s.quota;
            if (q == null) return false;
            float fm = q.prod.m / Mathf.Max(1f, q.target.m), fe = q.prod.e / Mathf.Max(1f, q.target.e), ff = q.prod.f / Mathf.Max(1f, q.target.f);
            return ff < Mathf.Min(fm, fe);
        }

        /// <summary>
        /// Stärkste Mischung aus dem Lagerbestand, die noch stabil genug ist (ohne die Rezepte der goldenen Seren).
        /// Ziel Fleisch: Fleischfaktor, sonst Ertragsfaktor; bei Gleichstand die kleinere Dosis.
        /// </summary>
        Dose BestDose(GameConfig cfg, GameState s, bool meat)
        {
            var best = new Dose(0, 0, 0);
            float bestScore = 0f;
            for (int a = 0; a <= Mathf.Min(opt.maxA, s.substances[0]); a++)
            for (int b = 0; b <= Mathf.Min(opt.maxB, s.substances[1]); b++)
            for (int c = 0; c <= Mathf.Min(4, s.substances[2]); c++)
            {
                var d = new Dose(a, b, c);
                if (d.Total == 0 || d.Total > cfg.mixing.sliderMax || cfg.UltimateForRecipe(d) != null) continue;
                if (Formula.Stability(d, cfg) < opt.minStability) continue;
                float score = meat ? Formula.MeatFactor(d, cfg) : Formula.RateFactor(d, cfg);
                if (score > bestScore + 0.001f || (Mathf.Abs(score - bestScore) <= 0.001f && d.Total < best.Total))
                {
                    best = d;
                    bestScore = score;
                }
            }
            return best;
        }

        static bool HasStock(GameState s, int[] r) => s.substances[0] >= r[0] && s.substances[1] >= r[1] && s.substances[2] >= r[2];

        /// <summary>
        /// Seren vorbereiten: Kryo leeren, in der Zentrifuge im Grünen bremsen, Rohserum einfrieren, Normalserum
        /// konzentrieren, nachmischen. Ab Plan 3 goldene Seren (Ω für Milch/Eier, Koloss für Fleisch).
        /// </summary>
        void PrepareSerums(GameManager gm, GameState s)
        {
            var cfg = gm.Config;
            if (centrifuge == null) centrifuge = FindAnyObjectByType<Centrifuge>();
            if (cryoStation == null) cryoStation = FindAnyObjectByType<CryoChamber>();

            // Kryo: fertiges Serum herausnehmen
            if (s.cryo != null && s.cryo.Ready && s.serums.Count < cfg.mixing.rackSize && cryoStation != null)
                cryoStation.Execute();

            // Zentrifuge: im Grünen bremsen bzw. laden
            var job = s.centrifuge;
            if (job != null && job.spinning && MachineLogic.InGreen(job.rpm, cfg) && centrifuge != null)
            {
                centrifuge.Execute();
                Write("Zentrifuge gebremst bei " + (job.rpm * 100f).ToString("0") + " %");
            }
            else if (job == null && centrifuge != null && MachineLogic.FindForCentrifuge(s) >= 0)
                centrifuge.Execute();

            // Kryo: zentrifugiertes Rohserum einfrieren (wird ab Plan 3 golden)
            if (s.cryo == null && cryoStation != null && s.planIdx >= cfg.machinesFixed.cryo.unlockPlanIdx)
            {
                int i = s.serums.FindIndex(x => x.stage == SerumStage.Centrifuged);
                int first = s.serums.FindIndex(MachineLogic.CanFreeze);
                if (i >= 0 && i == first) cryoStation.Execute();
            }

            // Nachmischen, solange das Regal nicht voll ist
            if (MixingLogic.IsMixing(s) || s.serums.Count >= 3) return;
            bool meat = MeatBehind(s);
            bool ultOpen = s.planIdx >= cfg.machinesFixed.ultimate.unlockPlanIdx && s.planIdx >= cfg.machinesFixed.cryo.unlockPlanIdx;
            bool ultInWork = s.serums.Exists(x => x.stage != SerumStage.Ready || x.IsUltimate) || s.cryo != null
                             || (s.centrifuge != null && s.centrifuge.serum.stage == SerumStage.Raw);
            if (opt.ultimates && ultOpen && !ultInWork)
            {
                var ult = cfg.UltimateById(meat ? "koloss" : "omega");
                if (ult != null && HasStock(s, ult.recipe))
                {
                    s.mix = new Dose(ult.recipe[0], ult.recipe[1], ult.recipe[2]);
                    if (MixingSystem.StartMix()) Write("Mischen golden " + ult.id);
                    return;
                }
            }
            var d = BestDose(cfg, s, meat);
            // schwache Reste (z. B. nur C) lohnen nicht
            if (d.Total == 0 || (meat ? Formula.MeatFactor(d, cfg) : Formula.RateFactor(d, cfg)) < opt.minFactor) return;
            s.mix = d;
            if (MixingSystem.StartMix()) Write("Mischen " + d.Key + (meat ? " (Fleisch)" : " (Ertrag)"));
        }

        /// <summary>Bestes sprühbares Serum: golden passend zum Bedarf, sonst das stärkste fertige (Konzentration zählt mit).</summary>
        int BestSerum(GameManager gm, GameState s)
        {
            var cfg = gm.Config;
            bool meat = MeatBehind(s);
            int best = -1;
            float bestScore = -1f;
            for (int i = 0; i < s.serums.Count; i++)
            {
                var x = s.serums[i];
                if (x.stage != SerumStage.Ready) continue;
                float score;
                if (x.IsUltimate) score = (x.ultimateId == "koloss") == meat ? 100f : 50f;
                else score = (meat ? Formula.MeatFactor(x.dose, cfg) : Formula.RateFactor(x.dose, cfg)) * (x.IsConcentrated ? x.conc : 1f);
                if (score > bestScore)
                {
                    best = i;
                    bestScore = score;
                }
            }
            return best;
        }

        bool Report(string what, Feedback fb)
        {
            if (!fb.IsOk) Write(what + " abgelehnt: " + fb.key);
            else Write(what);
            return fb.IsOk;
        }
    }
}
