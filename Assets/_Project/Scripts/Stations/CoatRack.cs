namespace PlanErfuellt
{
    /// <summary>Kittel in der Umkleide: Schutzkittel für diese Schicht anziehen (RoomLogic.PutOnCoat).</summary>
    public class CoatRack : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = !State.coatOn;
            text = Texts.Format(canUse ? "ui.prompts.coat" : "ui.prompts.coatOn", ("pct", UnityEngine.Mathf.RoundToInt(Cfg.sideRooms.locker.protectChance * 100f)));
            holdTime = canUse ? Cfg.interaction.hold.coat : DisabledHold;
        }

        public override void Execute()
        {
            if (!RoomLogic.PutOnCoat(State).Report(GM)) return;
            GM.Toast("ui.toasts.coatOn", ToastKind.Good, ("pct", UnityEngine.Mathf.RoundToInt(Cfg.sideRooms.locker.protectChance * 100f)));
            EventBus.RaiseStation("coat");
        }
    }
}
