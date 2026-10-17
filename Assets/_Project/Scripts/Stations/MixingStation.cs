using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Mischstation (GDD 6.2). Ziel im Bereich davor; Wertung = seitlicher Abstand. Öffnet das Panel.</summary>
    public class MixingStation : Interactable
    {
        public override bool TryGetScore(Vector2 p, out float score)
        {
            var z = Cfg.interaction.mixingZone;
            score = Mathf.Abs(p.x - Layout.x);
            return p.x >= z.xMin && p.x < z.xMax && p.y >= z.zMin && p.y < z.zMax;
        }

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            text = Texts.Get("ui.prompts.mix");
            canUse = true;
            holdTime = Cfg.interaction.hold.mixing;
        }

        public override void Execute() => EventBus.RaisePanelRequested("mixing");
    }
}
