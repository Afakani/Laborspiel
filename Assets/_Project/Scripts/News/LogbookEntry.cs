using System.Collections.Generic;

namespace PlanErfuellt
{
    /// <summary>Eine Karte im Forschungslogbuch.</summary>
    public class LogbookEntry
    {
        public LogbookSection section;
        public string title;
        public string sub;
        public List<string> pros = new List<string>();
        public List<string> cons = new List<string>();
        /// <summary>Dosis für die Skizze.</summary>
        public Dose sketch;
        public AnimalKind kind;
        public string ultimateId;
        public string funId;
    }
}
