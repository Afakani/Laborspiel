using System;

namespace PlanErfuellt
{
    /// <summary>Grundwerte einer Tierart.</summary>
    [Serializable]
    public class AnimalBaseConfig
    {
        /// <summary>Grundertrag Milch/Eier in t/s</summary>
        public float baseRate;
        /// <summary>Grundfleisch in t bei Verarbeitung</summary>
        public float baseMeat;
        /// <summary>Ware: m oder e</summary>
        public string produces;
    }
}
