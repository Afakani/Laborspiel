using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Grammophon als Propaganda-Radio (D154): Kurbel halten → Marsch-Polka für musicSec, dazu alle lineEverySec eine
    /// Durchsage als Untertitel (radio.lines). Reine Stimmung ohne Spielwirkung. Darstellung in GramophoneFx.
    /// </summary>
    public class Gramophone : Interactable
    {
        float lineTimer;

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
            lineTimer = 0f;
            EventBus.RaiseStation("gramophone");
            if (Random.value < Cfg.machinesFixed.gramophone.directorCommentChance)
            {
                var director = FindAnyObjectByType<Director>();
                if (director != null) director.SayAny("director.gramophone");
                lineTimer = Cfg.machinesFixed.gramophone.lineEverySec * 0.5f;
            }
        }

        /// <summary>Durchsagen, solange die Musik läuft.</summary>
        void Update()
        {
            var gm = GM;
            if (gm == null || !StationLogic.MusicPlaying(gm.State)) return;
            lineTimer -= gm.Clock.DeltaTime;
            if (lineTimer > 0f) return;
            lineTimer = gm.Config.machinesFixed.gramophone.lineEverySec;
            EventBus.RaiseRadioSpeak(gm.Texts.Pick("radio.lines"));
        }
    }
}
