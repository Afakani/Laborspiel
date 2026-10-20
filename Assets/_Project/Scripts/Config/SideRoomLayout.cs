using System;

namespace PlanErfuellt
{
    /// <summary>Nebenzimmer im Anbau: x-Bereich und Türmitte in der Rückwand der Halle.</summary>
    [Serializable]
    public class SideRoomLayout
    {
        /// <summary>storage, locker, bedroom, canteen, director (Schild: ui.rooms.<id>)</summary>
        public string id;
        public float xMin;
        public float xMax;
        /// <summary>Türmitte</summary>
        public float doorX;
    }
}
