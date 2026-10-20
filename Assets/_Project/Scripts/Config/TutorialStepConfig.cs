using System;

namespace PlanErfuellt
{
    /// <summary>Schritt der Einführung: Bedingung (id) und Ziel, auf das der Pfeil zeigt.</summary>
    [Serializable]
    public class TutorialStepConfig
    {
        /// <summary>walkMixing, mix, clone, spray, watch, process, order, cloth, finish</summary>
        public string id;
        /// <summary>Station-Id (mixing, pult, terminal …), chamber oder leer</summary>
        public string target;
    }
}
