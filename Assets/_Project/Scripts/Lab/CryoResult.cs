namespace PlanErfuellt
{
    /// <summary>Ergebnis beim Herausnehmen aus der Kryokammer.</summary>
    public enum CryoResult
    {
        /// <summary>Normales Serum tiefgekühlt (+Stabilität beim Sprühen).</summary>
        Frozen,
        /// <summary>Zentrifugiertes Rohserum ab Plan 3: ultimativ.</summary>
        Ultimate,
        /// <summary>Zentrifugiertes Rohserum vor Plan 3: nur tiefgekühlt.</summary>
        NoReaction
    }
}
