using System;

namespace PlanErfuellt
{
    /// <summary>Mischstation (GDD 4.2).</summary>
    [Serializable]
    public class MixingConfig
    {
        /// <summary>Maximale Dosis pro Regler (auch: voller Tank in der Anzeige)</summary>
        public int sliderMax;
        /// <summary>Mischdauer</summary>
        public float mixTimeSec;
        /// <summary>Plätze im Serum-Regal</summary>
        public int rackSize;
        /// <summary>Rührer-Drehung pro s beim Mischen</summary>
        public float stirSpeedDeg;
        /// <summary>Farbe zentrifugierter Rohseren</summary>
        public string centrifugedColor;
    }
}
