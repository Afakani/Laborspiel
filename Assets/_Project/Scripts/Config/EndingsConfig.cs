using System;

namespace PlanErfuellt
{
    /// <summary>Schwellen für die Spielenden (GDD 13).</summary>
    [Serializable]
    public class EndingsConfig
    {
        public float heroMinPop;
        public int heroMaxFailedDays;
        public float deadBelowPop;
        /// <summary>So viele verfehlte Pläne in Folge: ab nach Sibirien (D162)</summary>
        public int siberiaFailStreak;
    }
}
