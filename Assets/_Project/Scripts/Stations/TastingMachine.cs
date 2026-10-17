namespace PlanErfuellt
{
    /// <summary>Verkostungsautomat (GDD 6.6), ab Plan 2. Siegel folgt in Phase 9.</summary>
    public class TastingMachine : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var t = Cfg.machinesFixed.tasting;
            if (State.planIdx < t.unlockPlanIdx)
            {
                text = Texts.Format("ui.prompts.tastingLocked", ("n", t.unlockPlanIdx + 1));
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            text = Texts.Format("ui.prompts.tasting", ("cost", t.cost), ("bonus", t.qualityBonus));
            canUse = true;
            holdTime = Cfg.interaction.hold.tasting;
        }

        public override void Execute() => Placeholder("9");
    }
}
