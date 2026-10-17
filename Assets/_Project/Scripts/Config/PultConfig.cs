using System;

namespace PlanErfuellt
{
    /// <summary>Pult: Reinigung, Hebelanimation, Statuslampen (GDD 6.1).</summary>
    [Serializable]
    public class PultConfig
    {
        /// <summary>Dauer der Kammerreinigung</summary>
        public float cleanSec;
        public float leverPullSec;
        public float leverPullDeg;
        /// <summary>Hitze, ab der die Temperaturlampe gelb wird</summary>
        public float statusTempWarn;
        /// <summary>Stabilität, unter der die Vitallampe rot wird</summary>
        public float statusVitBad;
        /// <summary>Auffrischtakt offener Panels</summary>
        public float panelRefreshSec;
        /// <summary>Anleinen erst ab dieser Schwebehöhe möglich</summary>
        public float leashMinFloat;
    }
}
