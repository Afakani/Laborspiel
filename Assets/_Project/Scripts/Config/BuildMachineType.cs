using Newtonsoft.Json;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Baubare Maschine (GDD 6.10). Wird vom BalancingImporter erzeugt.</summary>
    public class BuildMachineType : ScriptableObject
    {
        /// <summary>pump, spray, cool oder doc.</summary>
        public string id;
        [JsonProperty("name")] public string displayName;
        public float price;
        public int unlockPlanIdx;
        /// <summary>Kurzbeschreibung der Wirkung.</summary>
        public string effect;
        /// <summary>Melkpumpe: zusätzliche Milch in t/s.</summary>
        public float milkPerSec;
        /// <summary>Melkpumpe: Qualität der gepumpten Milch.</summary>
        public float quality;
        /// <summary>Auto-Sprüher / Notfall-Automat: Abstand in Sekunden.</summary>
        public float intervalSec;
        /// <summary>Kühlaggregat: Hitzeabbau pro Sekunde.</summary>
        public float heatPerSec;
    }
}
