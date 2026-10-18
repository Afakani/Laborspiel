using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Fenster zur Welt (GDD 7.7, Prototyp winTex/updateCrowd): bei Hunger eine Warteschlange vor dem leeren
    /// Milchladen, bei satten Bürgern ein Festumzug mit Fahnen, Konfetti und Banner, sonst nur die Einöde.
    /// Schwellen aus world.windowHunger/windowParade; die Figuren wippen leicht.
    /// </summary>
    public class WindowView : MonoBehaviour
    {
        [SerializeField] GameObject hunger;
        [SerializeField] GameObject parade;
        [SerializeField] Transform[] figures = new Transform[0];
        [SerializeField] ParticleSystem confetti;
        [SerializeField] float refreshSec = 1f;
        [SerializeField] float bobAmp = 0.04f;

        Vector3[] basePos;
        float timer;
        int mode = -1;

        public void Setup(GameObject hungerGroup, GameObject paradeGroup, Transform[] figs, ParticleSystem confettiFx)
        {
            hunger = hungerGroup;
            parade = paradeGroup;
            figures = figs;
            confetti = confettiFx;
        }

        void Awake()
        {
            basePos = new Vector3[figures.Length];
            for (int i = 0; i < figures.Length; i++) basePos[i] = figures[i].localPosition;
            hunger.SetActive(false);
            parade.SetActive(false);
        }

        /// <summary>0 = still, 1 = Hunger, 2 = Festumzug.</summary>
        public static int Mode(WorldValues w, GameConfig cfg)
        {
            var c = cfg.world;
            if (w.pop < c.windowHunger.popBelow || w.food < c.windowHunger.foodBelow) return 1;
            if (w.pop >= c.windowParade.popMin && w.food >= c.windowParade.foodMin) return 2;
            return 0;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            timer -= Time.unscaledDeltaTime;
            if (timer <= 0f)
            {
                timer = refreshSec;
                int m = Mode(WorldValues.Compute(gm.State, gm.Config), gm.Config);
                if (m != mode)
                {
                    mode = m;
                    hunger.SetActive(m == 1);
                    parade.SetActive(m == 2);
                    if (m == 2) confetti.Play();
                    else confetti.Stop();
                }
            }
            // Leichtes Wippen (Warten bzw. Marschieren)
            float t = Time.time;
            for (int i = 0; i < figures.Length; i++)
            {
                if (!figures[i].gameObject.activeInHierarchy) continue;
                float speed = mode == 2 ? 6f : 1.3f;
                figures[i].localPosition = basePos[i] + Vector3.up * Mathf.Abs(Mathf.Sin(t * speed + i * 0.7f)) * bobAmp;
            }
        }
    }
}
