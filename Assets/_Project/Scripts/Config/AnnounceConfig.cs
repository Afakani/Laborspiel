using System;

namespace PlanErfuellt
{
    /// <summary>Hallen-Durchsagen der Partei und Rotlicht vor der Kontrolle (D157).</summary>
    [Serializable]
    public class AnnounceConfig
    {
        /// <summary>Lautstärke relativ zu den Effekten</summary>
        public float volume;
        /// <summary>Parteikontrolle: erste Durchsage ab</summary>
        public float watchAt;
        /// <summary>Parteikontrolle: zweite Durchsage ab</summary>
        public float alarmAt;
        /// <summary>Erneut erst, wenn die Parteikontrolle so weit unter die Schwelle gefallen ist</summary>
        public float hysteresis;
        /// <summary>Pause zwischen zwei Durchsagen</summary>
        public float gapSec;
        /// <summary>Rotlicht (Hex)</summary>
        public string lightColor;
        public float lightIntensity;
        /// <summary>Reichweite in m</summary>
        public float lightRange;
        /// <summary>Blinken pro s</summary>
        public float lightPulseHz;
        /// <summary>Höhe der Lampen in m</summary>
        public float lightHeight;
        /// <summary>Roter Schein über dem Bild: höchste Deckkraft</summary>
        public float tintAlpha;
    }
}
