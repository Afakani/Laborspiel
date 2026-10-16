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
    }
}
