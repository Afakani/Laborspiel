namespace PlanErfuellt
{
    /// <summary>
    /// Wann die Partei durchsagt (D157): steigt die Parteikontrolle über announce.watchAt bzw. alarmAt, kommt einmal eine
    /// Durchsage; erneut erst, wenn sie um announce.hysteresis darunter gefallen ist. Während einer Kontrolle und vor dem
    /// ersten Kontrolltag still.
    /// </summary>
    public static class AnnounceLogic
    {
        public const string Watch = "watch";
        public const string Alarm = "alarm";
        public const string Soon = "soon";
        public const string Arrive = "arrive";
        public const string ArriveBoss = "arriveBoss";
        public const string Passed = "passed";

        /// <summary>Zeitschritt in der Schicht. Gibt den Schlüssel einer neuen Durchsage zurück, sonst null.</summary>
        public static string Tick(GameState s, GameConfig cfg)
        {
            var a = cfg.announce;
            float v = s.authority;
            // Rückfall nur mit Abstand, damit es bei Schwankungen an der Schwelle nicht dauernd durchsagt
            while (s.authorityAnnounced > 0 && v < Threshold(s.authorityAnnounced, a) - a.hysteresis)
                s.authorityAnnounced--;
            int level = v >= a.alarmAt ? 2 : v >= a.watchAt ? 1 : 0;
            if (level <= s.authorityAnnounced) return null;
            s.authorityAnnounced = level;
            if (s.inspection != null || s.dayNum < cfg.inspection.fromDay) return null;
            return level == 2 ? Alarm : Watch;
        }

        static float Threshold(int level, AnnounceConfig a) => level >= 2 ? a.alarmAt : a.watchAt;
    }
}
