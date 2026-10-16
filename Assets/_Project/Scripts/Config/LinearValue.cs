using System;

namespace PlanErfuellt
{
    /// <summary>Wert, der mit dem Plan-Index wächst: base + perPlan * planIdx.</summary>
    [Serializable]
    public class LinearValue
    {
        /// <summary>Grundwert</summary>
        public float @base;
        /// <summary>Zuwachs pro Plan-Index</summary>
        public float perPlan;
    }
}
