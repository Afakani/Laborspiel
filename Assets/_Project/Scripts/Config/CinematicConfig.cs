using System;

namespace PlanErfuellt
{
    /// <summary>Ultimativ-Höhepunkt (GDD 10, Prototyp ultCinematic).</summary>
    [Serializable]
    public class CinematicConfig
    {
        /// <summary>Licht gedimmt, Tier leuchtet</summary>
        public float durationSec;
        /// <summary>Bildhelligkeit während der Sequenz</summary>
        public float lightFactor;
        /// <summary>„… … …“, dann der Spruch</summary>
        public float directorLineDelaySec;
        /// <summary>Tier leuchtet heller, damit es im Dunkeln strahlt</summary>
        public float animalGlowBoost;
        /// <summary>Abdunkeln/Aufhellen pro s</summary>
        public float fadeSpeed;
    }
}
