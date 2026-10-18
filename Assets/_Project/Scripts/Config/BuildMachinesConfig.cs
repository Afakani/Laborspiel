using System;

namespace PlanErfuellt
{
    /// <summary>Baumodus allgemein (GDD 6.10). Maschinentypen liegen als eigene Assets vor.</summary>
    [Serializable]
    public class BuildMachinesConfig
    {
        /// <summary>Preisfaktor pro gleichem Typ</summary>
        public float priceFactor;
        public BuildGridConfig grid = new BuildGridConfig();
        /// <summary>Anteil des Kaufpreises beim Abriss</summary>
        public float sellRefund;
        /// <summary>Auto-Sprüher: neuer Versuch nach, wenn nichts passt</summary>
        public float autoSprayRetrySec;
        /// <summary>Auto-Sprüher: Meldung „kein Serum“ höchstens so oft</summary>
        public float autoSprayWarnGapSec;
        /// <summary>Höhe der Rohre zur Kammer</summary>
        public float pipeY;
        /// <summary>Rohrende vor dem Kammerrand</summary>
        public float pipeGap;
    }
}
