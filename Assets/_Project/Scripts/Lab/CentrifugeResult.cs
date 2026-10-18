namespace PlanErfuellt
{
    /// <summary>Ergebnis des Bremsens an der Zentrifuge.</summary>
    public enum CentrifugeResult
    {
        /// <summary>Unter der gelben Zone: nichts passiert.</summary>
        TooEarly,
        /// <summary>Rohserum zentrifugiert (gelb oder rot).</summary>
        Centrifuged,
        /// <summary>Rohserum in der grünen Zone.</summary>
        CentrifugedPerfect,
        /// <summary>Normales Serum ×1,25.</summary>
        Concentrated,
        /// <summary>Normales Serum ×1,5 (grün).</summary>
        ConcentratedPerfect
    }
}
