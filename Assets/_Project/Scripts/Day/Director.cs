using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Direktor im Fernseher (GDD 7.2): spricht über Untertitel (EventBus.DirectorSpeak), Sprüche je Wut-Stufe aus
    /// director.* in der Text-JSON, zufällige Rufe (je wütender, desto öfter), Reaktionen auf Explosion,
    /// Instabilität und NOT-STOPP. Läuft nur während einer Schicht.
    /// </summary>
    public class Director : MonoBehaviour
    {
        float shoutTimer;

        GameManager GM => GameManager.Instance;

        /// <summary>Darf der Direktor gerade spontan rufen (Schicht läuft)?</summary>
        public bool Active { get; set; }

        void OnEnable()
        {
            EventBus.Risk += OnRisk;
            EventBus.AnimalStateChanged += OnAnimalState;
        }

        void OnDisable()
        {
            EventBus.Risk -= OnRisk;
            EventBus.AnimalStateChanged -= OnAnimalState;
        }

        void Start() => ResetTimer(true);

        public void ResetTimer(bool start)
        {
            var a = GM.Config.anger;
            shoutTimer = start ? a.startShoutDelaySec : NextInterval();
        }

        float NextInterval()
        {
            var a = GM.Config.anger;
            return Mathf.Lerp(a.shoutIntervalSec[1], a.shoutIntervalSec[0], GM.State.anger / a.max) + Random.value * a.shoutJitterSec;
        }

        /// <summary>Text direkt sagen.</summary>
        public void Say(string text)
        {
            if (!string.IsNullOrEmpty(text)) EventBus.RaiseDirectorSpeak(text);
        }

        /// <summary>Spruch je Wut-Stufe aus einer Liste von Listen (z. B. director.met).</summary>
        public void SayTier(string path, params (string key, object value)[] values) =>
            Say(TextDatabase.Fill(GM.Texts.PickByAnger(path, GM.State.anger), values));

        /// <summary>Zufälliger Spruch aus einer einfachen Liste (z. B. director.unstable).</summary>
        public void SayAny(string path) => Say(GM.Texts.Pick(path));

        void Update()
        {
            if (!Active || GM == null) return;
            float dt = GM.Clock.DeltaTime;
            if (dt <= 0f) return;
            shoutTimer -= dt;
            if (shoutTimer > 0f) return;
            SayTier("director.shouts");
            shoutTimer = NextInterval();
        }

        void OnRisk(RiskEvent e)
        {
            if (!Active) return;
            if (e == RiskEvent.Unstable) SayAny("director.unstable");
            else if (e == RiskEvent.Explosion || e == RiskEvent.ProcessingExplosion) SayTier("director.boom");
        }

        void OnAnimalState(AnimalState s)
        {
            if (!Active || s != AnimalState.Destroy) return;
            if (Random.value < GM.Config.anger.stopShoutChance) SayAny("director.stop");
        }
    }
}
