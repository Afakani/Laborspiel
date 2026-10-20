using System.Text;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Autopilot für Testdurchläufe (nur Editor und Development Builds, wie das Debug-Menü): spielt über die echten
    /// Spielsysteme (Bestellung, Klonen, Mischen, Sprühen, Verarbeiten, Not-Stopp, Reinigen, Sendung überspringen)
    /// im Zeitraffer bis zum Zielplan und schreibt Meldungen, Moduswechsel und Fehler in eine Logdatei.
    /// Entwicklerwerkzeug, daher Strategie-Werte als Felder und Logtexte im Code.
    /// </summary>
    public class DebugAutoPlay : MonoBehaviour
    {
        [SerializeField] float timeScale = 10f;
        [SerializeField] int stopAtPlan = 1;
        [SerializeField] int[] order = { 2, 4, 3, 1, 0, 0, 1 };
        [SerializeField] float processAfterSec = 35f;
        [SerializeField] string logPath = "Temp/autoplay_log.txt";

        public static DebugAutoPlay Instance { get; private set; }
        public bool Running { get; private set; }
        public bool Finished { get; private set; }

        readonly StringBuilder log = new StringBuilder();
        Chamber chamber;
        Cloth cloth;
        MixingStation mixing;
        DayMode lastMode = (DayMode)(-1);
        float animalAge;
        bool sprayed;
        int errors;

        void Awake()
        {
            if (!Debug.isDebugBuild) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>Durchlauf starten (Ziel: planIdx erreicht stopAtPlan).</summary>
        public void Begin(float scale, int targetPlanIdx)
        {
            timeScale = scale;
            stopAtPlan = targetPlanIdx;
            chamber = FindAnyObjectByType<Chamber>();
            cloth = FindAnyObjectByType<Cloth>();
            mixing = FindAnyObjectByType<MixingStation>();
            log.Clear();
            errors = 0;
            Running = true;
            Finished = false;
            EventBus.Toast -= OnToast;
            Application.logMessageReceived -= OnLog;
            EventBus.Toast += OnToast;
            Application.logMessageReceived += OnLog;
            Write("Start Autopilot, Zeitraffer " + timeScale + ", Ziel Plan-Index " + stopAtPlan);
        }

        void OnDestroy()
        {
            EventBus.Toast -= OnToast;
            Application.logMessageReceived -= OnLog;
        }

        void OnToast(string text, ToastKind kind) => Write("[" + kind + "] " + text);

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
                lastMode = dc.Mode;
                Write($"Modus {dc.Mode} · Kasse {s.Money:0} · Milch {s.quota?.prod.m:0}/{s.quota?.target.m:0} Eier {s.quota?.prod.e:0}/{s.quota?.target.e:0} Fleisch {s.quota?.prod.f:0}/{s.quota?.target.f:0} · Wut {s.anger:0}");
            }

            if (s.planIdx >= stopAtPlan || dc.Mode == DayMode.GameOver)
            {
                Running = false;
                Finished = true;
                gm.Clock.SetTimeScale(1f);
                Write($"Ende: Plan-Index {s.planIdx}, Tag {s.dayNum}, Modus {dc.Mode}, Fehler im Log: {errors}");
                return;
            }

            switch (dc.Mode)
            {
                case DayMode.Intro:
                    dc.StartFromIntro();
                    break;
                case DayMode.News:
                    if (dc.CanSkipNews) dc.SkipNews();
                    break;
                case DayMode.Play:
                    // der Autopilot „arbeitet“ ständig; keine Leerlauf-Wut (Prototyp-Testschritt setzt idleT = 0)
                    dc.DebugResetIdle();
                    if (s.pendingOrder == null) PlaceOrder(dc, s);
                    PlayShift(gm, s);
                    break;
            }
        }

        void PlaceOrder(DayCycle dc, GameState s)
        {
            var cfg = GameManager.Instance.Config;
            var amounts = (int[])order.Clone();
            // für morgen bestellen, so viel die Kasse hergibt (Rest bleibt fürs Klonen)
            while (OrderLogic.Sum(cfg, amounts) > s.Money * 0.6f)
            {
                int i = System.Array.FindLastIndex(amounts, a => a > 0);
                if (i < 0) break;
                amounts[i]--;
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

            // Kontrolle: mutiertes Tier mit dem Laken verdecken, verdächtiges Regal abdecken; danach wieder aufdecken
            bool inspect = s.inspection != null;
            if (cloth != null && (inspect && chamber.HasMutatedAnimal) != s.chamberCovered)
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
                var q = s.quota;
                bool needEggs = q != null && q.target.e > 0f && q.prod.e / q.target.e < q.prod.m / Mathf.Max(1f, q.target.m);
                var fb = chamber.Clone(needEggs ? AnimalKind.Hen : AnimalKind.Cow);
                if (fb.IsOk) Write("Klonen " + (needEggs ? "Huhn" : "Kuh"));
                return;
            }

            if (!chamber.HasLivingAnimal) return;
            animalAge += dt;

            // Serum mischen (B für Ertrag, C für Stabilität) und einmal sprühen
            if (!sprayed)
            {
                if (s.serums.Count == 0 && !MixingLogic.IsMixing(s))
                {
                    s.mix = new Dose(s.substances[0] > 0 ? 1 : 0, s.substances[1] > 0 ? 1 : 0, s.substances[2] > 0 ? 1 : 0);
                    if (s.mix.Total > 0 && MixingSystem.StartMix()) Write("Mischen " + s.mixPending);
                }
                else if (s.serums.Count > 0 && chamber.State == AnimalState.Ok)
                {
                    sprayed = Report("Sprühen", chamber.Spray(0));
                }
            }

            // Nach einer Weile verarbeiten (Fleisch), sonst gibt es keins
            if (animalAge >= processAfterSec || s.quota != null && s.quota.left < processAfterSec * 0.3f && animalAge > 5f)
                Report("Verarbeiten", chamber.SendToProcessing());
        }

        bool Report(string what, Feedback fb)
        {
            if (!fb.IsOk) Write(what + " abgelehnt: " + fb.key);
            else Write(what);
            return fb.IsOk;
        }
    }
}
