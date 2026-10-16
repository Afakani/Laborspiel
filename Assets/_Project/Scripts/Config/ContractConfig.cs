using System;

namespace PlanErfuellt
{
    /// <summary>Ein Sonderauftrag (GDD 7.4).</summary>
    [Serializable]
    public class ContractConfig
    {
        /// <summary>Ware m/e/f</summary>
        public string k;
        /// <summary>Mindestqualität</summary>
        public float minQ;
        /// <summary>Auftraggeber</summary>
        public string who;
        /// <summary>Warenname</summary>
        public string what;
    }
}
