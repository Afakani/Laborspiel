namespace PlanErfuellt
{
    /// <summary>Laufende Kontrolle durch den Kommissar (GDD 9). Position x in Prototyp-Koordinaten.</summary>
    public class Inspection
    {
        /// <summary>Aktuelle Position auf dem Weg vor der Kammer (läuft von rechts nach links).</summary>
        public float x;
        /// <summary>Verdacht 0..suspMax.</summary>
        public float suspicion;
        /// <summary>Restzeit der Wodka-Pause (er steht und schwankt).</summary>
        public float pause;
        /// <summary>Oberkommissar: doppelte Prämie, mehr Verdacht.</summary>
        public bool boss;
        /// <summary>Warnung „beäugt das Serum-Regal“ schon gezeigt.</summary>
        public bool rackWarned;

        public bool Drinking => pause > 0f;
    }
}
