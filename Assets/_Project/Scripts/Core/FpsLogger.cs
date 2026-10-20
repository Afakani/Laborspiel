using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Leistungsmessung im Build: mit der Startoption -fpslog wird alle paar Sekunden der Durchschnitt, das langsamste
    /// Bild und die Anzahl Bilder unter 60 FPS ins Player.log geschrieben. Ohne die Option passiert nichts.
    /// </summary>
    public class FpsLogger : MonoBehaviour
    {
        const string Arg = "-fpslog";
        [SerializeField] float intervalSec = 10f;

        float t, worst;
        int frames, slow;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), Arg) < 0) return;
            var go = new GameObject("FpsLogger");
            DontDestroyOnLoad(go);
            go.AddComponent<FpsLogger>();
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            t += dt;
            frames++;
            worst = Mathf.Max(worst, dt);
            if (dt > 1f / 60f + 0.0005f) slow++;
            if (t < intervalSec) return;
            Debug.Log($"FPS Ø {frames / t:0.0} · langsamstes Bild {worst * 1000f:0.0} ms · unter 60: {slow}/{frames} · {Screen.width}×{Screen.height} · Qualität {QualitySettings.GetQualityLevel()}");
            t = worst = 0f;
            frames = slow = 0;
        }
    }
}
