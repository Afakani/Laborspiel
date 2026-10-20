namespace PlanErfuellt
{
    /// <summary>Bestellterminal im Lager: öffnet das Bestell-Panel (Bestellung für den nächsten Morgen).</summary>
    public class OrderTerminal : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var s = State;
            text = s.pendingOrder != null ? Texts.Get("ui.prompts.terminalOrdered") : Texts.Get("ui.prompts.terminal");
            canUse = true;
            holdTime = Cfg.interaction.hold.terminal;
        }

        public override void Execute() => EventBus.RaisePanelRequested("order");
    }
}
