using System;

namespace PlanErfuellt
{
    /// <summary>Rekorde (GDD 12).</summary>
    [Serializable]
    public class RecordsConfig
    {
        /// <summary>Neuer Rekord erst ab altem Wert + epsilon</summary>
        public float epsilon;
        /// <summary>Direktor meldet den Rekord</summary>
        public float directorChance;
    }
}
