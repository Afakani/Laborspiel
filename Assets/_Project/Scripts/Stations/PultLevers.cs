using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kipphebel am 3D-Pult (GAS, KLON, FÖRDERBAND): werden beim Ziehen kurz nach vorn gelegt (Prototyp pullLever).
    /// </summary>
    public class PultLevers : MonoBehaviour
    {
        [SerializeField] Transform[] levers = new Transform[3];

        float[] timers;
        Quaternion[] baseRot;

        void Awake()
        {
            timers = new float[levers.Length];
            baseRot = new Quaternion[levers.Length];
            for (int i = 0; i < levers.Length; i++) baseRot[i] = levers[i].localRotation;
        }

        void OnEnable() => EventBus.LeverPulled += Pull;
        void OnDisable() => EventBus.LeverPulled -= Pull;

        void Pull(int i)
        {
            var gm = GameManager.Instance;
            if (gm != null && i >= 0 && i < timers.Length) timers[i] = gm.Config.pult.leverPullSec;
        }

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var p = gm.Config.pult;
            float dt = gm.Clock.DeltaTime;
            for (int i = 0; i < levers.Length; i++)
            {
                if (timers[i] <= 0f) continue;
                timers[i] = Mathf.Max(0f, timers[i] - dt);
                // Halbe Sinuswelle: nach vorn (zum Spieler, −X-Drehung) und zurück
                float a = timers[i] > 0f ? p.leverPullDeg * Mathf.Sin(timers[i] / p.leverPullSec * Mathf.PI) : 0f;
                levers[i].localRotation = baseRot[i] * Quaternion.Euler(-a, 0f, 0f);
            }
        }
    }
}
