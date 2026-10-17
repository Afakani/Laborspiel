using System;

namespace PlanErfuellt
{
    /// <summary>Feuer bei der Vernichtung (Not-Stopp) statt Gas.</summary>
    [Serializable]
    public class ChamberFireConfig
    {
        /// <summary>Nachbrennen, nachdem das Tier weg ist</summary>
        public float afterSec;
        public float lightIntensity;
        public float flickerAmp;
        public float flickerFreq;
        public string lightColor;
        /// <summary>Farbe von Düsenlicht und Kegel während des Feuers</summary>
        public string nozzleColor;
        /// <summary>Ein- und Ausblenden von Licht und Düsenfarbe</summary>
        public float fadeSec;
    }
}
