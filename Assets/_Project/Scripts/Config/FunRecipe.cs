using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Lustiges Rezept / Kuriosität (GDD 4.6). Wird vom BalancingImporter erzeugt.</summary>
    public class FunRecipe : ScriptableObject
    {
        /// <summary>Schlüssel aus der JSON (milkhen, sing, coinhen).</summary>
        [JsonIgnore] public string id;
        [JsonConverter(typeof(StringEnumConverter))] public AnimalKind animal;
        /// <summary>Gesamtdosis A-B-C am Tier.</summary>
        public int[] total = new int[3];
        [JsonProperty("name")] public string displayName;
        public float rateMult = 1f;
        public float meatMult = 1f;
        public float coinsPerEgg;
        public float qualityMalus;
        public string[] pros = new string[0];
        public string[] cons = new string[0];

        public Dose Total => Dose.FromArray(total);
    }
}
