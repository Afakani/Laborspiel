using System;

namespace PlanErfuellt
{
    /// <summary>Umkleide: Schutzkittel für eine Schicht.</summary>
    [Serializable]
    public class LockerRoomConfig
    {
        /// <summary>Anteil der Explosionen, deren Ansteckung der Kittel abhält</summary>
        public float protectChance;
    }
}
