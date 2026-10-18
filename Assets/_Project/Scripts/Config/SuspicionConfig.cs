using System;

namespace PlanErfuellt
{
    /// <summary>Verdacht pro Sekunde während der Kontrolle (GDD 9).</summary>
    [Serializable]
    public class SuspicionConfig
    {
        public float uncovered;
        public float uncoveredKoloss;
        public float coveredBase;
        public float coveredPerDose;
        public float coveredMaxDose;
        public float coveredUnstable;
        public float coveredOmega;
        public float coveredKoloss;
        public float contaminated;
        public float rack;
        public float rackRange;
        public int rackSuspiciousAB;
        /// <summary>Mutationsgrad eines Ultimativs (Laken-Verdacht und Wackeln)</summary>
        public float ultLevel;
    }
}
