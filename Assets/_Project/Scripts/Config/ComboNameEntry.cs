using System;

namespace PlanErfuellt
{
    /// <summary>Fester Name einer Kombination, z. B. 1-1-1 = Planformel.</summary>
    [Serializable]
    public class ComboNameEntry
    {
        /// <summary>Dosis als A-B-C</summary>
        public string key;
        /// <summary>Name</summary>
        public string displayName;
    }
}
