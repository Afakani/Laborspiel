using System;

namespace PlanErfuellt
{
    /// <summary>Verarbeitung und Mut-Bonus (GDD 5.6).</summary>
    [Serializable]
    public class ProcessingConfig
    {
        public float braveMinStab;
        public float braveBonus;
        public float braveExplodeChance;
        public float kolossBeltBreakChance;
    }
}
