using System;

namespace PlanErfuellt
{
    /// <summary>Klang zu einer Meldung (Prototyp: SFX-Aufruf neben toast()).</summary>
    [Serializable]
    public class ToastSound
    {
        /// <summary>Textpfad der Meldung, z. B. ui.toasts.planMet</summary>
        public string key;
        /// <summary>Klang-Id (Dateiname ohne Endung)</summary>
        public string sound;
    }
}
