using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Sichtbare Mutation und Bewegung eines Tiermodells (GDD 5.5, Prototyp updateSubject): Wachsen mit A,
    /// Aufblähen mit A und B ≥ 3, größere und zusätzliche Euter mit B, Glühen bei Instabilität (rot bei Hitze,
    /// golden bei Ω), Beine beim Laufen/Schweben, Zittern, Flügelschlag beim Huhn.
    /// </summary>
    public class AnimalVisuals : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] AnimalKind kind;
        [SerializeField] Transform body;
        [SerializeField] Transform[] legs = new Transform[0];
        [SerializeField] Transform udder;
        [SerializeField] GameObject[] extraUdders = new GameObject[0];
        [SerializeField] Transform[] wings = new Transform[0];
        [SerializeField] Renderer[] bodyRenderers = new Renderer[0];
        [SerializeField] Renderer[] spotRenderers = new Renderer[0];

        MaterialPropertyBlock mpb;
        Vector3 bodyBase;
        Quaternion[] legBase, wingBase;
        float infl, udderScale = 1f, glow;
        int shownExtra = -1;

        public AnimalKind Kind => kind;
        /// <summary>Aktuelles Glühen 0..1 (für das Kammerlicht).</summary>
        public float Glow => glow;
        /// <summary>Faktor für Leuchten und Glühlicht (Zwischensequenz, sonst 1).</summary>
        public float GlowBoost { get; set; } = 1f;
        public Color GlowColor { get; private set; }
        public float Pulse { get; private set; } = 1f;

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            bodyBase = body.localScale;
            legBase = new Quaternion[legs.Length];
            for (int i = 0; i < legs.Length; i++) legBase[i] = legs[i].localRotation;
            wingBase = new Quaternion[wings.Length];
            for (int i = 0; i < wings.Length; i++) wingBase[i] = wings[i].localRotation;
        }

        /// <summary>Neues Tier: Übergänge zurücksetzen.</summary>
        public void ResetLook()
        {
            infl = 0f;
            udderScale = 1f;
            glow = 0f;
            shownExtra = -1;
        }

        /// <summary>
        /// Einmal pro Frame von der Kammer. walk = läuft auf dem Band, pop = Auftauchen 0..1.
        /// </summary>
        public void Apply(AnimalStats s, AnimalState state, bool walk, float pop, float dt, float t, GameConfig cfg)
        {
            var v = cfg.animalVisuals;
            int a = s.VisualA(cfg), b = s.VisualB(cfg);
            bool omega = s.ultimate != null && s.ultimate.id == UltimateIds.Omega;
            bool koloss = s.ultimate != null && s.ultimate.id == UltimateIds.Koloss;

            float grow = 1f + v.growPerA * Mathf.Min(a, v.growCapA) + (koloss ? v.kolossGrowExtra : 0f);
            float inflTarget = (b >= 3 && !s.IsUltimate ? v.inflPerBOver2 * Mathf.Min(b - 2, v.inflBCap) : 0f) + v.inflPerA * Mathf.Min(a, v.growCapA);
            infl = Mathf.Lerp(infl, inflTarget, Mathf.Min(1f, dt * v.inflSmoothing));
            body.localScale = bodyBase * (1f + infl);

            if (kind == AnimalKind.Cow)
            {
                udderScale = Mathf.Lerp(udderScale, 1f + v.udderPerB * Mathf.Min(b, v.udderBCap), Mathf.Min(1f, dt * v.udderSmoothing));
                if (udder != null) udder.localScale = Vector3.one * udderScale;
            }
            int extra = kind == AnimalKind.Cow ? Mathf.Max(0, (b - 1) * 2) : Mathf.Max(0, b * 2 - 1);
            if (extra != shownExtra)
            {
                shownExtra = extra;
                for (int i = 0; i < extraUdders.Length; i++) extraUdders[i].SetActive(i < extra);
            }

            // Glühen: grün ab 40 % Instabilität, rot mit Hitze, golden bei Ω, voll bei INSTABIL
            bool alive = state == AnimalState.Ok || state == AnimalState.Unstable;
            float instab = alive ? 1f - s.stability / 100f : 0f;
            float glowTarget = state == AnimalState.Unstable ? 1f : omega ? v.glowOmega : Mathf.Max(0f, instab - v.glowFromInstab) * v.glowFactor;
            glow = Mathf.Lerp(glow, glowTarget, Mathf.Min(1f, dt * v.glowSmoothing));
            Pulse = v.pulseBase + v.pulseAmp * Mathf.Sin(t * (v.pulseFreq + s.heat * v.pulseHeatFreq + (state == AnimalState.Unstable ? v.pulseUnstableFreq : 0f)));
            GlowColor = omega
                ? Hex(v.goldColor)
                : Color.Lerp(Hex(v.glowColor), Hex(v.heatColor), Mathf.Max(s.heat / 100f, state == AnimalState.Unstable ? v.unstableHeatColorMix : 0f));
            Tint(bodyRenderers, null, GlowColor * (glow * v.emissiveFactor * Pulse * GlowBoost));
            if (kind == AnimalKind.Cow)
                Tint(spotRenderers, Color.Lerp(Hex(v.spotColor), omega ? Hex(v.goldColor) : Hex(v.spotGlowColor), glow * v.spotGlowFactor), null);

            // Größe mit Auftauchen (pop) und Basismaßstab je Tierart
            float baseScale = kind == AnimalKind.Cow ? 1f : v.henScale;
            transform.localScale = Vector3.one * (baseScale * grow * (0.2f + 0.8f * Ease(pop)));

            // Schweben: Wiegen; instabil: Zittern; kritisch: leichtes Zittern
            float fk = Mathf.Min(1f, s.floatHeight);
            float rollDeg = Mathf.Sin(t * v.floatRollFreq) * v.floatRollDeg * fk
                            + (state == AnimalState.Unstable ? Mathf.Sin(t * v.wobbleUnstableFreq) * v.wobbleUnstableDeg
                            : state == AnimalState.Ok && s.stability < v.lowStabWobbleBelow ? Mathf.Sin(t * v.wobbleLowStabFreq) * v.wobbleLowStabDeg : 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rollDeg);

            float walkK = walk ? 1f : 0f;
            for (int i = 0; i < legs.Length; i++)
            {
                float deg = Mathf.Sin(t * v.legFloatFreq + i) * v.legSwingFloatDeg * fk + Mathf.Sin(t * v.legWalkFreq + i * 2f) * v.legSwingWalkDeg * walkK;
                legs[i].localRotation = legBase[i] * Quaternion.Euler(0f, 0f, deg);
            }
            for (int i = 0; i < wings.Length; i++)
            {
                float freq = s.floatHeight > v.floatBobFrom ? v.wingFlapFast : v.wingFlapSlow;
                float deg = Mathf.Sin(t * freq) * v.wingFlapDeg * (i == 0 ? 1f : -1f);
                wings[i].localRotation = wingBase[i] * Quaternion.Euler(deg, 0f, 0f);
            }
        }

        void Tint(Renderer[] rs, Color? baseColor, Color? emission)
        {
            foreach (var r in rs)
            {
                r.GetPropertyBlock(mpb);
                if (baseColor.HasValue) mpb.SetColor(BaseColorId, baseColor.Value);
                if (emission.HasValue) mpb.SetColor(EmissionId, emission.Value);
                r.SetPropertyBlock(mpb);
            }
        }

        static float Ease(float k) => k < 0.5f ? 4f * k * k * k : 1f - Mathf.Pow(-2f * k + 2f, 3f) / 2f;

        static Color Hex(string h) => ColorUtility.TryParseHtmlString(h, out var c) ? c : Color.white;
    }
}
