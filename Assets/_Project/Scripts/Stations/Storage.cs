namespace PlanErfuellt
{
    /// <summary>Lager (GDD 3): zeigt den Bestand, keine Aktion.</summary>
    public class Storage : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var s = State;
            text = Texts.Format("ui.prompts.storage", ("a", s.substances[0]), ("b", s.substances[1]), ("c", s.substances[2]), ("ant", s.antidote));
            canUse = false;
            holdTime = DisabledHold;
        }

        public override void Execute() { }
    }
}
