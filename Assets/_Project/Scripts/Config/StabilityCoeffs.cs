using System;

namespace PlanErfuellt
{
    /// <summary>Koeffizienten der Stabilitätsformel (GDD 4.3).</summary>
    [Serializable]
    public class StabilityCoeffs
    {
        public float @base;
        public float perA;
        public float perB;
        public float perAB;
        public float perC;
        /// <summary>Ab dieser Gesamtdosis gibt es Überlast-Abzug</summary>
        public int overloadFrom;
        /// <summary>Abzug pro Dosis über overloadFrom</summary>
        public float overloadPer;
    }
}
