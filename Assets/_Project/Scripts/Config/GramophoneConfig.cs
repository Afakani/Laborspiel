using System;

namespace PlanErfuellt
{
    /// <summary>Grammophon (GDD 6.5).</summary>
    [Serializable]
    public class GramophoneConfig
    {
        public float crankSec;
        public float musicSec;
        public float directorCommentChance;
        /// <summary>Abstand der Propaganda-Durchsagen</summary>
        public float lineEverySec;
    }
}
