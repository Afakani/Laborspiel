namespace PlanErfuellt
{
    /// <summary>Rohrpost zum Direktor (GDD 6.7): Kaviar senken die Wut; die Kapsel saust das Rohr hoch (RohrpostFx).</summary>
    public class Rohrpost : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = State.caviar > 0;
            text = canUse ? Texts.Format("ui.prompts.rohrpost", ("n", State.caviar)) : Texts.Get("ui.prompts.rohrpostNone");
            holdTime = canUse ? Cfg.interaction.hold.rohrpost : DisabledHold;
        }

        public override void Execute()
        {
            if (!StationLogic.SendCaviar(State, Cfg).Report(GM)) return;
            GM.Toast("ui.toasts.caviar", ToastKind.Good, ("n", UnityEngine.Mathf.RoundToInt(-Cfg.anger.caviar)));
            EventBus.RaiseStation("rohrpost");
            var director = FindAnyObjectByType<Director>();
            if (director != null) director.SayAny("director.caviar");
        }
    }
}
