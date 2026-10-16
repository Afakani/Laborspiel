using System.Collections.Generic;

namespace PlanErfuellt
{
    /// <summary>Werte und Vor-/Nachteile einer Kombination, berechnet auf Kuh-Basis (GDD 4.4).</summary>
    public class ComboInfo
    {
        public Dose dose;
        public float rate;
        public float meat;
        public float quality;
        public float stability;
        public readonly List<ComboEffect> pros = new List<ComboEffect>();
        public readonly List<ComboEffect> cons = new List<ComboEffect>();
    }
}
