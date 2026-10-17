using System;

namespace PlanErfuellt
{
    /// <summary>Zielwahl und Haltezeiten (GDD 2, Prototyp pickTarget).</summary>
    [Serializable]
    public class InteractionConfig
    {
        /// <summary>Wertung, wenn der Spieler in der Pult-Zone steht</summary>
        public float pultScore;
        public float clothScore;
        /// <summary>Haltezeit bei nicht möglichen Aktionen (nur Anzeige)</summary>
        public float disabledHoldSec;
        /// <summary>Bereich vor der Mischstation (Prototyp-Koordinaten)</summary>
        public AreaLayout mixingZone = new AreaLayout();
        public HoldTimesConfig hold = new HoldTimesConfig();
    }
}
