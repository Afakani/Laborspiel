namespace PlanErfuellt
{
    /// <summary>Gebaute Maschine auf dem Raster (GDD 6.10).</summary>
    public class PlacedMachine
    {
        /// <summary>Id des Maschinentyps (pump, spray, cool, doc).</summary>
        public string typeId;
        public int gx, gz;
        /// <summary>Bezahlter Preis (für den Abriss).</summary>
        public float paid;
        /// <summary>Zeitgeber für Auto-Sprüher und Notfall-Automat.</summary>
        public float timer;
    }
}
