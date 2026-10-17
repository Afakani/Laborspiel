namespace PlanErfuellt
{
    /// <summary>Welche Warnzeichen das Tier schon gezeigt hat (je einmal pro Tier, GDD 5.3).</summary>
    [System.Flags]
    public enum RiskWarnings
    {
        None = 0,
        Heat = 1,
        Acid = 2,
        Float = 4,
        Stab = 8
    }
}
