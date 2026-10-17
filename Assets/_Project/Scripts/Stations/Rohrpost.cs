namespace PlanErfuellt
{
    /// <summary>Rohrpost zum Direktor (GDD 6.7). Kaviar-Wirkung folgt in Phase 9.</summary>
    public class Rohrpost : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = State.caviar > 0;
            text = canUse ? Texts.Format("ui.prompts.rohrpost", ("n", State.caviar)) : Texts.Get("ui.prompts.rohrpostNone");
            holdTime = canUse ? Cfg.interaction.hold.rohrpost : DisabledHold;
        }

        public override void Execute() => Placeholder("9");
    }
}
