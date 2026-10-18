using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Spielzeit-Takt. Alle Spielsysteme nutzen DeltaTime von hier statt Time.deltaTime,
    /// damit Pause und Zeitraffer überall gleich wirken.
    /// </summary>
    public class GameClock
    {
        readonly float maxStep;

        /// <summary>Spielzeit seit Start in Sekunden.</summary>
        public float Time { get; private set; }
        /// <summary>Spielzeit-Schritt dieses Frames (0 bei Pause).</summary>
        public float DeltaTime { get; private set; }
        public bool Paused { get; private set; }
        /// <summary>Angehalten durch ein Overlay (Bestellterminal, Intro, Spielende) – unabhängig von der Pause-Taste.</summary>
        public bool Held { get; private set; }
        /// <summary>Zeitraffer-Faktor (1 = normal).</summary>
        public float TimeScale { get; private set; } = 1f;

        /// <param name="maxStep">Größter erlaubter Echtzeit-Schritt, damit Ruckler keine Sprünge erzeugen.</param>
        public GameClock(float maxStep)
        {
            this.maxStep = maxStep;
        }

        /// <summary>Einmal pro Frame mit der echten Frame-Zeit aufrufen.</summary>
        public void Tick(float unscaledDeltaTime)
        {
            DeltaTime = Paused || Held ? 0f : Mathf.Min(unscaledDeltaTime, maxStep) * TimeScale;
            Time += DeltaTime;
        }

        public void SetPaused(bool paused)
        {
            if (Paused == paused) return;
            Paused = paused;
            EventBus.RaisePauseChanged(paused);
        }

        public void TogglePause() => SetPaused(!Paused);

        public void SetHeld(bool held) => Held = held;

        public void SetTimeScale(float scale)
        {
            scale = Mathf.Max(0f, scale);
            if (Mathf.Approximately(TimeScale, scale)) return;
            TimeScale = scale;
            EventBus.RaiseTimeScaleChanged(scale);
        }
    }
}
