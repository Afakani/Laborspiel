using System;

namespace PlanErfuellt
{
    /// <summary>Mischstation (GDD 4.2).</summary>
    [Serializable]
    public class MixingConfig
    {
        /// <summary>Maximale Dosis pro Regler</summary>
        public int sliderMax;
        /// <summary>Mischdauer</summary>
        public float mixTimeSec;
        /// <summary>Plätze im Serum-Regal</summary>
        public int rackSize;
    }
}
