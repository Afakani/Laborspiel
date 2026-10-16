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

        void Awake()
        {
            if (!Debug.isDebugBuild) Destroy(this);
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

            if (GUILayout.Button("Neues Spiel (Startwerte)")) gm.NewGame();

            GUI.DragWindow();
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
