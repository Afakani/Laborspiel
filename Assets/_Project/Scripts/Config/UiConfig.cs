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
    }
}
