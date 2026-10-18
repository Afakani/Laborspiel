using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Zentrifuge (GDD 6.3): Serum laden, Drehzahl steigt, im richtigen Moment bremsen (Drehzahlbalken im Prompt).
    /// Regeln in MachineLogic, Darstellung in CentrifugeFx.
    /// </summary>
    public class Centrifuge : Interactable
    {
        /// <summary>Dreht gerade hoch (Prompt zeigt den Drehzahlbalken)?</summary>
        public bool Spinning => State.centrifuge != null && State.centrifuge.spinning;

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var job = State.centrifuge;
            var hold = Cfg.interaction.hold;
            if (job != null && job.spinning)
            {
                text = Texts.Format("ui.prompts.centrifugeBrake", ("n", Mathf.RoundToInt(job.rpm * 100f)));
                canUse = true;
                holdTime = hold.centrifugeBrake;
                return;
            }
            if (job != null)
            {
                text = Texts.Get(State.serums.Count >= Cfg.mixing.rackSize ? "ui.prompts.centrifugeRackFull" : "ui.prompts.centrifugeSpinDown");
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            canUse = MachineLogic.FindForCentrifuge(State) >= 0;
            text = Texts.Get(canUse ? "ui.prompts.centrifugeLoad" : "ui.prompts.centrifugeNone");
            holdTime = canUse ? hold.centrifugeLoad : DisabledHold;
        }

        public override void Execute()
        {
            if (State.centrifuge != null)
            {
                if (!State.centrifuge.spinning) return;
                Report(MachineLogic.Brake(State, Cfg));
                return;
            }
            if (!MachineLogic.LoadCentrifuge(State).Report(GM)) return;
            EventBus.RaiseSerumRackChanged();
            GM.Toast("ui.toasts.centrifugeRunning", ToastKind.Warn);
        }

        void Report(CentrifugeResult r)
        {
            switch (r)
            {
                case CentrifugeResult.TooEarly: GM.Toast("ui.toasts.centrifugeTooEarly", ToastKind.Warn); break;
                case CentrifugeResult.Centrifuged: GM.Toast("ui.toasts.centrifugeDone", ToastKind.Good); break;
                case CentrifugeResult.CentrifugedPerfect: GM.Toast("ui.toasts.centrifugePerfect", ToastKind.Good); break;
                case CentrifugeResult.Concentrated: GM.Toast("ui.toasts.concNormal", ToastKind.Good); break;
                case CentrifugeResult.ConcentratedPerfect: GM.Toast("ui.toasts.concPerfect", ToastKind.Good); break;
            }
        }
    }
}
