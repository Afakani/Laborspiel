using System;

namespace PlanErfuellt
{
    /// <summary>Sonderwünsche des Direktors (GDD 7.3).</summary>
    [Serializable]
    public class WishesConfig
    {
        public int fromDay;
        public float firstAfterSec;
        public float[] intervalSec = new float[0];
        public LinearValue reward = new LinearValue();
        public WishTypeConfig[] types = new WishTypeConfig[0];
    }
}
