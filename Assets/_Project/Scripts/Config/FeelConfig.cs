using System;

namespace PlanErfuellt
{
    /// <summary>Rückmeldung und Gefühl: Geld-Zahlen, Plan-erfüllt-Moment, wackelnder Fernseher, Tierangst.</summary>
    [Serializable]
    public class FeelConfig
    {
        /// <summary>Geld-Zahl höchstens so oft (kleine Beträge werden gesammelt)</summary>
        public float moneyIntervalSec;
        /// <summary>Kleinere Summen erst zusammen mit späteren zeigen</summary>
        public float moneyMin;
        /// <summary>So lange steigt eine Geld-Zahl auf</summary>
        public float moneyRiseSec;
        /// <summary>So weit steigt sie (Pixel)</summary>
        public float moneyRisePx;
        /// <summary>Direktor jubelt nach erfülltem Plan</summary>
        public float cheerSec;
        /// <summary>Fernseher wackelt bei voller Wut so weit (m)</summary>
        public float tvShakeM;
        /// <summary>… und dreht so weit (Grad)</summary>
        public float tvShakeDeg;
        /// <summary>Wackeln pro Sekunde</summary>
        public float tvShakeFreq;
        /// <summary>Tier klagt unter dieser Stabilität</summary>
        public float distressBelowStab;
        /// <summary>Abstand der Angstlaute</summary>
        public float distressIntervalSec;
        /// <summary>Tonhöhe der Angstlaute (Faktor)</summary>
        public float distressPitch;
    }
}
