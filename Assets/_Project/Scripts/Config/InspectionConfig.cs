using System;

namespace PlanErfuellt
{
    /// <summary>Kontrolle durch den Kommissar (GDD 9).</summary>
    [Serializable]
    public class InspectionConfig
    {
        public int fromDay;
        public float triggerAuthority;
        public float firstAfterDay2StartSec;
        public float gapSec;
        public float minGapSec;
        public float speed;
        public float range;
        public float suspMax;
        public SuspicionConfig susp = new SuspicionConfig();
        public InspectionMultConfig mult = new InspectionMultConfig();
        public VodkaConfig vodka = new VodkaConfig();
        public float prize;
        public float bossPrizeMult;
        public float bossAfterUltSec;
        /// <summary>So lange vorher wird der Kommissar angekündigt</summary>
        public float announceSec;
    }
}
