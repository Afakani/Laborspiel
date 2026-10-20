using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Stimmung des Sprechers (NewsMood): Brauen, Mundform, Augen, Wangen, Schweißtropfen, Tränen, Taschentuch,
    /// Konfetti sowie Arm- und Kopfbewegung. NewsStudio verrechnet Arme und Kopf mit dem Zusammensacken.
    /// </summary>
    public class NewsAnchorMood : MonoBehaviour
    {
        [SerializeField] Transform browL, browR;
        [SerializeField] GameObject smile, bigSmile, frown, wavy;
        [SerializeField] Transform eyes;
        [SerializeField] GameObject cheeks;
        [SerializeField] Transform[] sweatDrops = new Transform[0];
        [SerializeField] Transform[] tears = new Transform[0];
        [SerializeField] GameObject tearStreaks;
        [SerializeField] GameObject hanky;
        [SerializeField] ParticleSystem confetti;
        [SerializeField] Transform paper;

        [Header("Bewegung")]
        [SerializeField] float browWorryDeg = 22f;
        [SerializeField] float browJoyLift = 0.035f;
        [SerializeField] float waveFreq = 7f;
        [SerializeField] float wipePeriod = 3.2f;
        [SerializeField] float sobFreq = 9f;
        [SerializeField] float dropFall = 0.35f;

        NewsMood mood;
        Vector3 browLPos, browRPos, eyesPos, eyesScale, paperPos;
        Vector3[] sweatPos, tearPos;
        bool init;

        /// <summary>Arm-Drehungen (relativ zur Ruhehaltung) und Kopf-Zusatz in Grad, jedes Bild neu.</summary>
        public Quaternion ArmL { get; private set; } = Quaternion.identity;
        public Quaternion ArmR { get; private set; } = Quaternion.identity;
        public Vector3 HeadEuler { get; private set; }
        public NewsMood Mood => mood;

        public void Setup(Transform bl, Transform br, GameObject smileGo, GameObject bigSmileGo, GameObject frownGo, GameObject wavyGo,
            Transform eyesTf, GameObject cheeksGo, Transform[] sweat, Transform[] tearDrops, GameObject streaks, GameObject hankyGo,
            ParticleSystem confettiPs, Transform paperTf)
        {
            browL = bl; browR = br; smile = smileGo; bigSmile = bigSmileGo; frown = frownGo; wavy = wavyGo; eyes = eyesTf;
            cheeks = cheeksGo; sweatDrops = sweat; tears = tearDrops; tearStreaks = streaks; hanky = hankyGo; confetti = confettiPs; paper = paperTf;
        }

        void Init()
        {
            if (init) return;
            init = true;
            browLPos = browL.localPosition;
            browRPos = browR.localPosition;
            eyesPos = eyes.localPosition;
            eyesScale = eyes.localScale;
            paperPos = paper.localPosition;
            sweatPos = new Vector3[sweatDrops.Length];
            for (int i = 0; i < sweatDrops.Length; i++) sweatPos[i] = sweatDrops[i].localPosition;
            tearPos = new Vector3[tears.Length];
            for (int i = 0; i < tears.Length; i++) tearPos[i] = tears[i].localPosition;
        }

        /// <summary>Stimmung für die Sendung setzen (Mundform, Wangen, Konfetti …).</summary>
        public void Begin(NewsMood m)
        {
            Init();
            mood = m;
            smile.SetActive(m == NewsMood.Calm);
            bigSmile.SetActive(m == NewsMood.Joy);
            frown.SetActive(m == NewsMood.Cry);
            wavy.SetActive(m == NewsMood.Sweat);
            cheeks.SetActive(m == NewsMood.Joy);
            hanky.SetActive(m == NewsMood.Cry);
            tearStreaks.SetActive(m == NewsMood.Cry);
            if (m == NewsMood.Joy) confetti.Play(true);
            else confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>Sendung vorbei: Konfetti weg.</summary>
        public void End()
        {
            if (confetti != null) confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>Ein Bild animieren; alive = 0 zusammengesackt … 1 lebendig.</summary>
        public void Tick(float t, float alive)
        {
            Init();
            bool show = alive > 0.5f;
            float worry = mood == NewsMood.Cry ? 1f : mood == NewsMood.Sweat ? 0.6f : 0f;
            float lift = mood == NewsMood.Joy ? browJoyLift * (1f + 0.4f * Mathf.Sin(t * 5f)) : 0f;
            browL.localPosition = browLPos + Vector3.up * lift;
            browR.localPosition = browRPos + Vector3.up * lift;
            browL.localEulerAngles = new Vector3(0f, 0f, worry * browWorryDeg);
            browR.localEulerAngles = new Vector3(0f, 0f, -worry * browWorryDeg);
            browL.gameObject.SetActive(show);
            browR.gameObject.SetActive(show);

            // Augen: Freude kneift zusammen, Schwitzen schaut hin und her
            eyes.localScale = mood == NewsMood.Joy ? Vector3.Scale(eyesScale, new Vector3(1f, 0.45f, 1f)) : eyesScale;
            eyes.localPosition = eyesPos + (mood == NewsMood.Sweat ? Vector3.right * (Mathf.Sin(t * 2.6f) * 0.03f) : Vector3.zero);

            // Tropfen: Schweiß an der Stirn, Tränen an den Augen
            bool sweaty = show && (mood == NewsMood.Sweat || mood == NewsMood.Cry);
            for (int i = 0; i < sweatDrops.Length; i++)
            {
                if (sweatDrops[i].gameObject.activeSelf != sweaty) sweatDrops[i].gameObject.SetActive(sweaty);
                float p = Mathf.Repeat(t * 0.8f + i * 0.37f, 1f);
                sweatDrops[i].localPosition = sweatPos[i] + Vector3.down * (dropFall * p);
            }
            bool crying = show && mood == NewsMood.Cry;
            for (int i = 0; i < tears.Length; i++)
            {
                if (tears[i].gameObject.activeSelf != crying) tears[i].gameObject.SetActive(crying);
                float p = Mathf.Repeat(t * 1.3f + i * 0.5f, 1f);
                tears[i].localPosition = tearPos[i] + new Vector3(0f, -dropFall * 0.8f * p, 0.04f * p);
            }

            // Arme und Kopf je Stimmung
            switch (mood)
            {
                case NewsMood.Joy:
                    ArmR = Quaternion.Euler(-20f, 0f, 135f + Mathf.Sin(t * waveFreq) * 20f);
                    ArmL = Quaternion.Euler(-65f + Mathf.Sin(t * 3f) * 8f, 0f, 0f);
                    HeadEuler = new Vector3(Mathf.Sin(t * 5f) * 5f, 0f, Mathf.Sin(t * 2.5f) * 6f);
                    break;
                case NewsMood.Sweat:
                {
                    // wischt sich regelmäßig die Stirn
                    float k = Mathf.Repeat(t / wipePeriod, 1f);
                    float wipe = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(k < 0.5f ? k * 4f : (1f - k) * 4f - 1f));
                    ArmR = Quaternion.Slerp(Quaternion.Euler(-50f, 0f, 0f), Quaternion.Euler(-150f, 0f, -35f + Mathf.Sin(t * 10f) * 8f), wipe);
                    ArmL = Quaternion.Euler(-50f + Mathf.Sin(t * 31f) * 2f, 0f, 0f);
                    HeadEuler = new Vector3(0f, Mathf.Sin(t * 2.3f) * 12f, 0f);
                    break;
                }
                case NewsMood.Cry:
                    ArmR = Quaternion.Euler(-138f + Mathf.Sin(t * sobFreq) * 4f, 0f, -32f);
                    ArmL = Quaternion.Euler(-50f, 0f, 0f);
                    HeadEuler = new Vector3(8f + Mathf.Abs(Mathf.Sin(t * sobFreq)) * 6f, 0f, Mathf.Sin(t * 1.7f) * 4f);
                    break;
                default:
                    // gefasst: raschelt mit dem Papier
                    float tap = Mathf.Max(0f, Mathf.Sin(t * 4f));
                    ArmL = Quaternion.Euler(-55f - tap * 10f, 0f, 0f);
                    ArmR = Quaternion.Euler(-55f - tap * 10f, 0f, 0f);
                    HeadEuler = Vector3.zero;
                    break;
            }
            paper.localPosition = paperPos + (mood == NewsMood.Calm ? Vector3.up * (Mathf.Max(0f, Mathf.Sin(t * 4f)) * 0.06f) : Vector3.zero);
        }
    }
}
