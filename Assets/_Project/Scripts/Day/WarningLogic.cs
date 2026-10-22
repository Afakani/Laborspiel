namespace PlanErfuellt
{
    /// <summary>
    /// Warnmeldungen in der Bildmitte (auf Wunsch nach Spieltest, auch ohne Einführung): Direktor wird ungeduldig bzw.
    /// wütend (je Wut-Stufe einmal, nach Beruhigung wieder), Plan kurz vor Schichtende noch offen (day.lateNoticeSec,
    /// einmal pro Schicht), Kommissar kommt gleich (inspection.announceSec vor der geplanten Kontrolle).
    /// </summary>
    public static class WarningLogic
    {
        public const string AngerAnnoyed = "angerAnnoyed";
        public const string AngerFurious = "angerFurious";
        public const string TimeLow = "timeLow";
        public const string InspectionSoon = "inspectionSoon";

        /// <summary>Wut-Stufe 0 (ruhig), 1 (ungeduldig), 2 (wütend).</summary>
        public static int AngerLevel(GameState s, GameConfig cfg) =>
            s.anger >= cfg.anger.tiers[1] ? 2 : s.anger >= cfg.anger.tiers[0] ? 1 : 0;

        /// <summary>Zeitschritt in der Schicht. Gibt die Id einer neuen Warnung zurück, sonst null.</summary>
        public static string Tick(GameState s, GameConfig cfg)
        {
            int level = AngerLevel(s, cfg);
            if (level < s.angerWarned) s.angerWarned = level;
            if (level > s.angerWarned)
            {
                s.angerWarned = level;
                return level == 2 ? AngerFurious : AngerAnnoyed;
            }

            var q = s.quota;
            if (q != null && !q.met && q.left <= cfg.day.lateNoticeSec && s.timeWarnedDay != s.dayNum)
            {
                s.timeWarnedDay = s.dayNum;
                return TimeLow;
            }

            if (InspectionDue(s, cfg) && !s.inspectionAnnounced)
            {
                s.inspectionAnnounced = true;
                return InspectionSoon;
            }
            return null;
        }

        /// <summary>Planmäßige Kontrolle in höchstens announceSec?</summary>
        static bool InspectionDue(GameState s, GameConfig cfg)
        {
            var c = cfg.inspection;
            float left = s.nextInspectionAt - s.playTime;
            return s.inspection == null && s.dayNum >= c.fromDay && left > 0f && left <= c.announceSec;
        }
    }
}
