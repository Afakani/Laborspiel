namespace PlanErfuellt
{
    /// <summary>Kryokammer (GDD 6.4), ab Plan 2. Einfrieren folgt in Phase 8.</summary>
    public class CryoChamber : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var c = Cfg.machinesFixed.cryo;
            if (State.planIdx < c.unlockPlanIdx)
            {
                text = Texts.Format("ui.prompts.cryoLocked", ("n", c.unlockPlanIdx + 1));
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            canUse = HasSuitableSerum();
            text = Texts.Get(canUse ? "ui.prompts.cryoLoad" : "ui.prompts.cryoNone");
            holdTime = canUse ? Cfg.interaction.hold.cryoLoad : DisabledHold;
        }

        public override void Execute() => Placeholder("8");

        /// <summary>Zentrifugiertes oder normales, noch nicht tiefgekühltes Serum im Regal.</summary>
        bool HasSuitableSerum()
        {
            foreach (var s in State.serums)
                if (s.stage == SerumStage.Centrifuged || (!s.IsUltimate && !s.frozen && s.stage == SerumStage.Ready)) return true;
            return false;
        }
    }
}
