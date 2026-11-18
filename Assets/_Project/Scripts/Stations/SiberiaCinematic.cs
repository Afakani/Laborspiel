using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Spielende Sibirien (D162): nach zu vielen verfehlten Plänen in Folge (DayMode.Arrest) übernimmt eine eigene
    /// Kamera mit Kinobalken. Der Direktor verleugnet den Spieler, das Rolltor rechts unter dem Laufsteg geht hoch,
    /// zwei Stasi-Männer stehen im Gegenlicht, kommen herein und sprechen (Stimme stasi_line0/1, Untertitel), dann
    /// Abblende und Endbildschirm. Zeiten aus siberia.
    /// </summary>
    public class SiberiaCinematic : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] Transform shutter;
        [SerializeField] Light gateLight;
        [SerializeField] Transform[] men = new Transform[0];
        [SerializeField] Transform[] legsL = new Transform[0];
        [SerializeField] Transform[] legsR = new Transform[0];
        [SerializeField] GameObject overlay;
        [SerializeField] RectTransform barTop, barBottom;
        [SerializeField] Image fade;
        [SerializeField] TMP_Text subtitle;

        CanvasGroup hud;
        Vector3[] menStart;
        Vector3 shutterPos, shutterScale;
        float t = -1f;
        int step;

        public void Setup(Camera camera, Transform shutterTf, Light light, Transform[] figures, Transform[] left, Transform[] right,
            GameObject overlayRoot, RectTransform top, RectTransform bottom, Image fadeImage, TMP_Text subtitleText)
        {
            cam = camera;
            shutter = shutterTf;
            gateLight = light;
            men = figures;
            legsL = left;
            legsR = right;
            overlay = overlayRoot;
            barTop = top;
            barBottom = bottom;
            fade = fadeImage;
            subtitle = subtitleText;
        }

        void Awake()
        {
            menStart = new Vector3[men.Length];
            for (int i = 0; i < men.Length; i++) menStart[i] = men[i].localPosition;
            shutterPos = shutter.localPosition;
            shutterScale = shutter.localScale;
            Show(false);
        }

        void OnEnable() => EventBus.Arrest += Begin;
        void OnDisable() => EventBus.Arrest -= Begin;

        void Begin()
        {
            var gm = GameManager.Instance;
            var c = gm.Config.siberia;
            var hudGo = FindAnyObjectByType<PromptUI>();
            hud = hudGo != null ? hudGo.GetComponent<CanvasGroup>() : null;
            if (hud != null) hud.alpha = 0f;
            barTop.anchorMin = new Vector2(0f, 1f - c.letterbox);
            barBottom.anchorMax = new Vector2(1f, c.letterbox);
            cam.fieldOfView = c.camFov;
            subtitle.text = "";
            t = 0f;
            step = 0;
            Show(true);
            Pose(0f, c);
        }

        void Show(bool on)
        {
            cam.enabled = on;
            overlay.SetActive(on);
            gateLight.enabled = on;
            foreach (var m in men) m.gameObject.SetActive(on);
            if (!on)
            {
                shutter.localPosition = shutterPos;
                shutter.localScale = shutterScale;
                for (int i = 0; i < men.Length; i++) men[i].localPosition = menStart[i];
            }
        }

        void Update()
        {
            if (t < 0f) return;
            var gm = GameManager.Instance;
            var c = gm.Config.siberia;
            t += Time.unscaledDeltaTime;
            Pose(t, c);

            // Ereignisse in Reihenfolge
            if (step == 0 && t >= c.directorSec)
            {
                step++;
                string line = gm.Texts.Get("siberia.director");
                var director = FindAnyObjectByType<Director>();
                if (director != null) director.Say(line);
                subtitle.text = gm.Texts.Get("ui.hud.subtitleSpeaker") + line;
            }
            if (step == 1 && t >= c.openStartSec)
            {
                step++;
                if (AudioService.Instance != null) AudioService.Instance.Play("crank", 0.6f);
            }
            if (step == 2 && t >= c.line1Sec) Line(0, gm);
            if (step == 3 && t >= c.line2Sec) Line(1, gm);
            if (step == 4 && t >= c.endSec)
            {
                step++;
                t = -1f;
                Show(false);
                if (hud != null) hud.alpha = 1f;
                DayCycle.Instance.GameOver(GameOverKind.Siberia);
            }
        }

        /// <summary>Debug/Test: zu einem Zeitpunkt der laufenden Sequenz springen.</summary>
        public void DebugSeek(float time)
        {
            if (t < 0f) return;
            t = time;
            Pose(t, GameManager.Instance.Config.siberia);
        }

        void Line(int i, GameManager gm)
        {
            step++;
            if (AudioService.Instance != null) AudioService.Instance.PlayAnnounce("stasi_line" + i, gm.Config.siberia.voiceVolume);
            subtitle.text = gm.Texts.Get("ui.hud.stasiSpeaker") + gm.Texts.Get($"siberia.lines[{i}]");
        }

        /// <summary>Alles zum Zeitpunkt time stellen: Blende, Kamera, Tor, Gegenlicht, Männer.</summary>
        void Pose(float time, SiberiaConfig c)
        {
            float fadeIn = 1f - Mathf.Clamp01(time / Mathf.Max(0.01f, c.fadeInSec));
            float fadeOut = Mathf.Clamp01((time - c.fadeOutStartSec) / Mathf.Max(0.01f, c.endSec - c.fadeOutStartSec));
            fade.color = new Color(0f, 0f, 0f, Mathf.Max(fadeIn, fadeOut));

            // Kamera fährt langsam auf das Tor zu
            float k = Smooth(Mathf.Clamp01(time / c.endSec));
            var from = LayoutConfig.ToUnity(c.camX, c.camHeight, c.camZ - c.dolly * k);
            var look = LayoutConfig.ToUnity(c.gateX, c.camLookHeight, c.gateZ);
            cam.transform.position = from;
            cam.transform.rotation = Quaternion.LookRotation(look - from);

            // Rolltor rollt nach oben auf (wird kürzer, Unterkante steigt)
            float open = Smooth(Mathf.Clamp01((time - c.openStartSec) / c.openSec));
            float rest = 0.06f;
            float sy = Mathf.Lerp(1f, rest, open);
            shutter.localScale = new Vector3(shutterScale.x, shutterScale.y * sy, shutterScale.z);
            shutter.localPosition = shutterPos + Vector3.up * (c.gateHeight * (1f - sy) * 0.5f);
            gateLight.intensity = c.lightIntensity * open;

            // Stasi-Männer gehen nach vorn, danach stehen sie still
            float w = Mathf.Clamp01((time - c.walkStartSec) / c.walkSec);
            bool walking = w > 0f && w < 1f;
            for (int i = 0; i < men.Length; i++)
            {
                men[i].localPosition = menStart[i] + Vector3.back * (c.walkDist * w);
                float swing = walking ? Mathf.Sin(time * c.stepFreq * 2f * Mathf.PI) * c.legSwingDeg : 0f;
                legsL[i].localEulerAngles = new Vector3(swing, 0f, 0f);
                legsR[i].localEulerAngles = new Vector3(-swing, 0f, 0f);
            }
        }

        static float Smooth(float x) => x * x * (3f - 2f * x);
    }
}
