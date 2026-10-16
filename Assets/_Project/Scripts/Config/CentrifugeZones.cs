using System;

namespace PlanErfuellt
{
    /// <summary>Zonen des Drehzahlbalkens (0..1).</summary>
    [Serializable]
    public class CentrifugeZones
    {
        public float tooEarly;
        public float[] yellow = new float[0];
        public float[] green = new float[0];
        public float[] red = new float[0];
    }
}
