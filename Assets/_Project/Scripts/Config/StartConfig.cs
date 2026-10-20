using System;

namespace PlanErfuellt
{
    /// <summary>Startwerte eines neuen Spiels (GDD 4.1).</summary>
    [Serializable]
    public class StartConfig
    {
        /// <summary>Start-Kasse</summary>
        public float money;
        /// <summary>Startlager A/B/C</summary>
        public SubstanceAmounts inventory = new SubstanceAmounts();
        /// <summary>Gegenmittel</summary>
        public int antidote;
        /// <summary>Wodka</summary>
        public int vodka;
        /// <summary>Kaviar</summary>
        public int caviar;
        /// <summary>Laken</summary>
        public int cloths;
    }
}
