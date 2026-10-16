using System;

namespace PlanErfuellt
{
    /// <summary>Glaskammer B-12 (GDD 3).</summary>
    [Serializable]
    public class ChamberLayout
    {
        public float x;
        public float z;
        public float radius;
        public float height;
        public float baseY;
        /// <summary>Spieler kommt der Kammermitte nicht näher</summary>
        public float playerCollisionRadius;
    }
}
