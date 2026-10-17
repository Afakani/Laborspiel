using System;

namespace PlanErfuellt
{
    /// <summary>Animation der Ego-Hände mit Spritze.</summary>
    [Serializable]
    public class HandsConfig
    {
        public float smoothing;
        public float bob;
        public float sway;
        public float idleAmp;
        public float holdX;
        public float holdY;
        public float holdZ;
        public float holdTiltDeg;
        public float thrustZ;
        public float thrustTiltDeg;
    }
}
