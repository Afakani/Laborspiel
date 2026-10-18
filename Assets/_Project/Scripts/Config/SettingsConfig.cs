using System;

namespace PlanErfuellt
{
    /// <summary>Einstellungen: Vorgaben und Grenzen (Maus, Grafik).</summary>
    [Serializable]
    public class SettingsConfig
    {
        /// <summary>Mausempfindlichkeit Faktor</summary>
        public float mouseDefault;
        public float mouseMin;
        public float mouseMax;
        /// <summary>Grafik Niedrig: Auflösungsfaktor</summary>
        public float lowRenderScale;
        /// <summary>Grafik Niedrig: Schattenweite m</summary>
        public float lowShadowDistance;
        /// <summary>Grafik Niedrig: Schattenkarte</summary>
        public int lowShadowResolution;
        /// <summary>Grafik Niedrig: Kaskaden</summary>
        public int lowShadowCascades;
        /// <summary>Fenstermodus: Breite</summary>
        public int windowedWidth;
        /// <summary>Fenstermodus: Höhe</summary>
        public int windowedHeight;
    }
}
