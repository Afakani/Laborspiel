using System;

namespace PlanErfuellt
{
    /// <summary>Koeffizienten für Ertrag bzw. Fleisch (pro gedämpfter Dosis).</summary>
    [Serializable]
    public class YieldCoeffs
    {
        public float perA;
        public float perB;
    }
}
