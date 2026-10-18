using System;

namespace PlanErfuellt
{
    /// <summary>Staats-TV (GDD 11).</summary>
    [Serializable]
    public class NewsConfig
    {
        public float durationSec;
        public float levelDead;
        public float levelCrisis;
        public float levelWarn;
        /// <summary>Ab diesem Hunger ist der Sprecher verschwunden (Automatikbetrieb)</summary>
        public float anchorGoneFrom;
        /// <summary>Überspringen erst nach</summary>
        public float skipAfterSec;
        /// <summary>Höchstens so viele Taten im Lauftext</summary>
        public int factsMax;
        /// <summary>„PROGNOSE JAHR +n“ = Plan-Index × Wert</summary>
        public float prognosisYearsPerPlan;
        public float meatBarBase;
        public float meatBarPerPct;
        public float meatBarMin;
        public float meatBarMax;
        public float qualityBarBase;
        public float qualityBarPerPct;
        public float qualityBarMax;
        /// <summary>Lauftext in Pixeln pro Sekunde</summary>
        public float tickerSpeed;
    }
}
