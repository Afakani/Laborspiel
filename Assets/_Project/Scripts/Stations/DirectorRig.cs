using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Direktor im Fernseher aus Ebenen (Tools/gen_director.py): Mund bewegt sich beim Sprechen (brüllt, wenn er
    /// wütend ist), blinzelt, Brauen senken sich und das Gesicht läuft mit der Wut rot an; ab der ersten Wut-Stufe
    /// schwitzt er, ab der zweiten pocht die Zornesader und der Kopf zittert. Nach erfülltem Plan jubelt er (Cheer):
    /// Mund offen, Brauen hoch, Kopf wippt.
    /// </summary>
    public class DirectorRig : MonoBehaviour
    {
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Renderer[] renderers;
        [SerializeField] Renderer[] skin;
        [SerializeField] Transform head;
        [SerializeField] Transform browL, browR;
        [SerializeField] Renderer mouth;
        [SerializeField] Material[] mouthFrames;
        [SerializeField] GameObject lids;
        [SerializeField] Transform[] sweat;
        [SerializeField] Transform vein;

        [Header("Animation")]
        [SerializeField] Color flush = new Color(1f, 0.5f, 0.45f);
        [SerializeField] float browMaxDeg = 18f;
        [SerializeField] float browFriendlyDeg = -6f;
        [SerializeField] float browDrop = 0.06f;
        [SerializeField] float blinkMin = 2f, blinkMax = 5f, blinkDur = 0.13f;
        [SerializeField] float mouthRate = 9f;
        [SerializeField] float headSwayDeg = 1.5f, headTalkDeg = 2.5f, headRageDeg = 3f;
        [SerializeField] float sweatFall = 0.35f, sweatPeriod = 1.4f;
        [SerializeField] float veinPulse = 0.18f;
        [SerializeField] float flushAmount = 0.8f;
        [Tooltip("Leuchtkraft der Gesichtsebenen relativ zum Bildschirm (sonst überstrahlt die helle Haut)")]
        [SerializeField] float layerEmission = 0.6f;

        MaterialPropertyBlock mpb;
        Vector3 browLPos, browRPos, veinScale;
        Vector3[] sweatPos, sweatScale;
        float blinkAt, blinkLeft, speakLeft, cheerLeft, emission = 1.6f;
        int mouthFrame = -1;

        /// <summary>Teile setzen (Editor-Aufbau).</summary>
        public void Setup(Renderer[] all, Renderer[] skinParts, Transform headGroup, Transform left, Transform right,
            Renderer mouthRenderer, Material[] frames, GameObject lidsGo, Transform[] sweatDrops, Transform veinTf)
        {
            renderers = all;
            skin = skinParts;
            head = headGroup;
            browL = left;
            browR = right;
            mouth = mouthRenderer;
            mouthFrames = frames;
            lids = lidsGo;
            sweat = sweatDrops;
            vein = veinTf;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            browLPos = browL.localPosition;
            browRPos = browR.localPosition;
            veinScale = vein.localScale;
            sweatPos = new Vector3[sweat.Length];
            sweatScale = new Vector3[sweat.Length];
            for (int i = 0; i < sweat.Length; i++)
            {
                sweatPos[i] = sweat[i].localPosition;
                sweatScale[i] = sweat[i].localScale;
            }
            blinkAt = Random.Range(blinkMin, blinkMax);
            lids.SetActive(false);
        }

        /// <summary>Er spricht für die angegebene Zeit (Untertitel/Parole).</summary>
        public void Speak(float seconds) => speakLeft = Mathf.Max(speakLeft, seconds);

        /// <summary>Er jubelt für die angegebene Zeit (Plan erfüllt).</summary>
        public void Cheer(float seconds) => cheerLeft = Mathf.Max(cheerLeft, seconds);

        /// <summary>Helligkeit des Bildschirms (Flackern, Pulsieren aus TvDirectorFx).</summary>
        public void SetEmission(float e) => emission = e;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float t = Time.unscaledTime, dt = Time.unscaledDeltaTime;
            var anger = gm.Config.anger;
            float a = Mathf.Clamp01(gm.State.anger / Mathf.Max(1f, anger.max));
            bool annoyed = gm.State.anger >= anger.tiers[0];
            bool rage = gm.State.anger >= anger.tiers[1];
            bool speaking = speakLeft > 0f;
            if (speaking) speakLeft -= dt;
            bool cheer = cheerLeft > 0f;
            if (cheer) cheerLeft -= dt;

            // Blinzeln
            blinkAt -= dt;
            if (blinkAt <= 0f)
            {
                blinkLeft = blinkDur;
                blinkAt = Random.Range(blinkMin, blinkMax) * (rage ? 0.5f : 1f);
            }
            if (blinkLeft > 0f) blinkLeft -= dt;
            bool closed = blinkLeft > 0f;
            if (lids.activeSelf != closed) lids.SetActive(closed);

            // Mund: beim Sprechen auf und zu (wütend: brüllen), sonst zufrieden oder streng
            int frame;
            if (speaking)
            {
                float n = Mathf.PerlinNoise(t * mouthRate, 0.3f);
                frame = n < 0.35f ? (annoyed ? 1 : 0) : (n > 0.7f && annoyed ? 3 : 2);
            }
            else frame = annoyed ? 1 : 0;
            // Jubel: lachender offener Mund
            if (cheer) frame = Mathf.PerlinNoise(t * 4f, 0.8f) < 0.25f ? 0 : 2;
            if (frame != mouthFrame && frame < mouthFrames.Length)
            {
                mouthFrame = frame;
                mouth.sharedMaterial = mouthFrames[frame];
            }

            // Brauen: freundlich leicht gehoben, mit der Wut innen tief; beim Sprechen wippen sie
            float deg = cheer ? browFriendlyDeg * 2f : Mathf.Lerp(browFriendlyDeg, browMaxDeg, a);
            float lift = cheer ? browDrop * (0.6f + 0.4f * Mathf.Sin(t * 8f)) : speaking ? Mathf.Sin(t * 6f) * browDrop * 0.3f : 0f;
            var drop = Vector3.down * (browDrop * a) + Vector3.up * lift;
            browL.localPosition = browLPos + drop;
            browR.localPosition = browRPos + drop;
            browL.localEulerAngles = new Vector3(0f, 0f, -deg);
            browR.localEulerAngles = new Vector3(0f, 0f, deg);

            // Kopf: ruhiges Wiegen, beim Sprechen Nicken, bei Raserei Zittern
            float z = Mathf.Sin(t * 1.3f) * headSwayDeg;
            if (speaking) z += Mathf.Sin(t * 7f) * headTalkDeg;
            if (rage && !cheer) z += (Mathf.PerlinNoise(t * 18f, 1.7f) - 0.5f) * 2f * headRageDeg;
            if (cheer) z += Mathf.Sin(t * 10f) * headTalkDeg * 1.5f;
            head.localEulerAngles = new Vector3(0f, 0f, z);

            // Schweißtropfen laufen herunter
            for (int i = 0; i < sweat.Length; i++)
            {
                bool on = annoyed;
                if (sweat[i].gameObject.activeSelf != on) sweat[i].gameObject.SetActive(on);
                if (!on) continue;
                float p = Mathf.Repeat(t / sweatPeriod + i * 0.5f, 1f);
                sweat[i].localPosition = sweatPos[i] + Vector3.down * (sweatFall * p);
                sweat[i].localScale = sweatScale[i] * (1f - p * 0.4f);
            }
            if (vein.gameObject.activeSelf != rage) vein.gameObject.SetActive(rage);
            if (rage) vein.localScale = veinScale * (1f + veinPulse * Mathf.Abs(Mathf.Sin(t * 8f)));

            // Farbe: Gesicht läuft rot an; alle Ebenen folgen der Bildschirmhelligkeit
            var face = Color.Lerp(Color.white, flush, cheer ? 0f : a * a * flushAmount);
            foreach (var r in renderers)
            {
                bool isSkin = System.Array.IndexOf(skin, r) >= 0;
                var c = isSkin ? face : Color.white;
                r.GetPropertyBlock(mpb);
                mpb.SetColor(BaseColorId, c);
                mpb.SetColor(EmissionId, c * (emission * (r == renderers[0] ? 1f : layerEmission)));
                r.SetPropertyBlock(mpb);
            }
        }
    }
}
