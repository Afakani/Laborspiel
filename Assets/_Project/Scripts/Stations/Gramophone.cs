namespace PlanErfuellt
{
    /// <summary>Grammophon (GDD 6.5). Musik und Wirkung folgen in Phase 9.</summary>
    public class Gramophone : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            text = Texts.Get("ui.prompts.gramophoneCrank");
            canUse = true;
            holdTime = Cfg.machinesFixed.gramophone.crankSec;
        }

        public override void Execute() => Placeholder("9");
    }
}
