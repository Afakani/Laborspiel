using System;

namespace PlanErfuellt
{
    /// <summary>Zentrifuge (GDD 6.3).</summary>
    [Serializable]
    public class CentrifugeConfig
    {
        public int unlockPlanIdx;
        public float rpmAccelBase;
        public float rpmAccelPerRpm;
        public CentrifugeZones zones = new CentrifugeZones();
        public float concYellow;
        public float concGreen;
        public float spinDown;
        public float shakeAbove;
    }
}
