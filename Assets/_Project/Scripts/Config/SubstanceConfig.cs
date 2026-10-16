using System;
using Newtonsoft.Json;

namespace PlanErfuellt
{
    /// <summary>Ein Grundstoff (A/B/C) mit Anzeige-Daten.</summary>
    [Serializable]
    public class SubstanceConfig
    {
        /// <summary>A, B oder C</summary>
        public string id;
        /// <summary>z. B. A-17</summary>
        public string code;
        /// <summary>Anzeigename</summary>
        [JsonProperty("name")]
        public string displayName;
        /// <summary>Farbe als Hex</summary>
        public string color;
    }
}
