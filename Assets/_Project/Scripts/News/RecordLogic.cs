using System.Globalization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Rekorde (GDD 12, Prototyp checkRecord): höchster Ertrag, meistes Fleisch aus einem Tier, Tagesmengen je Ware.
    /// Der erste Wert wird still gespeichert; ein neuer Rekord gilt ab altem Wert + records.epsilon.
    /// </summary>
    public static class RecordLogic
    {
        public const string Rate = "rate";
        public const string Meat = "meat";
        public const string DayMilk = "day_m";
        public const string DayEggs = "day_e";
        public const string DayMeat = "day_f";

        public static readonly string[] All = { Rate, Meat, DayMilk, DayEggs, DayMeat };

        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        /// <summary>Wert eintragen. true = neuer Rekord, der gemeldet werden soll.</summary>
        public static bool Check(GameState s, GameConfig cfg, string key, float value)
        {
            if (value <= 0f) return false;
            s.records.TryGetValue(key, out float old);
            if (old > 0f && value <= old + cfg.records.epsilon) return false;
            s.records[key] = value;
            return old > 0f;
        }

        /// <summary>Wert wie im Prototyp formatiert (Ertrag und Fleisch mit einer Nachkommastelle, Tagesmengen ganz).</summary>
        public static string FormatValue(string key, float v) =>
            key == Rate || key == Meat ? v.ToString("0.0", German) : Mathf.RoundToInt(v).ToString("N0", German);

        /// <summary>Textpfad für die Meldung (ui.records.*).</summary>
        public static string ToastPath(string key) => "ui.records." + Suffix(key);

        /// <summary>Textpfad für die Rekordtafel im Logbuch (ui.logbook.record*).</summary>
        public static string LogbookPath(string key) => key switch
        {
            Rate => "ui.logbook.recordRate",
            Meat => "ui.logbook.recordMeat",
            DayMilk => "ui.logbook.recordDayM",
            DayEggs => "ui.logbook.recordDayE",
            _ => "ui.logbook.recordDayF"
        };

        static string Suffix(string key) => key switch
        {
            Rate => "rate",
            Meat => "meat",
            DayMilk => "dayM",
            DayEggs => "dayE",
            _ => "dayF"
        };
    }
}
