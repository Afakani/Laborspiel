using System;

namespace PlanErfuellt
{
    /// <summary>Kamerawackeln und Effektstärken (Prototyp shake).</summary>
    [Serializable]
    public class FxConfig
    {
        public float shakeExplosion;
        public float shakeProcExplosion;
        public float shakeBelt;
        public float shakeCable;
        /// <summary>Abbau pro s</summary>
        public float shakeDecay;
        /// <summary>Meter Kameraversatz pro Wackel-Einheit</summary>
        public float shakeScale;
        /// <summary>Schleimflecken am Boden nach einer Explosion</summary>
        public int slimeSplats;
        public float splatRadiusMin;
        public float splatRadiusMax;
        /// <summary>Drehung der Warnleuchte</summary>
        public float alarmTurnDegPerSec;
    }
}
