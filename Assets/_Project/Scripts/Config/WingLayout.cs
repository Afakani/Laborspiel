using System;

namespace PlanErfuellt
{
    /// <summary>Anbau mit Nebenzimmern hinter der Rückwand (Proto-z wächst nach hinten).</summary>
    [Serializable]
    public class WingLayout
    {
        /// <summary>Tiefe der Zimmer in m</summary>
        public float depth;
        /// <summary>Deckenhöhe der Zimmer</summary>
        public float height;
        /// <summary>Dicke der Trennwände</summary>
        public float partition;
        public float doorWidth;
        public float doorHeight;
        /// <summary>Abstand des Spielers zu den Wänden</summary>
        public float margin;
    }
}
