namespace PlanErfuellt
{
    /// <summary>Stockbett im Schlafraum: mit erfülltem Plan die Schicht früher beenden (RoomLogic.Sleep).</summary>
    public class Bunk : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = State.quota != null && State.quota.met;
            text = Texts.Get(canUse ? "ui.prompts.bed" : "ui.prompts.bedNotMet");
            holdTime = canUse ? Cfg.interaction.hold.bed : DisabledHold;
        }

        public override void Execute()
        {
            if (!RoomLogic.Sleep(State).Report(GM)) return;
            GM.Toast("ui.toasts.bedEarly", ToastKind.Good);
            EventBus.RaiseStation("bed");
        }
    }
}
