using System;

namespace PlanErfuellt
{
    /// <summary>Verkostungsautomat (GDD 6.6).</summary>
    [Serializable]
    public class TastingConfig
    {
        public int unlockPlanIdx;
        public float cost;
        public float qualityBonus;
        /// <summary>Qualität ab der ein Lob kommt</summary>
        public float hiFrom;
        /// <summary>Qualität für den mittleren Spruch</summary>
        public float midFrom;
    }
}
