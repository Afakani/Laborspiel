using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Fernseher mit dem Direktor (GDD 7.2): Beim Sprechen erscheint der Spruch auf dem Parolenfeld und das Bild
    /// pulsiert und der Direktor (DirectorRig) bewegt den Mund; ist er cholerisch (dritte Wut-Stufe), flackert das Bild
    /// und das TV-Licht wird rot. Ab der ersten Wut-Stufe wackelt der ganze Fernseher (stärker mit der Wut, feel.tvShake*);
    /// nach erfülltem Plan jubelt der Direktor (feel.cheerSec).
    /// </summary>
    public class TvDirectorFx : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] Renderer screen;
        [SerializeField] TMP_Text slogan;
        [SerializeField] Light tvLight;
        [SerializeField] DirectorRig rig;
        [SerializeField] float screenEmission = 1.6f;
        [SerializeField] Color calmLight = new Color(1f, 0.42f, 0.31f);
        [SerializeField] Color rageLight = new Color(1f, 0.08f, 0.05f);
        [SerializeField] float calmIntensity = 1.5f;
        [SerializeField] float rageIntensity = 4f;

        MaterialPropertyBlock mpb;
        Vector3 basePos;
        Quaternion baseRot;
        float speakLeft;
        string idleText;
        float sloganSize;

        public void Setup(Renderer screenRenderer, TMP_Text sloganText, Light light, DirectorRig directorRig)
        {
            rig = directorRig;
            screen = screenRenderer;
            slogan = sloganText;
            tvLight = light;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            sloganSize = slogan.fontSize;
            basePos = transform.localPosition;
            baseRot = transform.localRotation;
        }

        void OnEnable()
        {
            EventBus.DirectorSpeak += OnSpeak;
            EventBus.QuotaMet += OnPlanMet;
        }

        void OnDisable()
        {
            EventBus.DirectorSpeak -= OnSpeak;
            EventBus.QuotaMet -= OnPlanMet;
        }

        void OnPlanMet()
        {
            if (rig != null) rig.Cheer(GameManager.Instance.Config.feel.cheerSec);
        }

        void Start() => idleText = GameManager.Instance.Texts.Get("ui.signs.tvSlogan");

        void OnSpeak(string line)
        {
            var ui = GameManager.Instance.Config.ui;
            speakLeft = Mathf.Max(ui.subtitleMinSec, line.Length * ui.subtitlePerChar);
            slogan.enableAutoSizing = true;
            slogan.fontSizeMin = sloganSize * 0.35f;
            slogan.fontSizeMax = sloganSize * 0.8f;
            slogan.rectTransform.sizeDelta = new Vector2(1.5f, 2.8f);
            slogan.textWrappingMode = TMPro.TextWrappingModes.Normal;
            slogan.text = line;
            if (rig != null) rig.Speak(speakLeft);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float t = Time.unscaledTime;
            if (speakLeft > 0f)
            {
                speakLeft -= Time.unscaledDeltaTime;
                if (speakLeft <= 0f)
                {
                    slogan.enableAutoSizing = false;
                    slogan.fontSize = sloganSize;
                    slogan.text = idleText;
                }
            }
            bool rage = gm.State.anger >= gm.Config.anger.tiers[1];
            float e = screenEmission;
            if (speakLeft > 0f) e *= 0.85f + 0.25f * Mathf.Abs(Mathf.Sin(t * 9f));
            if (rage && Mathf.PerlinNoise(t * 6f, 0.5f) < 0.3f) e *= 0.15f;
            if (rig != null) rig.SetEmission(e);
            else
            {
                screen.GetPropertyBlock(mpb);
                mpb.SetColor(EmissionId, Color.white * e);
                screen.SetPropertyBlock(mpb);
            }
            // Wut: der Fernseher wackelt in der Aufhängung (ab der ersten Stufe, mit der Wut stärker)
            var anger = gm.Config.anger;
            var f = gm.Config.feel;
            float k = Mathf.Clamp01((gm.State.anger - anger.tiers[0]) / Mathf.Max(1f, anger.max - anger.tiers[0]));
            float nx = Mathf.PerlinNoise(t * f.tvShakeFreq, 0.1f) - 0.5f, ny = Mathf.PerlinNoise(0.7f, t * f.tvShakeFreq) - 0.5f;
            transform.localPosition = basePos + new Vector3(nx, ny, 0f) * (2f * f.tvShakeM * k);
            transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, nx * 2f * f.tvShakeDeg * k);
            tvLight.color = rage ? rageLight : calmLight;
            tvLight.intensity = rage ? rageIntensity * (0.6f + 0.4f * Mathf.Sin(t * 5f)) : calmIntensity;
        }
    }
}
