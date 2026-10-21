using System;

namespace PlanErfuellt
{
    /// <summary>Alle Zahlen der Serum-Formeln (GDD 4.3).</summary>
    [Serializable]
    public class FormulaConfig
    {
        /// <summary>Dämpfung e = 1/(1+dampPerC*C)</summary>
        public float dampPerC;
        public StabilityCoeffs stability = new StabilityCoeffs();
        public QualityCoeffs quality = new QualityCoeffs();
        /// <summary>Milch/Eier</summary>
        public YieldCoeffs rate = new YieldCoeffs();
        /// <summary>Fleisch</summary>
        public YieldCoeffs meat = new YieldCoeffs();
        /// <summary>Eier: Wirkung von B beim Huhn (statt rate.perB)</summary>
        public float ratePerBHen;
        /// <summary>Obergrenze für Stabilität und Qualität</summary>
        public float statMax;
        /// <summary>+Stabilität pro tiefgekühltem Serum</summary>
        public float frozenStabBonus;
        /// <summary>+Stabilität während Musik</summary>
        public float musicStabBonus;
        /// <summary>+Qualität durch Siegel</summary>
        public float stampQualityBonus;
        public AcidConfig acid = new AcidConfig();
    }
}
