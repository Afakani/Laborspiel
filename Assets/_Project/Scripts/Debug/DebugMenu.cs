using UnityEngine;
using UnityEngine.InputSystem;

namespace PlanErfuellt
{
    /// <summary>
    /// Debug-Menü (F1), nur im Editor und in Development Builds (Debug.isDebugBuild). Reines Entwicklerwerkzeug,
    /// deshalb stehen die Beschriftungen hier im Code und nicht in der Text-JSON.
    /// </summary>
    public class DebugMenu : MonoBehaviour
    {
        const int WindowId = 0x0B12;

        [SerializeField] float[] timeScales = { 1f, 2f, 5f, 10f };
        [SerializeField] float[] moneySteps = { 100f, 1000f };

        bool visible;
        Rect window = new Rect(20, 80, 300, 10);
        string moneyInput = "";
        Chamber chamber;
        InfectionSystem infection;

        void Awake()
        {
            if (!Debug.isDebugBuild) { Destroy(this); return; }
            chamber = FindAnyObjectByType<Chamber>();
            infection = FindAnyObjectByType<InfectionSystem>();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && kb.f1Key.wasPressedThisFrame) visible = !visible;
        }

        void OnGUI()
        {
            if (!visible || GameManager.Instance == null || GameManager.Instance.State == null) return;
            window = GUILayout.Window(WindowId, window, DrawWindow, "DEBUG (F1)");
        }

        void DrawWindow(int id)
        {
            var gm = GameManager.Instance;
            var s = gm.State;
            var clock = gm.Clock;

            GUILayout.Label($"Tag {s.dayNum} · Plan {s.planIdx + 1} · Spielzeit {clock.Time:0.0} s");

            // Zeitraffer
            GUILayout.Label($"Zeitraffer: ×{clock.TimeScale:0.##}{(clock.Paused ? " (PAUSE)" : "")}");
            GUILayout.BeginHorizontal();
            foreach (var ts in timeScales)
                if (GUILayout.Button($"×{ts:0.##}")) clock.SetTimeScale(ts);
            if (GUILayout.Button(clock.Paused ? "Weiter" : "Pause")) clock.TogglePause();
            GUILayout.EndHorizontal();

            // Geld
            GUILayout.Label($"Kasse: {s.Money:0} SM");
            GUILayout.BeginHorizontal();
            foreach (var m in moneySteps)
            {
                if (GUILayout.Button($"+{m:0}")) s.AddMoney(m);
                if (GUILayout.Button($"−{m:0}")) s.AddMoney(-m);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            moneyInput = GUILayout.TextField(moneyInput, GUILayout.Width(100));
            if (GUILayout.Button("Setzen") && float.TryParse(moneyInput, out var v)) s.SetMoney(v);
            GUILayout.EndHorizontal();

            // Lager
            GUILayout.Label("Lager");
            string[] names = { "A", "B", "C" };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                StockRow($"{names[i]}: {s.substances[i]}", () => s.AddSubstance(idx, -1), () => s.AddSubstance(idx, 1));
            }
            StockRow($"Gegenmittel: {s.antidote}", () => { s.antidote = Mathf.Max(0, s.antidote - 1); s.NotifyInventoryChanged(); },
                () => { s.antidote++; s.NotifyInventoryChanged(); });
            StockRow($"Wodka: {s.vodka}", () => { s.vodka = Mathf.Max(0, s.vodka - 1); s.NotifyInventoryChanged(); },
                () => { s.vodka++; s.NotifyInventoryChanged(); });
            StockRow($"Kaviar: {s.caviar}", () => { s.caviar = Mathf.Max(0, s.caviar - 1); s.NotifyInventoryChanged(); },
                () => { s.caviar++; s.NotifyInventoryChanged(); });

            DrawDay(gm, s);
            DrawInspection(gm, s);
            DrawRisks(gm, s);

            if (GUILayout.Button("Neues Spiel (Startwerte)")) gm.NewGame();

            GUI.DragWindow();
        }

