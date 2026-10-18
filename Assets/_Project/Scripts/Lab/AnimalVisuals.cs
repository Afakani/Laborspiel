using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Sichtbare Mutation und Bewegung eines Tiers mit Skelett (GDD 5.5, Prototyp updateSubject): weiches Wachsen mit A,
    /// Aufblähen (Blendshape) mit A und B ≥ 3, Euter-Knochen wächst mit B, Zusatz-Euter wachsen weich heraus,
    /// Glühen bei Instabilität (rot bei Hitze, golden bei Ω). Dazu Atmen, Umschauen, Ohrenzucken, Schwanzpendeln,
    /// Laufen mit Knie, Beine baumeln beim Schweben, Zittern bei Instabilität, Flügelschlag und Kopfnicken beim Huhn.
    /// </summary>
    public class AnimalVisuals : MonoBehaviour
    {
        public const string InflateShape = "Inflate";
        public const string BreatheShape = "Breathe";
        /// <summary>Anteil, mit dem der Hals quer zu seiner Achse aufbläht (im Blendshape gebacken).</summary>
        public const float NeckSwell = 0.8f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] AnimalKind kind;
        [SerializeField] Transform spine;
        [SerializeField] Transform neck;
        [SerializeField] Transform head;
        [SerializeField] Transform[] ears = new Transform[0];
        [SerializeField] Transform[] tail = new Transform[0];
        [SerializeField] Transform[] legs = new Transform[0];
        [SerializeField] Transform[] shins = new Transform[0];
        [SerializeField] Transform udder;
        [SerializeField] GameObject[] extraUdders = new GameObject[0];
        [SerializeField] Transform[] wings = new Transform[0];
        [SerializeField] Transform[] neckWear = new Transform[0];
        [SerializeField] SkinnedMeshRenderer[] skins = new SkinnedMeshRenderer[0];
        [SerializeField] Renderer[] bodyRenderers = new Renderer[0];
        [SerializeField] Renderer[] spotRenderers = new Renderer[0];
        // Aufblähen des Rumpfs je Achse, wie im Blendshape gebacken (Builder)
        [SerializeField] Vector3 inflateAxis = Vector3.one;

        MaterialPropertyBlock mpb;
        Quaternion spineRot, neckRot, headRot;
        Vector3 headPos, udderPos, neckPos, tailPos;
        Quaternion[] legBase, shinBase, wingBase, earBase, tailBase;
        Vector3[] wingPos, extraPos, extraScale, wearScale;
        float[] extraK;
        int[] inflateIdx, breatheIdx;
        float infl, udderScale = 1f, glow, grow = -1f;

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
            spineRot = spine.localRotation;
            if (neck != null)
            {
                neckRot = neck.localRotation;
                neckPos = neck.localPosition;
            }
            if (tail.Length > 0) tailPos = tail[0].localPosition;
            if (head != null)
            {
                headRot = head.localRotation;
                headPos = head.localPosition;
            }
            if (udder != null) udderPos = udder.localPosition;
            legBase = Rotations(legs);
            shinBase = Rotations(shins);
            wingBase = Rotations(wings);
            earBase = Rotations(ears);
            tailBase = Rotations(tail);
            wingPos = new Vector3[wings.Length];
            for (int i = 0; i < wings.Length; i++) wingPos[i] = wings[i].localPosition;
            wearScale = new Vector3[neckWear.Length];
            for (int i = 0; i < neckWear.Length; i++) wearScale[i] = neckWear[i].localScale;
            extraPos = new Vector3[extraUdders.Length];
            extraScale = new Vector3[extraUdders.Length];
            extraK = new float[extraUdders.Length];
            for (int i = 0; i < extraUdders.Length; i++)
            {
                extraPos[i] = extraUdders[i].transform.localPosition;
                extraScale[i] = extraUdders[i].transform.localScale;
            }
            inflateIdx = new int[skins.Length];
            breatheIdx = new int[skins.Length];
            for (int i = 0; i < skins.Length; i++)
            {
                inflateIdx[i] = skins[i].sharedMesh.GetBlendShapeIndex(InflateShape);
                breatheIdx[i] = skins[i].sharedMesh.GetBlendShapeIndex(BreatheShape);
            }
        }

        static Quaternion[] Rotations(Transform[] ts)
        {
            var r = new Quaternion[ts.Length];
            for (int i = 0; i < ts.Length; i++) r[i] = ts[i].localRotation;
            return r;
        }

        /// <summary>Neues Tier: Übergänge zurücksetzen.</summary>
        public void ResetLook()
        {
            infl = 0f;
            udderScale = 1f;
            glow = 0f;
            grow = -1f;
            if (extraK == null) return;
            for (int i = 0; i < extraK.Length; i++)
            {
                extraK[i] = 0f;
                extraUdders[i].SetActive(false);
            }
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
            bool unstable = state == AnimalState.Unstable;
            bool lowStab = state == AnimalState.Ok && s.stability < v.lowStabWobbleBelow;

            // Weiches Wachsen (beim neuen Tier sofort)
            float growTarget = 1f + v.growPerA * Mathf.Min(a, v.growCapA) + (koloss ? v.kolossGrowExtra : 0f);
            grow = grow < 0f ? growTarget : Mathf.Lerp(grow, growTarget, Mathf.Min(1f, dt * v.growSmoothing));

            // Aufblähen und Atmen über Blendshapes; anhängende Teile rücken mit nach außen
            float inflTarget = (b >= 3 && !s.IsUltimate ? v.inflPerBOver2 * Mathf.Min(b - 2, v.inflBCap) : 0f) + v.inflPerA * Mathf.Min(a, v.growCapA);
            infl = Mathf.Lerp(infl, inflTarget, Mathf.Min(1f, dt * v.inflSmoothing));
            float breathe = (0.5f + 0.5f * Mathf.Sin(t * v.breatheFreq * (unstable ? v.breatheUnstableMult : 1f))) * v.breatheAmp;
            for (int i = 0; i < skins.Length; i++)
            {
                if (inflateIdx[i] >= 0) skins[i].SetBlendShapeWeight(inflateIdx[i], infl * 100f);
                if (breatheIdx[i] >= 0) skins[i].SetBlendShapeWeight(breatheIdx[i], breathe * 100f);
            }
            for (int i = 0; i < wings.Length; i++) wings[i].localPosition = Swell(wingPos[i]);
            if (neck != null) neck.localPosition = Swell(neckPos);
            if (tail.Length > 0) tail[0].localPosition = Swell(tailPos);
            for (int i = 0; i < neckWear.Length; i++) neckWear[i].localScale = wearScale[i] * (1f + NeckSwell * infl);

            if (udder != null)
            {
                udderScale = Mathf.Lerp(udderScale, 1f + v.udderPerB * Mathf.Min(b, v.udderBCap), Mathf.Min(1f, dt * v.udderSmoothing));
                udder.localScale = Vector3.one * udderScale;
                udder.localPosition = Swell(udderPos);
            }

            // Zusatz-Euter / Wucherungen wachsen weich heraus und schrumpfen wieder
            int extra = kind == AnimalKind.Cow ? Mathf.Max(0, (b - 1) * 2) : Mathf.Max(0, b * 2 - 1);
            for (int i = 0; i < extraUdders.Length; i++)
            {
                extraK[i] = Mathf.MoveTowards(extraK[i], i < extra ? 1f : 0f, dt * v.extraGrowSpeed);
                bool on = extraK[i] > 0.001f;
                if (extraUdders[i].activeSelf != on) extraUdders[i].SetActive(on);
                if (!on) continue;
                var tr = extraUdders[i].transform;
                tr.localScale = extraScale[i] * Ease(extraK[i]);
                tr.localPosition = Swell(extraPos[i]);
            }

            // Glühen: grün ab 40 % Instabilität, rot mit Hitze, golden bei Ω, voll bei INSTABIL
            bool alive = state == AnimalState.Ok || unstable;
            float instab = alive ? 1f - s.stability / 100f : 0f;
            float glowTarget = unstable ? 1f : omega ? v.glowOmega : Mathf.Max(0f, instab - v.glowFromInstab) * v.glowFactor;
            glow = Mathf.Lerp(glow, glowTarget, Mathf.Min(1f, dt * v.glowSmoothing));
            Pulse = v.pulseBase + v.pulseAmp * Mathf.Sin(t * (v.pulseFreq + s.heat * v.pulseHeatFreq + (unstable ? v.pulseUnstableFreq : 0f)));
            GlowColor = omega
                ? Hex(v.goldColor)
                : Color.Lerp(Hex(v.glowColor), Hex(v.heatColor), Mathf.Max(s.heat / 100f, unstable ? v.unstableHeatColorMix : 0f));
            Tint(bodyRenderers, null, GlowColor * (glow * v.emissiveFactor * Pulse * GlowBoost));
            if (kind == AnimalKind.Cow)
                Tint(spotRenderers, Color.Lerp(Hex(v.spotColor), omega ? Hex(v.goldColor) : Hex(v.spotGlowColor), glow * v.spotGlowFactor), null);

            // Größe mit Auftauchen (pop) und Basismaßstab je Tierart
            float baseScale = kind == AnimalKind.Cow ? 1f : v.henScale;
            transform.localScale = Vector3.one * (baseScale * grow * (0.2f + 0.8f * Ease(pop)));

            // Schweben: Wiegen; instabil: Wackeln; kritisch: leichtes Wackeln
            float fk = Mathf.Min(1f, s.floatHeight);
            float rollDeg = Mathf.Sin(t * v.floatRollFreq) * v.floatRollDeg * fk
                            + (unstable ? Mathf.Sin(t * v.wobbleUnstableFreq) * v.wobbleUnstableDeg
                            : lowStab ? Mathf.Sin(t * v.wobbleLowStabFreq) * v.wobbleLowStabDeg : 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, rollDeg);

            // Rumpf zittert bei Instabilität
            float shiver = unstable ? 1f : lowStab ? 0.5f : 0f;
            spine.localRotation = spineRot * Quaternion.Euler(Mathf.Sin(t * v.shiverFreq) * v.shiverDeg * shiver, 0f, Mathf.Sin(t * v.shiverFreq * 1.3f) * v.shiverDeg * shiver);

            // Beine: Schritt mit Knie beim Laufen, Baumeln beim Schweben
            float walkK = walk ? 1f : 0f;
            for (int i = 0; i < legs.Length; i++)
            {
                float phase = t * v.legWalkFreq + i * 2f;
                float deg = Mathf.Sin(t * v.legFloatFreq + i) * v.legSwingFloatDeg * fk + Mathf.Sin(phase) * v.legSwingWalkDeg * walkK;
                legs[i].localRotation = legBase[i] * Quaternion.Euler(0f, 0f, deg);
                if (i < shins.Length)
                {
                    float knee = Mathf.Max(0f, Mathf.Cos(phase)) * v.kneeBendDeg * walkK + Mathf.Sin(t * v.legFloatFreq + i + 1f) * v.legSwingFloatDeg * 0.5f * fk;
                    // Vorderbein knickt nach hinten, Hinterbein (Sprunggelenk) nach vorn
                    bool rear = legs[i].localPosition.x < 0f;
                    shins[i].localRotation = shinBase[i] * Quaternion.Euler(0f, 0f, rear ? knee : -knee);
                }
            }

            // Kopf: Umschauen und Nicken, beim Laufen im Schritt; Huhn ruckt den Kopf vor und zurück
            if (head != null)
            {
                float calm = 1f - 0.6f * walkK;
                float yaw = (Mathf.Sin(t * v.headLookFreq) + 0.4f * Mathf.Sin(t * v.headLookFreq * 2.3f + 1f)) * v.headLookDeg * calm;
                float nod = Mathf.Sin(t * v.headLookFreq * 1.7f + 0.5f) * v.headNodDeg + Mathf.Sin(t * v.legWalkFreq * 2f) * v.headWalkBobDeg * walkK;
                if (unstable) yaw += Mathf.Sin(t * v.shiverFreq * 0.7f) * v.shiverDeg * 2f;
                if (neck != null) neck.localRotation = neckRot * Quaternion.Euler(0f, yaw * 0.35f, nod * 0.3f);
                head.localRotation = headRot * Quaternion.Euler(0f, yaw * 0.65f, nod * 0.7f);
                if (kind == AnimalKind.Hen)
                    head.localPosition = headPos + new Vector3(Mathf.Sin(t * v.legWalkFreq) * v.henHeadBob * walkK, 0f, 0f);
            }

            // Ohren zucken ab und zu (links und rechts versetzt)
            for (int i = 0; i < ears.Length; i++)
            {
                float side = i == 0 ? 1f : -1f;
                float ph = Mathf.Repeat(t + i * v.earFlickEverySec * 0.45f, v.earFlickEverySec);
                float flick = ph < v.earFlickSec ? Mathf.Sin(ph / v.earFlickSec * Mathf.PI) * v.earFlickDeg : 0f;
                ears[i].localRotation = earBase[i] * Quaternion.Euler(flick * side, 0f, 0f);
            }

            // Schwanz pendelt als Kette mit Verzögerung
            for (int i = 0; i < tail.Length; i++)
            {
                float deg = Mathf.Sin(t * v.tailFreq * (1f + walkK) - i * 0.7f) * v.tailSwingDeg * (1f + i * 0.3f);
                tail[i].localRotation = tailBase[i] * Quaternion.Euler(deg, 0f, 0f);
            }

            for (int i = 0; i < wings.Length; i++)
            {
                float freq = s.floatHeight > v.floatBobFrom ? v.wingFlapFast : v.wingFlapSlow;
                float deg = Mathf.Sin(t * freq) * v.wingFlapDeg * (i == 0 ? 1f : -1f);
                wings[i].localRotation = wingBase[i] * Quaternion.Euler(deg, 0f, 0f);
            }
        }

        /// <summary>Lage eines am Rumpf hängenden Teils nach dem Aufblähen (gleiche Formel wie der Blendshape).</summary>
        Vector3 Swell(Vector3 localPos) => localPos + Vector3.Scale(localPos, inflateAxis) * infl;

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
