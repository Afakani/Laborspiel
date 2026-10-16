using System;

namespace PlanErfuellt
{
    /// <summary>Staats-TV (GDD 11).</summary>
    [Serializable]
    public class NewsConfig
    {
        public float durationSec;
        public float levelDead;
        public float levelCrisis;
        public float levelWarn;
    }
}
