namespace PlanErfuellt
{
    /// <summary>Herstellungsstufe eines Serums (GDD 4.2).</summary>
    public enum SerumStage
    {
        /// <summary>Normales Serum.</summary>
        Ready,
        /// <summary>Mischung entspricht einem Ultimativ-Rezept.</summary>
        Raw,
        /// <summary>Rohserum nach der Zentrifuge.</summary>
        Centrifuged
    }
}
