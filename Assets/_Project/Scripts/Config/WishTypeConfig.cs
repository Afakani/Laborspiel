using System;

namespace PlanErfuellt
{
    /// <summary>Ein Sonderwunsch-Typ (GDD 7.3).</summary>
    [Serializable]
    public class WishTypeConfig
    {
        public string id;
        /// <summary>Menge in t (falls benötigt)</summary>
        public LinearValue amount = new LinearValue();
        public float minQuality;
        public float durationSec;
    }
}
