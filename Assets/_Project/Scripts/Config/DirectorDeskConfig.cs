using System;

namespace PlanErfuellt
{
    /// <summary>Direktorbüro: Kaviar persönlich abgeben.</summary>
    [Serializable]
    public class DirectorDeskConfig
    {
        /// <summary>Wut −x bei guter Laune</summary>
        public float angerReduce;
        /// <summary>Chance, dass er stattdessen brüllt</summary>
        public float shoutChance;
        /// <summary>Wut +x beim Brüllen</summary>
        public float shoutAnger;
    }
}
