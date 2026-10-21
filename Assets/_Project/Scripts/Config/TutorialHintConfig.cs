using System;

namespace PlanErfuellt
{
    /// <summary>Hinweis beim ersten Auftreten einer Lage (Texte ui.tutorial.hints.<id>).</summary>
    [Serializable]
    public class TutorialHintConfig
    {
        /// <summary>inspection, unstable, belt, contaminated, infected, noCloth, anger, centrifuge, cryo, ultimate, rooms</summary>
        public string id;
        /// <summary>Station-Id, chamber oder leer</summary>
        public string target;
        /// <summary>Reiner Info-Hinweis: verschwindet nach hintInfoSec</summary>
        public bool info;
    }
}
