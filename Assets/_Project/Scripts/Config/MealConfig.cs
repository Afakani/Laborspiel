using System;

namespace PlanErfuellt
{
    /// <summary>Essen in der Kantine: Preis und Serumstärke für den Rest der Schicht.</summary>
    [Serializable]
    public class MealConfig
    {
        /// <summary>Text: ui.canteen.meals[i]</summary>
        public string id;
        public float price;
        /// <summary>Faktor auf die Serumwirkung (Ertrag, Fleisch)</summary>
        public float boost;
    }
}
