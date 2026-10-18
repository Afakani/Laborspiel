using UnityEngine;
using UnityEngine.Rendering;

namespace PlanErfuellt
{
    /// <summary>
    /// Ultimativ-Höhepunkt (GDD 10, Prototyp ultCinematic): beim ersten Ultimativ dimmt das Licht auf
    /// cinematic.lightFactor, nur das Tier leuchtet, der Direktor ist sprachlos und meldet es dann als seine Erfindung.
    /// Danach Sondersendung. Bei weiteren Ultimativen nur ein Spruch.
    /// </summary>
    public class UltimateCinematic : MonoBehaviour
    {
        [SerializeField] Volume darkVolume;

        Chamber chamber;
        Director director;
        UltimateSerum current;
        float lineLeft;

        public void Setup(Volume volume) => darkVolume = volume;

        void Awake() => darkVolume.weight = 0f;

        void Start()
        {
            chamber = FindAnyObjectByType<Chamber>();
            director = FindAnyObjectByType<Director>();
        }

        void OnEnable() => EventBus.UltimateSprayed += OnUltimate;
        void OnDisable() => EventBus.UltimateSprayed -= OnUltimate;

        void OnUltimate(UltimateSerum u, bool first)
        {
            var gm = GameManager.Instance;
            if (!first)
            {
                director.SayAny("director.ult");
                return;
            }
            current = u;
            gm.State.cinematicLeft = gm.Config.cinematic.durationSec;
            lineLeft = gm.Config.cinematic.directorLineDelaySec;
            director.Say(gm.Texts.Get("director.speechless"));
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var c = gm.Config.cinematic;
            var s = gm.State;
            var dc = DayCycle.Instance;
            bool running = s.cinematicLeft > 0f;

            darkVolume.weight = Mathf.Lerp(darkVolume.weight, running ? 1f : 0f, Mathf.Min(1f, Time.unscaledDeltaTime * c.fadeSpeed));
            if (chamber != null) chamber.GlowBoost = Mathf.Lerp(1f, c.animalGlowBoost, darkVolume.weight);

            if (!running || dc == null || dc.Mode != DayMode.Play) return;
            float dt = gm.Clock.DeltaTime;
            if (lineLeft > 0f)
            {
                lineLeft -= dt;
                if (lineLeft <= 0f) director.SayAny("director.ult");
            }
            s.cinematicLeft -= dt;
            if (s.cinematicLeft > 0f) return;
            s.cinematicLeft = 0f;
            string name = current != null ? current.displayName : "";
            dc.ShowSpecialNews(gm.Texts.Format("newsFacts.specialHead", ("name", name)), c.specialNewsSec);
        }
    }
}
