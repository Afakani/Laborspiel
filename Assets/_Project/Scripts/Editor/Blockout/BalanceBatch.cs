using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Balancing-Serie: spielt die Varianten aus Temp/batch_queue.txt nacheinander mit dem Autopilot (je ein Play-Durchgang,
    /// neues Spiel). Zeile: Name;A;B;C;Pumpen;VerarbeitenNachSek;MaxTage[;Legebatterien[;HuhnA;HuhnB;HuhnC]]
    /// (A = -1: Smart-Strategie statt fester Dosis).
    /// Fortschritt in Temp/batch_index.txt (übersteht den Domain-Reload), Zusammenfassungen in Temp/batch_summary.txt,
    /// Kennzahlen je Schicht wie gewohnt in Temp/balance_runs.csv. Entwicklerwerkzeug.
    /// </summary>
    [InitializeOnLoad]
    public static class BalanceBatch
    {
        const string QueuePath = "Temp/batch_queue.txt";
        const string IndexPath = "Temp/batch_index.txt";
        const string SummaryPath = "Temp/batch_summary.txt";
        const float TimeScale = 20f;

        static double phaseStart;
        static int phase;
        static DebugAutoPlay runner;

        static BalanceBatch()
        {
            EditorApplication.playModeStateChanged += OnPlayMode;
            EditorApplication.update += Tick;
        }

        [MenuItem("Plan erfüllt/Balancing-Serie starten")]
        public static void Start()
        {
            System.IO.File.WriteAllText(IndexPath, "0");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Plan erfüllt/Balancing-Serie abbrechen")]
        public static void Stop()
        {
            System.IO.File.Delete(IndexPath);
            EditorApplication.isPlaying = false;
        }

        static bool Active(out int index, out string[] lines)
        {
            index = -1;
            lines = null;
            if (!System.IO.File.Exists(IndexPath) || !System.IO.File.Exists(QueuePath)) return false;
            index = int.Parse(System.IO.File.ReadAllText(IndexPath).Trim());
            lines = System.IO.File.ReadAllLines(QueuePath);
            return index < lines.Length;
        }

        static void OnPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                phase = 0;
                runner = null;
                phaseStart = EditorApplication.timeSinceStartup;
            }
            else if (change == PlayModeStateChange.EnteredEditMode && Active(out int i, out var lines))
            {
                // nächste Variante
                System.IO.File.WriteAllText(IndexPath, (i + 1).ToString());
                if (i + 1 < lines.Length) EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
                else System.IO.File.AppendAllText(SummaryPath, "FERTIG\n");
            }
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying || !Active(out int index, out var lines)) return;
            double t = EditorApplication.timeSinceStartup - phaseStart;
            if (phase == 0 && t > 1.5)
            {
                foreach (var b in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude))
                    if (b.name == "NewGame") { b.onClick.Invoke(); break; }
                phase = 1;
            }
            else if (phase == 1 && t > 2.5)
            {
                runner = new GameObject("AutoPlayRun").AddComponent<DebugAutoPlay>();
                runner.Begin(TimeScale, GameManager.Instance.Config.day.plans, Parse(lines[index]));
                phase = 2;
            }
            else if (phase == 2 && runner != null && runner.Finished)
            {
                System.IO.File.AppendAllText(SummaryPath, runner.Summary + "\n");
                phase = 3;
                EditorApplication.isPlaying = false;
            }
        }

        static DebugAutoPlay.Options Parse(string line)
        {
            var p = line.Split(';');
            int a = int.Parse(p[1]);
            var o = new DebugAutoPlay.Options
            {
                name = p[0],
                pumps = int.Parse(p[4]),
                processAfter = float.Parse(p[5], System.Globalization.CultureInfo.InvariantCulture),
                maxDays = int.Parse(p[6]),
                ultimates = false
            };
            if (p.Length > 7) o.layers = int.Parse(p[7]);
            if (a >= 0) o.dose = new[] { a, int.Parse(p[2]), int.Parse(p[3]) };
            if (p.Length > 10) o.henDose = new[] { int.Parse(p[8]), int.Parse(p[9]), int.Parse(p[10]) };
            return o;
        }
    }
}
