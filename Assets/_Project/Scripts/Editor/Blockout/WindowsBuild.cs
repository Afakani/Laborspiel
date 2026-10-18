using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Windows-Build (Phase 12): Produktname, Vollbild 1920 × 1080, Szenen aus den Build-Einstellungen,
    /// Ausgabe nach Builds/Windows. Ergebnis zusätzlich in Temp/build_result.txt (für Läufe ohne Fenster).
    /// </summary>
    public static class WindowsBuild
    {
        public const string OutDir = "Builds/Windows";
        public const string ExeName = "PlanErfuellt.exe";
        public const string ResultPath = "Temp/build_result.txt";

        [MenuItem("Plan erfüllt/Windows-Build erstellen")]
        public static void Build()
        {
            PlayerSettings.productName = "Plan erfüllt";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.SetStaticBatchingForPlatform(BuildTarget.StandaloneWindows64, true);

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"{OutDir}/{ExeName}",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            System.IO.File.Delete(ResultPath);
            var report = BuildPipeline.BuildPlayer(options);
            var s = report.summary;
            string text = $"{s.result} · {s.totalSize / (1024f * 1024f):0.0} MB · {s.totalTime.TotalSeconds:0} s · Fehler {s.totalErrors} · Warnungen {s.totalWarnings} · {s.outputPath}";
            System.IO.File.WriteAllText(ResultPath, text);
            Debug.Log("Windows-Build: " + text);
        }
    }
}
