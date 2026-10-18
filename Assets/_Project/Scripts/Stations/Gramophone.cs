using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Grammophon (GDD 6.5): Kurbel halten → Volksmusik. Stabilität +15, Hitze sinkt, Parteikontrolle und
    /// Kommissar-Verdacht wirken schwächer. Darstellung in GramophoneFx.
    /// </summary>
    public class Gramophone : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            if (StationLogic.MusicPlaying(State))
            {
                text = Texts.Format("ui.prompts.gramophonePlaying", ("sec", Mathf.CeilToInt(State.musicLeft)));
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            text = Texts.Get("ui.prompts.gramophoneCrank");
            canUse = true;
            holdTime = Cfg.machinesFixed.gramophone.crankSec;
        }

        public override void Execute()
        {
            if (!StationLogic.Crank(State, Cfg).Report(GM)) return;
            GM.Toast("ui.toasts.gramophone", ToastKind.Good);
            EventBus.RaiseStation("gramophone");
            if (Random.value < Cfg.machinesFixed.gramophone.directorCommentChance)
            {
                var director = FindAnyObjectByType<Director>();
                if (director != null) director.SayAny("director.gramophone");
            }
        }
    }
}
