namespace PlanErfuellt
{
    /// <summary>Essensausgabe in der Kantine: öffnet die Speisekarte (CanteenUI).</summary>
    public class CanteenCounter : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = true;
            text = State.mealBoost > 1f
                ? Texts.Format("ui.prompts.canteenFed", ("x", State.mealBoost.ToString("0.0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"))))
                : Texts.Get("ui.prompts.canteen");
            holdTime = Cfg.interaction.hold.canteen;
        }

        public override void Execute() => EventBus.RaisePanelRequested("canteen");
    }
}
