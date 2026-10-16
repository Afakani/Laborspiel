using System;

namespace PlanErfuellt
{
    /// <summary>Schwellen für Kombinationsnamen und Vor-/Nachteile (GDD 4.4).</summary>
    [Serializable]
    public class ComboConfig
    {
        /// <summary>Zusatz (extrem) ab A+B</summary>
        public int extremeFromAB;
        /// <summary>Zusatz (stabilisiert) braucht mindestens so viel C</summary>
        public int stabilizedMinC;
        /// <summary>... und C >= ratio*(A+B)</summary>
        public float stabilizedRatio;
        /// <summary>Vorteil wird ab diesem Faktor angezeigt</summary>
        public float showEffectAboveFactor;
        public int heatWarnMinA;
        public int acidWarnMinB;
        public int floatWarnMinB;
        public int beltWarnMinA;
    }
}
