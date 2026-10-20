using System;

namespace PlanErfuellt
{
    /// <summary>Kantine: Essen als Produktions-Schub.</summary>
    [Serializable]
    public class CanteenConfig
    {
        public MealConfig[] meals = new MealConfig[0];
    }
}
