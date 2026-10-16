using System;

namespace PlanErfuellt
{
    /// <summary>Infektion des Spielers (GDD 8).</summary>
    [Serializable]
    public class InfectionConfig
    {
        public float[] symptomIntervalSec = new float[0];
        public float[] symptomDurationSec = new float[0];
        public float fast;
        public float slow;
        public string[] symptoms = new string[0];
    }
}
