using System;

namespace PlanErfuellt
{
    /// <summary>Warnzeichen vor Krisen (GDD 5.3).</summary>
    [Serializable]
    public class WarningsConfig
    {
        /// <summary>Hitze-Bereich [von, bis]</summary>
        public float[] heat = new float[0];
        /// <summary>Säure-Warnung ab</summary>
        public float acidT;
        /// <summary>Schwebe-Bereich [von, bis]</summary>
        public float[] @float = new float[0];
        /// <summary>Zittern unter</summary>
        public float stab;
    }
}
