namespace PlanErfuellt
{
    /// <summary>Bestellterminal (GDD 6.9). Während der Schicht nur Hinweis; Bestell-Overlay folgt in Phase 6.</summary>
    public class OrderTerminal : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            text = Texts.Get("ui.prompts.terminal");
            canUse = false;
            holdTime = DisabledHold;
        }

        public override void Execute() { }
    }
}
