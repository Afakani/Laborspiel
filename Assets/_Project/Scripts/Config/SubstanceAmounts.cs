using System;

namespace PlanErfuellt
{
    /// <summary>Menge je Grundstoff A/B/C.</summary>
    [Serializable]
    public class SubstanceAmounts
    {
        /// <summary>Wachstum A-17</summary>
        public int A;
        /// <summary>Laktation B-08</summary>
        public int B;
        /// <summary>Stabilität C-04</summary>
        public int C;
    }
}
