using System;

namespace PlanErfuellt
{
    /// <summary>Empfohlene Bestellung für einen Plan.</summary>
    [Serializable]
    public class OrderPlanConfig
    {
        /// <summary>A, B, C, Gegenmittel, Wodka, Kaviar, Laken</summary>
        public int[] amounts = new int[0];
    }
}
