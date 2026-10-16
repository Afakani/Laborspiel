using System;

namespace PlanErfuellt
{
    /// <summary>Koeffizienten der Qualitätsformel (GDD 4.3).</summary>
    [Serializable]
    public class QualityCoeffs
    {
        public float @base;
        public float perA;
        public float perB;
        public float perC;
    }
}
