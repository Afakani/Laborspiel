using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Startet alle EditMode-Tests und schreibt das Ergebnis nach Temp/editmode_result.txt
    /// (Fehlschläge nach Temp/editmode_fail.txt). Für Läufe ohne Test-Runner-Fenster.
    /// </summary>
    public static class EditModeTestRun
    {
        public const string ResultPath = "Temp/editmode_result.txt";
        public const string FailPath = "Temp/editmode_fail.txt";

        [MenuItem("Plan erfüllt/EditMode-Tests ausführen")]
        public static void Start()
        {
            System.IO.File.Delete(ResultPath);
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Callbacks());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
        }

        class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor t) { }
            public void TestStarted(ITestAdaptor t) { }
            public void TestFinished(ITestResultAdaptor r) { }

            public void RunFinished(ITestResultAdaptor r)
            {
                var sb = new StringBuilder();
                Collect(r, sb);
                System.IO.File.WriteAllText(FailPath, sb.ToString());
                System.IO.File.WriteAllText(ResultPath, $"pass {r.PassCount} fail {r.FailCount} skip {r.SkipCount}");
            }

            static void Collect(ITestResultAdaptor r, StringBuilder sb)
            {
                if (r.HasChildren)
                {
                    foreach (var c in r.Children) Collect(c, sb);
                    return;
                }
                if (r.TestStatus == TestStatus.Failed) sb.AppendLine(r.FullName + ": " + r.Message);
            }
        }
    }
}
