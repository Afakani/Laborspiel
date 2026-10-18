using System;

namespace PlanErfuellt
{
    /// <summary>Laken über der Kammer: Animation und verräterische Bewegung (GDD 6.8, Prototyp clothK).</summary>
    [Serializable]
    public class ClothFxConfig
    {
        /// <summary>Annäherung pro s beim Werfen/Abnehmen</summary>
        public float speed;
        /// <summary>Wackeln je Mutationsgrad</summary>
        public float wobbleDegPerLevel;
        /// <summary>Frequenz = Basis + Grad</summary>
        public float wobbleBaseFreq;
        /// <summary>Zittern bei INSTABIL</summary>
        public float unstableDeg;
        public float unstableFreq;
        /// <summary>Goldenes Leuchten bei Ω</summary>
        public string omegaColor;
        public float omegaGlow;
        public float omegaPulse;
        public float omegaFreq;
    }
}
