using System;

namespace PlanErfuellt
{
    /// <summary>Probenmikroskop (GDD 6.2).</summary>
    [Serializable]
    public class MicroscopeConfig
    {
        public float cost;
        public float analyseSec;
        /// <summary>Schwellen für ↑ / ↑↑ / ↑↑↑</summary>
        public float[] factorLevels = new float[0];
        /// <summary>Schwellen für ↓ / – / ↑</summary>
        public float[] qualityLevels = new float[0];
        /// <summary>Schwellen für nervös / ruhig</summary>
        public float[] stabLevels = new float[0];
    }
}