        /// <summary>Phase 6: Schicht abkürzen, Plan erfüllen, Wut ändern.</summary>
        void DrawDay(GameManager gm, GameState s)
        {
            var dc = DayCycle.Instance;
            var q = s.quota;
            GUILayout.Label($"Ablauf: {(dc != null ? dc.Mode.ToString() : "-")} · Rest {(q != null ? q.left : 0f):0} s · erfüllt {s.quotasMet} · verfehlt {s.quotasFailed}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Plan erfüllen") && q != null)
            {
                q.prod.m = Mathf.Max(q.prod.m, q.target.m);
                q.prod.e = Mathf.Max(q.prod.e, q.target.e);
                q.prod.f = Mathf.Max(q.prod.f, q.target.f);
            }
            if (GUILayout.Button("Schicht beenden") && q != null) q.left = 0.05f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Wut +20")) s.AddAnger(20f, gm.Config);
            if (GUILayout.Button("Wut −20")) s.AddAnger(-20f, gm.Config);
            if (GUILayout.Button("Wunsch jetzt") && q != null) s.wishTimer = 0.01f;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Plan {s.planIdx + 1}", GUILayout.Width(60));
            if (GUILayout.Button("Plan −")) s.planIdx = Mathf.Max(0, s.planIdx - 1);
            if (GUILayout.Button("Plan +")) s.planIdx = Mathf.Min(gm.Config.day.plans - 1, s.planIdx + 1);
            GUILayout.EndHorizontal();
            var z = s.centrifuge;
            var k = s.cryo;
            GUILayout.Label($"Zentrifuge {(z != null ? $"{z.rpm * 100f:0} % {(z.spinning ? "hoch" : "aus")}" : "-")} · Kryo {(k != null ? (k.Ready ? $"raus {k.windowLeft:0} s" : $"friert {k.freezeLeft:0} s") : "-")}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Mix 2-3-5")) { s.serums.Add(new Serum(new Dose(2, 3, 5), SerumStage.Raw)); EventBus.RaiseSerumRackChanged(); }
            if (GUILayout.Button("Kryo fertig") && k != null) k.freezeLeft = 0.01f;
            GUILayout.EndHorizontal();
        }

        /// <summary>Phase 7: Parteikontrolle und Kommissar.</summary>
        void DrawInspection(GameManager gm, GameState s)
        {
            var ins = s.inspection;
            GUILayout.Label(ins != null
                ? $"Kontrolle: x {ins.x:0.0} · Verdacht {ins.suspicion:0} · Pause {ins.pause:0.0} · {(ins.boss ? "Oberkommissar" : "Kommissar")} · Regal {(s.rackHidden ? "zu" : "offen")}"
                : $"Keine Kontrolle · nächste bei {s.nextInspectionAt:0} s (jetzt {s.playTime:0} s) · Wodka {s.vodka}");
            GUILayout.BeginHorizontal();
            var sys = FindAnyObjectByType<InspectionSystem>();
            if (GUILayout.Button("Kontrolle jetzt") && sys != null) sys.ForceStart(false);
            if (GUILayout.Button("Oberkommissar") && sys != null) sys.ForceStart(true);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kontrolle +25")) s.AddAuthority(25f, gm.Config);
            if (GUILayout.Button("Verdacht +25") && ins != null) ins.suspicion += 25f;
            if (GUILayout.Button("Kommissar zur Kammer") && ins != null) ins.x = gm.Config.layout.chamber.x + gm.Config.inspection.range;
            GUILayout.EndHorizontal();
        }

        /// <summary>Phase 5: Risiken, Explosion und Infektion schnell auslösen.</summary>
        void DrawRisks(GameManager gm, GameState s)
        {
            GUILayout.Label($"Wut {s.anger:0} · Kontrolle {s.authority:0} · Explosionen {s.explosions}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tag −")) s.dayNum = Mathf.Max(0, s.dayNum - 1);
            if (GUILayout.Button("Tag +")) s.dayNum++;
            GUILayout.EndHorizontal();
            if (chamber != null)
            {
                var st = chamber.Stats;
                GUILayout.Label($"Tier: {chamber.State} · Stab {chamber.Stability:0} · Hitze {st.heat:0} · Säure {st.acidTimer:0.0} · Schweben {st.floatHeight:0.0}");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Kuh klonen")) chamber.Clone(AnimalKind.Cow).Report(gm);
                if (GUILayout.Button("Serum 2-2-0")) { s.serums.Add(new Serum(new Dose(2, 2, 0), SerumStage.Raw)); EventBus.RaiseSerumRackChanged(); }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Sprühen")) chamber.Spray(s.serums.Count - 1).Report(gm);
                if (GUILayout.Button("NOT-STOPP")) chamber.NotStop().Report(gm);
                if (GUILayout.Button("Explosion")) chamber.Explode();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Stromausfall") && s.blackoutTimer <= 0f)
            {
                s.blackoutTimer = gm.Config.risks.blackoutSec;
                gm.Toast("ui.toasts.cableTorn", ToastKind.Bad);
                EventBus.RaiseRisk(RiskEvent.CableTorn);
            }
            if (GUILayout.Button("Band reißen") && !s.beltBroken)
            {
                s.beltBroken = true;
                gm.Toast("ui.toasts.beltBroken", ToastKind.Bad);
                EventBus.RaiseRisk(RiskEvent.BeltBroken);
            }
            if (GUILayout.Button("Infizieren") && infection != null) infection.ForceInfect();
            GUILayout.EndHorizontal();
        }

        static void StockRow(string label, System.Action minus, System.Action plus)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(150));
            if (GUILayout.Button("−", GUILayout.Width(30))) minus();
            if (GUILayout.Button("+", GUILayout.Width(30))) plus();
            GUILayout.EndHorizontal();
        }
    }
}
