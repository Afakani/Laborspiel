namespace PlanErfuellt
{
    /// <summary>Zentrifuge (GDD 6.3). Timing-Minispiel folgt in Phase 8.</summary>
    public class Centrifuge : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = HasSuitableSerum();
            text = Texts.Get(canUse ? "ui.prompts.centrifugeLoad" : "ui.prompts.centrifugeNone");
            holdTime = canUse ? Cfg.interaction.hold.centrifugeLoad : DisabledHold;
        }

        public override void Execute() => Placeholder("8");

        /// <summary>Rohserum oder normales, noch nicht konzentriertes Serum im Regal.</summary>
        bool HasSuitableSerum()
        {
            foreach (var s in State.serums)
                if (s.stage == SerumStage.Raw || (!s.IsUltimate && !s.IsConcentrated && s.stage == SerumStage.Ready)) return true;
            return false;
        }
    }
}
