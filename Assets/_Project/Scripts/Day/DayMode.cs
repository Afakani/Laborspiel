namespace PlanErfuellt
{
    /// <summary>Abschnitt des Spielablaufs (Prototyp G.mode).</summary>
    public enum DayMode
    {
        Intro,
        Order,
        Play,
        DayOver,
        /// <summary>Sondersendung im Staats-TV (Spielzeit steht).</summary>
        News,
        /// <summary>Spielende Sibirien: Zwischensequenz am Rolltor (Spielzeit steht).</summary>
        Arrest,
        GameOver
    }
}
