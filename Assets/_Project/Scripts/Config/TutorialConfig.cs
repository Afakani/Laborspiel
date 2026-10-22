using System;

namespace PlanErfuellt
{
    /// <summary>Geführte Einführung an Tag 1 und Hinweise bei erstem Auftreten.</summary>
    [Serializable]
    public class TutorialConfig
    {
        /// <summary>Lese-Schritte gehen nach dieser Zeit weiter</summary>
        public float readSec;
        /// <summary>Reihenfolge der Schritte (Texte in ui.tutorial.steps)</summary>
        public TutorialStepConfig[] steps = new TutorialStepConfig[0];
        /// <summary>Hinweise, Reihenfolge = Vorrang</summary>
        public TutorialHintConfig[] hints = new TutorialHintConfig[0];
        /// <summary>Anzeigedauer von Info-Hinweisen</summary>
        public float hintInfoSec;
        /// <summary>Höchstdauer eines Hinweises, auch wenn die Lage bleibt</summary>
        public float hintMaxSec;
        /// <summary>Pflicht-Mischung der Einführung (A, B, C)</summary>
        public int[] mixDose = new int[0];
    }
}
