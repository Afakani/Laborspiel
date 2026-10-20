using System.Collections.Generic;

namespace PlanErfuellt
{
    /// <summary>Fertige Sendung: Kennung, Schlagzeile, Zeitzeile, Laufband, Stufe und Weltwerte fürs Diagramm.</summary>
    public class NewsCopy
    {
        public string tag;
        public string head;
        public string year;
        public readonly List<string> ticker = new List<string>();
        public NewsLevel level;
        public bool anchorGone;
        /// <summary>Stimmung des Sprechers (NewsLogic.Mood).</summary>
        public NewsMood mood;
        public bool special;
        public WorldValues world;
    }
}
