namespace PlanErfuellt
{
    /// <summary>Sonderwunsch des Direktors (GDD 7.3).</summary>
    public class Wish
    {
        public WishTypeConfig type;
        public string Id => type.id;
        /// <summary>Benötigte Menge (t) bzw. 1 bei Qualität/Ruhe.</summary>
        public float need;
        public float got;
        public float left;
        /// <summary>Ruhe-Wunsch: bisher keine Krise.</summary>
        public bool ok = true;
    }
}
