using System;

namespace PlanErfuellt
{
    /// <summary>Risiken am Tier (GDD 5.2).</summary>
    [Serializable]
    public class RisksConfig
    {
        /// <summary>Risiken nur unter dieser Basis-Stabilität</summary>
        public float activeBelowStab;
        public int heatMinA;
        public float heatPerA;
        public float heatDecay;
        public float heatCrisis;
        public float heatUnstable;
        public float heatStabPenalty;
        public int acidMinB;
        public float acidThreshold;
        public int floatMinB;
        public float floatPerB;
        public int floatFromB;
        public float floatDecay;
        public float floatCrisis;
        public float floatEscape;
        public float escapeCost;
        public float leashTo;
        public int beltBreakMinA;
        public float beltBreakChancePerAPerSec;
        public float cableTearBelowStab;
        public float cableTearChancePerSec;
        public float blackoutSec;
        public float blackoutProductionFactor;
        public float unstableCountdownSec;
        public float infectionDistance;
        public float infectionChance;
        public int infectionFromDay;
        public int infectionDays;
    }
}
