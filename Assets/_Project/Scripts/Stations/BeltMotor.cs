namespace PlanErfuellt
{
    /// <summary>Bandmotor (GDD 5.2): bei gerissenem Förderband hier reparieren.</summary>
    public class BeltMotor : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = State.beltBroken;
            text = Texts.Get(canUse ? "ui.prompts.beltRepair" : "ui.prompts.beltOk");
            holdTime = canUse ? Cfg.interaction.hold.beltRepair : DisabledHold;
        }

        public override void Execute()
        {
            State.beltBroken = false;
            GM.Toast("ui.toasts.beltRepaired", ToastKind.Good);
        }
    }
}
