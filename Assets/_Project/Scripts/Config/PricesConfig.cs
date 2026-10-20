using System;

namespace PlanErfuellt
{
    /// <summary>Einkaufspreise am Bestellterminal und Kosten fürs Klonen.</summary>
    [Serializable]
    public class PricesConfig
    {
        public float A;
        public float B;
        public float C;
        public float antidote;
        public float vodka;
        public float caviar;
        /// <summary>Laken (wird bei jedem Einsatz verbraucht)</summary>
        public float cloth;
        /// <summary>Klonen einer Kuh</summary>
        public float cloneCow;
        /// <summary>Klonen eines Huhns</summary>
        public float cloneHen;
    }
}
