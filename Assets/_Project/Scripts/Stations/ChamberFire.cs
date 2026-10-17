using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Vernichtung per Not-Stopp: Flammen in der Kammer statt Gas. Düsenlicht und Lichtkegel werden orange,
    /// ein flackerndes Feuerlicht leuchtet, Gas und grüner Nebel treten zurück. Dauer: animal.destroySec + chamberFire.afterSec.
    /// </summary>
    public class ChamberFire : MonoBehaviour
    {
        static readonly int ShaftColorId = Shader.PropertyToID("_Color");
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] ParticleSystem[] flames = new ParticleSystem[0];
        [SerializeField] Light fireLight;
        [SerializeField] Light[] nozzleLights = new Light[0];
        [SerializeField] Renderer[] nozzleCones = new Renderer[0];
        [SerializeField] ChamberGas gas;
        [SerializeField] Renderer fog;

        Color[] lightBase, coneBase;
        Color fogBase;
        MaterialPropertyBlock mpb;
        float timer, k;
        bool burning;

        public void Setup(ParticleSystem[] fx, Light light, Light[] lights, Renderer[] cones, ChamberGas chamberGas, Renderer fogRenderer)
        {
            fog = fogRenderer;
            flames = fx;
            fireLight = light;
            nozzleLights = lights;
            nozzleCones = cones;
            gas = chamberGas;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            lightBase = new Color[nozzleLights.Length];
            for (int i = 0; i < nozzleLights.Length; i++) lightBase[i] = nozzleLights[i].color;
            coneBase = new Color[nozzleCones.Length];
            for (int i = 0; i < nozzleCones.Length; i++) coneBase[i] = nozzleCones[i].sharedMaterial.GetColor(ShaftColorId);
            if (fog != null) fogBase = fog.sharedMaterial.GetColor(BaseColorId);
            fireLight.intensity = 0f;
            foreach (var f in flames) f.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        void OnEnable() => EventBus.AnimalStateChanged += OnState;
        void OnDisable() => EventBus.AnimalStateChanged -= OnState;

        void OnState(AnimalState s)
        {
            if (s != AnimalState.Destroy) return;
            var cfg = GameManager.Instance.Config;
            timer = cfg.animal.destroySec + cfg.chamberFire.afterSec;
            burning = true;
            foreach (var f in flames) f.Play(true);
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var c = gm.Config.chamberFire;
            float dt = gm.Clock.DeltaTime;

            if (burning)
            {
                timer -= dt;
                if (timer <= 0f)
                {
                    burning = false;
                    foreach (var f in flames) f.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            k = Mathf.MoveTowards(k, burning ? 1f : 0f, c.fadeSec > 0f ? dt / c.fadeSec : 1f);

            var fireColor = Hex(c.lightColor);
            var nozzleColor = Hex(c.nozzleColor);
            float flicker = 1f + c.flickerAmp * (Mathf.PerlinNoise(gm.Clock.Time * c.flickerFreq, 0.3f) * 2f - 1f);
            fireLight.color = fireColor;
            fireLight.intensity = k * c.lightIntensity * flicker;

            for (int i = 0; i < nozzleLights.Length; i++) nozzleLights[i].color = Color.Lerp(lightBase[i], nozzleColor, k);
            for (int i = 0; i < nozzleCones.Length; i++)
            {
                nozzleCones[i].GetPropertyBlock(mpb);
                mpb.SetColor(ShaftColorId, Color.Lerp(coneBase[i], nozzleColor, k));
                nozzleCones[i].SetPropertyBlock(mpb);
            }
            if (gas != null) gas.FireAmount = k;
            if (fog != null)
            {
                // Grüner Nebel wird zu dunklem, orange angestrahltem Qualm
                var smoke = new Color(nozzleColor.r * 0.5f, nozzleColor.g * 0.3f, nozzleColor.b * 0.15f, fogBase.a);
                fog.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, Color.Lerp(fogBase, smoke, k));
                fog.SetPropertyBlock(mpb);
            }
        }

        static Color Hex(string h) => ColorUtility.TryParseHtmlString(h, out var col) ? col : Color.white;
    }
}
