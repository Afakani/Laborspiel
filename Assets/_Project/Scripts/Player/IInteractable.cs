using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Etwas, das der Spieler mit gehaltener Leertaste benutzen kann (Architektur 05, Punkt 5).</summary>
    public interface IInteractable
    {
        /// <summary>Eindeutiger Schlüssel; wechselt er, beginnt der Haltebalken von vorn.</summary>
        string Key { get; }

        /// <summary>Ist der Spieler in Reichweite? Kleinere Wertung gewinnt.</summary>
        bool TryGetScore(Vector2 playerProtoXZ, out float score);

        /// <summary>Prompt-Text, ob die Aktion gerade möglich ist, und Haltezeit.</summary>
        void GetPrompt(out string text, out bool canUse, out float holdTime);

        /// <summary>Aktion ausführen (Haltezeit erreicht).</summary>
        void Execute();
    }
}
