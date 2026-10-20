namespace PlanErfuellt
{
    /// <summary>Stimmung des Sprechers in der Sendung (Haltung, Gesicht, Kamera).</summary>
    public enum NewsMood
    {
        /// <summary>Plan erfüllt: strahlt, winkt, Konfetti.</summary>
        Joy,
        /// <summary>Kein Tagesergebnis oder gemischte Lage: gefasst, raschelt mit Papier.</summary>
        Calm,
        /// <summary>Plan verfehlt: schwitzt, wischt sich die Stirn, Blick wandert nervös.</summary>
        Sweat,
        /// <summary>Wiederholt verfehlt oder Krise: weint ins Taschentuch.</summary>
        Cry
    }
}
