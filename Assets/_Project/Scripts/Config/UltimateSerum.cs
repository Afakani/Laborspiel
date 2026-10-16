using Newtonsoft.Json;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Ein ultimatives Serum (GDD 4.5). Wird vom BalancingImporter erzeugt.</summary>
    public class UltimateSerum : ScriptableObject
    {
        /// <summary>Schlüssel aus der JSON (omega, koloss).</summary>
        [JsonIgnore] public string id;
        [JsonProperty("name")] public string displayName;
        /// <summary>Rezept A-B-C.</summary>
        public int[] recipe = new int[3];
        public string color;
        public float rateMult = 1f;
        public float meatMult = 1f;
        public float quality;
        public float stability;
        public string[] pros = new string[0];
        public string[] cons = new string[0];

        public Dose Recipe => Dose.FromArray(recipe);
        public Color Color => ColorUtility.TryParseHtmlString(color, out var c) ? c : Color.white;
    }
}
