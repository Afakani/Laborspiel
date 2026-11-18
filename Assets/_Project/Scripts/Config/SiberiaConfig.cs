using System;

namespace PlanErfuellt
{
    /// <summary>Zwischensequenz Sibirien (D162): Rolltor, zwei Stasi-Männer, Kamera, Ablauf in s.</summary>
    [Serializable]
    public class SiberiaConfig
    {
        /// <summary>Tor in der Vorderwand (Proto-x)</summary>
        public float gateX;
        /// <summary>Wand (Proto-z)</summary>
        public float gateZ;
        public float gateWidth;
        public float gateHeight;
        /// <summary>Kamera (Proto)</summary>
        public float camX;
        public float camZ;
        public float camHeight;
        public float camLookHeight;
        public float camFov;
        /// <summary>Kamerafahrt nach vorn in m</summary>
        public float dolly;
        public float fadeInSec;
        /// <summary>Direktor verleugnet den Spieler</summary>
        public float directorSec;
        /// <summary>Tor geht hoch ab</summary>
        public float openStartSec;
        public float openSec;
        /// <summary>Stasi-Männer gehen los ab</summary>
        public float walkStartSec;
        public float walkSec;
        /// <summary>Weg nach vorn in m</summary>
        public float walkDist;
        public float line1Sec;
        public float line2Sec;
        public float fadeOutStartSec;
        public float endSec;
        /// <summary>Größe der Stasi-Männer</summary>
        public float stasiScale;
        /// <summary>Abstand zwischen beiden in m</summary>
        public float stasiSpacing;
        /// <summary>Schritte pro s</summary>
        public float stepFreq;
        public float legSwingDeg;
        /// <summary>Gegenlicht im Tor (Hex)</summary>
        public string lightColor;
        public float lightIntensity;
        /// <summary>Kinobalken je Anteil der Bildhöhe</summary>
        public float letterbox;
        public float voiceVolume;
    }
}
