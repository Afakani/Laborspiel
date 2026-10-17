using System;

namespace PlanErfuellt
{
    /// <summary>Kopfwippen und Neigen der Kamera beim Laufen.</summary>
    [Serializable]
    public class BobConfig
    {
        /// <summary>Schrittphase pro s beim Laufen</summary>
        public float phaseSpeed;
        public float ampY;
        public float ampX;
        public float breatheAmp;
        public float breatheFreq;
        /// <summary>Neigung pro m/s Seitwärtslaufen</summary>
        public float rollPerSpeedDeg;
        /// <summary>Ein-/Ausblenden des Wippens</summary>
        public float fadeSpeed;
        public float rollSmoothing;
    }
}
