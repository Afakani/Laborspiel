using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Explosion (GDD 5.4, Prototyp explode): Schleim- und Feuerwolke, grüne Schleimflecken am Boden und ein
    /// Brandfleck in der Kammer. Die Flecken bleiben, bis die Kammer gereinigt ist. Auch die Explosion in der
    /// Verarbeitung (Mut-Bonus verloren) bekommt eine Wolke.
    /// </summary>
    public class ExplosionFx : MonoBehaviour
    {
        [SerializeField] ParticleSystem slimeBurst;
        [SerializeField] ParticleSystem fireBurst;
        [SerializeField] Renderer[] splats = new Renderer[0];
        [SerializeField] Renderer scorch;
        [SerializeField] int slimeCount = 90;
        [SerializeField] int fireCount = 30;

        public void Setup(ParticleSystem slimeFx, ParticleSystem fireFx, Renderer[] splatRenderers, Renderer scorchRenderer)
        {
            slimeBurst = slimeFx;
            fireBurst = fireFx;
            splats = splatRenderers;
            scorch = scorchRenderer;
        }

        void Start() => Show(false);
        void OnEnable() => EventBus.Risk += OnRisk;
        void OnDisable() => EventBus.Risk -= OnRisk;

        void OnRisk(RiskEvent e)
        {
            var cfg = GameManager.Instance.Config;
            if (e == RiskEvent.Explosion)
            {
                var c = LayoutConfig.ToUnity(cfg.layout.chamber.x, 0f, cfg.layout.chamber.z);
                Burst(c + Vector3.up * 2.5f, 1f);
                PlaceSplats(c, cfg);
                Show(true);
            }
            else if (e == RiskEvent.ProcessingExplosion)
            {
                var p = cfg.layout.Station("processing");
                Burst(LayoutConfig.ToUnity(p.x, 0f, p.z) + Vector3.up * 2.8f, 0.7f);
            }
        }

        void Burst(Vector3 pos, float k)
        {
            slimeBurst.transform.position = pos;
            fireBurst.transform.position = pos;
            slimeBurst.Emit(Mathf.RoundToInt(slimeCount * k));
            fireBurst.Emit(Mathf.RoundToInt(fireCount * k));
        }

        /// <summary>Flecken zufällig rund um die Kammer verteilen (Prototyp: Abstand 2,8 bis 9,3 m).</summary>
        void PlaceSplats(Vector3 center, GameConfig cfg)
        {
            var fx = cfg.fx;
            var room = cfg.layout.room;
            for (int i = 0; i < splats.Length; i++)
            {
                float a = Random.value * Mathf.PI * 2f;
                float r = Mathf.Lerp(fx.splatRadiusMin, fx.splatRadiusMax, Random.value);
                var p = center + new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r * 0.8f);
                p.z = Mathf.Clamp(p.z, -room.backWallZ + 1f, -room.frontWallZ - 0.5f);
                var t = splats[i].transform;
                t.position = new Vector3(p.x, 0.012f + i * 0.0005f, p.z);
                t.rotation = Quaternion.Euler(90f, Random.value * 360f, 0f);
                t.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.9f, Random.value);
            }
        }

        void Show(bool on)
        {
            foreach (var s in splats) s.enabled = on;
            if (scorch != null) scorch.enabled = on;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || scorch == null) return;
            // Nach dem Reinigen verschwinden die Spuren
            if (scorch.enabled && !gm.State.contaminated) Show(false);
        }
    }
}
