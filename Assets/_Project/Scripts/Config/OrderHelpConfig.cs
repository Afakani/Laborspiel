using System;

namespace PlanErfuellt
{
    /// <summary>Empfohlene Bestellung am Terminal (und Pflicht in der Einführung).</summary>
    [Serializable]
    public class OrderHelpConfig
    {
        /// <summary>Empfehlung je Plan (für den Plan von morgen)</summary>
        public OrderPlanConfig[] plans = new OrderPlanConfig[0];
        /// <summary>Reihenfolge beim Kürzen, wenn die Kasse nicht reicht (Artikel-Index)</summary>
        public int[] trim = new int[0];
    }
}
