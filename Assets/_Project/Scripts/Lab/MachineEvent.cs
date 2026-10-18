namespace PlanErfuellt
{
    /// <summary>Meldungen aus MachineLogic.Tick für Toasts und Effekte.</summary>
    public enum MachineEvent
    {
        /// <summary>Überdrehzahl: Gläser zersprungen, Serum verloren.</summary>
        CentrifugeOverspeed,
        /// <summary>Rotor ausgelaufen, Serum zurück im Regal.</summary>
        CentrifugeReturned,
        /// <summary>Kryokammer fertig, Zeitfenster läuft.</summary>
        CryoReady,
        /// <summary>Zeitfenster verpasst: Eisblock, Serum verloren.</summary>
        CryoIceBlock
    }
}
