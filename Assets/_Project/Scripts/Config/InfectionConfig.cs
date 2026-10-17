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
        /// <summary>Infektionswert bei Ansteckung</summary>
        public float levelStart;
        /// <summary>Zuwachs bei erneuter Ansteckung</summary>
        public float levelMore;
        /// <summary>Langsamer Anstieg pro s</summary>
        public float levelPerSec;
        /// <summary>Erstes Symptom nach</summary>
        public float firstSymptomSec;
        /// <summary>Grüner Schleier: Grunddeckkraft</summary>
        public float veilBase;
        /// <summary>Schleier pro Infektionswert</summary>
        public float veilPerLevel;
        /// <summary>Anteil des Pulsierens</summary>
        public float veilPulse;
        /// <summary>Deckkraft beim Symptom grün</summary>
        public float greenVeil;
        /// <summary>Schwanken der Kamera beim Symptom wobble</summary>
        public float wobbleDeg;
        public float wobbleFreq;
        /// <summary>Unschärfe beim Symptom blur</summary>
        public float blurRadius;
    }
}
