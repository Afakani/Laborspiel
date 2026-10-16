using System;

namespace PlanErfuellt
{
    /// <summary>Raummaße in Prototyp-Koordinaten (GDD 3).</summary>
    [Serializable]
    public class RoomLayout
    {
        /// <summary>Seitenwände bei ±x</summary>
        public float sideWallX;
        /// <summary>Wand hinter der Kammer (Proto-z)</summary>
        public float frontWallZ;
        /// <summary>Rückwand hinter dem Spieler (Proto-z)</summary>
        public float backWallZ;
        public float wallThickness;
        /// <summary>Deckenhöhe</summary>
        public float height;
    }
}
