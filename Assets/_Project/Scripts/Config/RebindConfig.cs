using System;

namespace PlanErfuellt
{
    /// <summary>Belegbare Taste: Aktion (Map/Name) und bei Laufen der Teil (up, down, left, right).</summary>
    [Serializable]
    public class RebindConfig
    {
        /// <summary>Text unter ui.settings.keyNames</summary>
        public string id;
        /// <summary>z. B. Player/Interact</summary>
        public string action;
        /// <summary>Teil einer zusammengesetzten Bindung oder leer</summary>
        public string part;
    }
}
