using System;

namespace PlanErfuellt
{
    /// <summary>Zone über |x| und z in Prototyp-Koordinaten (Pult, Laken).</summary>
    [Serializable]
    public class ZoneLayout
    {
        public float absXMin;
        public float absXMax;
        public float zMin;
        public float zMax;
    }
}
