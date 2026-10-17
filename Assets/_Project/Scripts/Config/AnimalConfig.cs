using System;

namespace PlanErfuellt
{
    /// <summary>Tier in der Kammer: Bewegung, Eier, Schild, Meter (Prototyp updateSubject).</summary>
    [Serializable]
    public class AnimalConfig
    {
        public float standY;
        public float enterDropHeight;
        public float enterDropSpeed;
        public float enterSpeed;
        public float exitSpeed;
        public float procStopBefore;
        public float popInSpeed;
        public float popOutSpeed;
        public float destroySec;
        public float sinkSpeed;
        public float flyRiseSpeed;
        public float flyDriftSpeed;
        public float flyGoneHeight;
        public float eggInterval;
        public float labelHeightCow;
        public float labelHeightHen;
        public float meterRateMaxCow;
        public float meterRateMaxHen;
        public int meterSegments;
        public float meterStabGood;
        public float meterStabMid;
        public float sprayToastBad;
        public float sprayToastWarn;
        public float gasFlashSec;
    }
}
