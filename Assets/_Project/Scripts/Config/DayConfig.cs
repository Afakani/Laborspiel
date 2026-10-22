using System;

namespace PlanErfuellt
{
    /// <summary>Tagesablauf, Quoten und Bezahlung (GDD 7.1).</summary>
    [Serializable]
    public class DayConfig
    {
        /// <summary>Anzahl Pläne bis Spielende</summary>
        public int plans;
        /// <summary>Schichtlänge in Sekunden</summary>
        public float dayLengthSec;
        /// <summary>Tagesziel je Plan</summary>
        public GoodsValues[] targets = new GoodsValues[0];
        /// <summary>Prämie je Plan</summary>
        public float[] prizes = new float[0];
        /// <summary>SM pro Tonne</summary>
        public GoodsValues payPerTon = new GoodsValues();
        /// <summary>Aufschlag über Soll (0.5 = +50 %)</summary>
        public float overfulfilPayBonus;
        /// <summary>Restzeit, ab der der Bildschirmrand pulsiert</summary>
        public float lateWarningSec;
        /// <summary>Restzeit, ab der eine Warnmeldung kommt (Plan noch offen)</summary>
        public float lateNoticeSec;
        /// <summary>Lieferung ohne Bestellung (A, B, C, Gegenmittel, Wodka, Kaviar, Laken)</summary>
        public int[] standardOrder = new int[0];
    }
}
