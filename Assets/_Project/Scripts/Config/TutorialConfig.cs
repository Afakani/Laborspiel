using System;

namespace PlanErfuellt
{
    /// <summary>Geführte Einführung an Tag 1.</summary>
    [Serializable]
    public class TutorialConfig
    {
        /// <summary>Lese-Schritte gehen nach dieser Zeit weiter</summary>
        public float readSec;
        /// <summary>Reihenfolge der Schritte (Texte in ui.tutorial.steps)</summary>
        public TutorialStepConfig[] steps = new TutorialStepConfig[0];
    }
}
