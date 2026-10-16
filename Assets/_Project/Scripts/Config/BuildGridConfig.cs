using System;

namespace PlanErfuellt
{
    /// <summary>Raster des Baumodus (GDD 3).</summary>
    [Serializable]
    public class BuildGridConfig
    {
        public int cols;
        public int rows;
        public float cell;
        public float x0;
        public float z0;
        public int blockedRow;
        public int blockedFrom;
        public int blockedTo;
    }
}
