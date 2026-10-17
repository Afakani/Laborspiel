using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Pult vor der Kammer (GDD 6.1). Ziel, solange der Spieler in der Pult-Zone steht. Panel folgt in Phase 4a.</summary>
    public class ControlPanel : Interactable
    {
        public override bool TryGetScore(Vector2 p, out float score)
        {
            score = Cfg.interaction.pultScore;
            return InAbsZone(p, Cfg.layout.pultZone);
        }

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            text = Texts.Get("ui.prompts.pult");
            canUse = true;
            holdTime = Cfg.interaction.hold.pult;
        }

        public override void Execute() => Placeholder("4a");
    }
}
