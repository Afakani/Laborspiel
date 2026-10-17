using System;

namespace PlanErfuellt
{
    /// <summary>Hubtüren links (Einlass) und rechts (Auslass) der Kammer.</summary>
    [Serializable]
    public class ChamberDoorsConfig
    {
        /// <summary>Dauer Öffnen</summary>
        public float openSec;
        /// <summary>Dauer Schließen</summary>
        public float closeSec;
        /// <summary>Lichte Breite der Schleuse</summary>
        public float width;
        /// <summary>Lichte Höhe ab Kammerboden</summary>
        public float height;
        /// <summary>Hubweg der Tür nach oben</summary>
        public float lift;
        /// <summary>Abstand der Tür vor dem Glas</summary>
        public float radialOffset;
        /// <summary>Kuh wartet so weit vor der geschlossenen Tür (Mitte, mal Größe)</summary>
        public float waitGapCow;
        public float waitGapHen;
        /// <summary>Tür schließt, wenn die Kuh so weit hindurch ist (mal Größe)</summary>
        public float clearGapCow;
        public float clearGapHen;
    }
}
