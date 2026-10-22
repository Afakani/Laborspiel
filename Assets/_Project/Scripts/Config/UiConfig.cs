using System;

namespace PlanErfuellt
{
    /// <summary>Allgemeine Anzeige-Werte (GDD 7.6).</summary>
    [Serializable]
    public class UiConfig
    {
        /// <summary>Gleichzeitige Kurzmeldungen</summary>
        public int toastMax;
        /// <summary>Anzeigedauer</summary>
        public float toastSec;
        /// <summary>Ab diesem Anteil der Dauer ausblenden</summary>
        public float toastFadeFrom;
        /// <summary>Untertitel mindestens so lange</summary>
        public float subtitleMinSec;
        /// <summary>plus pro Zeichen</summary>
        public float subtitlePerChar;
        /// <summary>Plantafel und TV-Bild auffrischen alle</summary>
        public float boardRefreshSec;
        /// <summary>Pause zwischen Schichtende und Bestellterminal</summary>
        public float dayEndDelaySec;
        /// <summary>Sonderauftrag wird so spät nach Schichtbeginn gemeldet</summary>
        public float contractAnnounceDelaySec;
        /// <summary>Anzeigedauer der Warnmeldung in der Bildmitte</summary>
        public float alertSec;
    }
}
