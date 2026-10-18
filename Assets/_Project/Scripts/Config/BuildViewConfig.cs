using System;

namespace PlanErfuellt
{
    /// <summary>Draufsicht im Baumodus (Prototyp camTop, Prototyp-Koordinaten).</summary>
    [Serializable]
    public class BuildViewConfig
    {
        public float camX;
        public float camY;
        public float camZ;
        public float lookX;
        public float lookY;
        public float lookZ;
        public float fov;
        /// <summary>Umgebung hinter dieser z-Linie (Rückwand) wird ausgeblendet</summary>
        public float hideBehindZ;
        /// <summary>Umgebung links dieser x-Linie (linke Wand) wird ausgeblendet</summary>
        public float hideLeftX;
        /// <summary>Umgebung über dieser Höhe (Decke) wird ausgeblendet</summary>
        public float hideAboveY;
    }
}
