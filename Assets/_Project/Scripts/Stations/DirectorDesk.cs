using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Schreibtisch im Direktorbüro: Kaviar persönlich abgeben – mehr Wirkung als die Rohrpost, aber er kann brüllen.</summary>
    public class DirectorDesk : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = State.caviar > 0;
            var d = Cfg.sideRooms.desk;
            text = canUse
                ? Texts.Format("ui.prompts.desk", ("n", State.caviar), ("anger", Mathf.RoundToInt(d.angerReduce)), ("pct", Mathf.RoundToInt(d.shoutChance * 100f)))
                : Texts.Get("ui.prompts.deskNone");
            holdTime = canUse ? Cfg.interaction.hold.desk : DisabledHold;
        }

        public override void Execute()
        {
            if (!RoomLogic.DeliverCaviar(State, Cfg, Random.value, out bool shouted).Report(GM)) return;
            var d = Cfg.sideRooms.desk;
            var director = FindAnyObjectByType<Director>();
            if (shouted)
            {
                GM.Toast("ui.toasts.deskShout", ToastKind.Bad, ("n", Mathf.RoundToInt(d.shoutAnger)));
                if (director != null) director.SayAny("director.deskShout");
            }
            else
            {
                GM.Toast("ui.toasts.deskCaviar", ToastKind.Good, ("n", Mathf.RoundToInt(d.angerReduce)));
                if (director != null) director.SayAny("director.deskThanks");
            }
            EventBus.RaiseStation("desk");
        }
    }
}
