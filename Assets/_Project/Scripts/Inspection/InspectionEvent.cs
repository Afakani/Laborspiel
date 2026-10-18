namespace PlanErfuellt
{
    /// <summary>Meldungen aus InspectionLogic.Step für Toasts, Direktor und Effekte.</summary>
    public enum InspectionEvent
    {
        /// <summary>Kommissar (oder Oberkommissar) betritt das Labor.</summary>
        Started,
        /// <summary>Er beäugt das offene Serum-Regal.</summary>
        RackWarning,
        /// <summary>Er hat das Labor verlassen: Kontrolle bestanden.</summary>
        Passed,
        /// <summary>Verdacht voll: Skandal, Spielende.</summary>
        Scandal
    }
}
