using System;

namespace PlanErfuellt
{
    /// <summary>Direktor-Wut (GDD 7.2).</summary>
    [Serializable]
    public class AngerConfig
    {
        public float max;
        public float idleAfterSec;
        public float idlePerSec;
        public float explode;
        public float collapse;
        public float planFailed;
        public float planMet;
        public float inspectionPassed;
        public float caviar;
        public float wishDone;
        public float wishFailed;
        /// <summary>Grenzen der Wut-Stufen</summary>
        public float[] tiers = new float[0];
        /// <summary>Abstand zufälliger Rufe [min, max]</summary>
        public float[] shoutIntervalSec = new float[0];
        /// <summary>Zufälliger Zuschlag auf den Rufabstand</summary>
        public float shoutJitterSec;
        /// <summary>Wahrscheinlichkeit eines Spruchs beim NOT-STOPP</summary>
        public float stopShoutChance;
        /// <summary>Erster zufälliger Ruf nach Spielstart</summary>
        public float startShoutDelaySec;
    }
}
