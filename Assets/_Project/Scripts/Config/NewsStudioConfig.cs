using System;

namespace PlanErfuellt
{
    /// <summary>Kamerafahrt und Sprecher im Studio (Prototyp studio, Studio-Koordinaten).</summary>
    [Serializable]
    public class NewsStudioConfig
    {
        public float[] widePos = new float[0];
        public float[] wideTarget = new float[0];
        public float[] closePos = new float[0];
        public float[] closeTarget = new float[0];
        public float[] closeDeadPos = new float[0];
        public float[] closeDeadTarget = new float[0];
        public float fov;
        /// <summary>Sprecher sackt nach dieser Zeit zusammen</summary>
        public float slumpDelaySec;
        public float slumpSpeed;
    }
}
