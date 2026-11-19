using System;
using System.Linq;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PlanErfuellt
{
    /// <summary>
    /// Leistungsmessung im Build (nur mit Startparameter -perflog): schreibt alle 5 s mittlere und schlechteste
    /// Frame-Zeit, CPU-Hauptthread, GPU-Zeit, Speicheranforderungen pro Bild, Draw Calls und Schattenwerfer ins Player.log.
    /// Für Vergleiche: -perfstart startet die Schicht ohne Einführung und klont eine Kuh, -perfnoshadows schaltet
    /// Echtzeitschatten aus, -perfnossao die Umgebungsverdeckung. Im Development-Build zeichnet -perfprofile 10 s
    /// Profiler-Daten nach perf.raw im Spielstand-Ordner auf.
    /// </summary>
    public class PerfProbe : MonoBehaviour
    {
        const float Window = 5f;
        const float ProfileStart = 8f, ProfileSec = 10f;

        ProfilerRecorder mainThread, drawCalls, setPass, shadowCasters, tris, gcAlloc;
        readonly FrameTiming[] timing = new FrameTiming[1];
        float timer, worst, sum, gpuSum, gcSum;
        int frames, gpuFrames;
        bool started, profiling, profiled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!Arg("-perflog")) return;
            var go = new GameObject("PerfProbe");
            DontDestroyOnLoad(go);
            go.AddComponent<PerfProbe>();
        }

        static bool Arg(string a) => Environment.GetCommandLineArgs().Contains(a);

        void OnEnable()
        {
            mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            shadowCasters = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Shadow Casters Count");
            tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            gcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            Debug.Log($"[Perf] Start: {SystemInfo.graphicsDeviceName}, {SystemInfo.graphicsDeviceType}, {SystemInfo.processorType}, " +
                      $"{Screen.width}x{Screen.height}, Development {Debug.isDebugBuild}");
        }

        void OnDisable()
        {
            mainThread.Dispose();
            drawCalls.Dispose();
            setPass.Dispose();
            shadowCasters.Dispose();
            tris.Dispose();
            gcAlloc.Dispose();
        }

        void Update()
        {
            float now = Time.realtimeSinceStartup;
            if (!started && now > 3f)
            {
                started = true;
                Setup();
            }
            if (Arg("-perfprofile") && Debug.isDebugBuild && !profiled)
            {
                if (!profiling && now > ProfileStart)
                {
                    profiling = true;
                    Profiler.logFile = System.IO.Path.Combine(Application.persistentDataPath, "perf");
                    Profiler.enableBinaryLog = true;
                    Profiler.enabled = true;
                    Debug.Log("[Perf] Profiler-Aufzeichnung läuft: " + Profiler.logFile + ".raw");
                }
                else if (profiling && now > ProfileStart + ProfileSec)
                {
                    Profiler.enabled = false;
                    Profiler.enableBinaryLog = false;
                    Profiler.logFile = "";
                    profiled = true;
                    Debug.Log("[Perf] Profiler-Aufzeichnung fertig");
                }
            }

            FrameTimingManager.CaptureFrameTimings();
            if (FrameTimingManager.GetLatestTimings(1, timing) > 0 && timing[0].gpuFrameTime > 0)
            {
                gpuSum += (float)timing[0].gpuFrameTime;
                gpuFrames++;
            }
            float dt = Time.unscaledDeltaTime;
            sum += dt;
            worst = Mathf.Max(worst, dt);
            gcSum += gcAlloc.LastValue;
            frames++;
            timer += dt;
            if (timer < Window) return;
            Debug.Log($"[Perf] {frames / sum:F0} FPS, Mittel {sum / frames * 1000f:F1} ms, schlechtester {worst * 1000f:F1} ms, " +
                      $"Hauptthread {mainThread.LastValue / 1e6:F1} ms, GPU {(gpuFrames > 0 ? gpuSum / gpuFrames : 0f):F1} ms, " +
                      $"Speicher pro Bild {gcSum / frames / 1024f:F1} KB, Draw Calls {drawCalls.LastValue}, SetPass {setPass.LastValue}, " +
                      $"Schattenwerfer {shadowCasters.LastValue}, Dreiecke {tris.LastValue}");
            timer = sum = worst = gpuSum = gcSum = 0f;
            frames = gpuFrames = 0;
        }

        void Setup()
        {
            // Messung ohne Bildbegrenzung
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            if (Arg("-perfstart") && DayCycle.Instance != null)
            {
                DayCycle.Instance.StartFromIntro(false);
                var card = FindAnyObjectByType<DayStartUI>();
                if (card != null) card.Close();
                var chamber = FindAnyObjectByType<Chamber>();
                if (chamber != null && !Arg("-perfempty")) chamber.Clone(Arg("-perfhen") ? AnimalKind.Hen : AnimalKind.Cow);
                Debug.Log("[Perf] Schicht gestartet" + (Arg("-perfempty") ? ", Kammer leer" : ", Tier geklont"));
            }
            if (Arg("-perfnopost"))
            {
                foreach (var v in FindObjectsByType<Volume>()) v.enabled = false;
                Debug.Log("[Perf] Nachbearbeitung aus");
            }
            if (Arg("-perfnoshadows"))
            {
                foreach (var l in FindObjectsByType<Light>()) l.shadows = LightShadows.None;
                Debug.Log("[Perf] Echtzeitschatten aus");
            }
            if (Arg("-perfnossao") && GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset rp)
            {
                foreach (var d in rp.rendererDataList)
                    if (d != null)
                        foreach (var f in d.rendererFeatures)
                            if (f != null && f.name.Contains("Ambient")) f.SetActive(false);
                Debug.Log("[Perf] SSAO aus");
            }
        }
    }
}
