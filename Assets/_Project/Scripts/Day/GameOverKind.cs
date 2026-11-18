namespace PlanErfuellt
{
    /// <summary>Grund für das Spielende (GDD 13).</summary>
    public enum GameOverKind
    {
        Final,
        Fired,
        Scandal,
        /// <summary>Zu viele verfehlte Pläne in Folge: ab nach Sibirien (D162).</summary>
        Siberia
    }
}
