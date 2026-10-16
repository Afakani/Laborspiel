using System;

namespace PlanErfuellt
{
    /// <summary>Grammophon (GDD 6.5).</summary>
    [Serializable]
    public class GramophoneConfig
    {
        public float crankSec;
        public float musicSec;
        public float heatExtraDecay;
        public float directorCommentChance;
    }
}
