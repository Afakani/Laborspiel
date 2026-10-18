using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kryokammer (GDD 6.4), ab Plan 2: Serum einfrieren, im Zeitfenster herausnehmen, sonst Eisblock.
    /// Ab Plan 3 wird zentrifugiertes Rohserum ultimativ. Regeln in MachineLogic, Darstellung in CryoFx.
    /// </summary>
    public class CryoChamber : Interactable
    {
        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var c = Cfg.machinesFixed.cryo;
            var hold = Cfg.interaction.hold;
            holdTime = DisabledHold;
            canUse = false;
            if (State.planIdx < c.unlockPlanIdx)
            {
                text = Texts.Format("ui.prompts.cryoLocked", ("n", c.unlockPlanIdx + 1));
                return;
            }
            var job = State.cryo;
            if (job != null && !job.Ready)
            {
                text = Texts.Format("ui.prompts.cryoFreezing", ("sec", Mathf.CeilToInt(job.freezeLeft)));
                return;
            }
            if (job != null)
            {
                bool full = State.serums.Count >= Cfg.mixing.rackSize;
                text = Texts.Format(full ? "ui.prompts.cryoRackFull" : "ui.prompts.cryoTake", ("sec", Mathf.CeilToInt(job.windowLeft)));
                canUse = !full;
                if (canUse) holdTime = hold.cryoTake;
                return;
            }
            canUse = MachineLogic.FindForCryo(State) >= 0;
            text = Texts.Get(canUse ? "ui.prompts.cryoLoad" : "ui.prompts.cryoNone");
            if (canUse) holdTime = hold.cryoLoad;
        }

        public override void Execute()
        {
            if (State.cryo != null)
            {
                if (!MachineLogic.TakeCryo(State, Cfg, out var result).Report(GM)) return;
                EventBus.RaiseSerumRackChanged();
                var serum = State.serums[State.serums.Count - 1];
                switch (result)
                {
                    case CryoResult.Ultimate:
                        GM.Toast("ui.toasts.ultimateSerum", ToastKind.Good, ("name", Cfg.UltimateById(serum.ultimateId).displayName));
                        break;
                    case CryoResult.NoReaction:
                        GM.Toast("ui.toasts.cryoNoReaction", ToastKind.Warn, ("n", Cfg.machinesFixed.ultimate.unlockPlanIdx + 1));
                        break;
                    default:
                        GM.Toast("ui.toasts.cryoFrozen", ToastKind.Good);
                        break;
                }
                return;
            }
            if (!MachineLogic.LoadCryo(State, Cfg).Report(GM)) return;
            EventBus.RaiseSerumRackChanged();
            GM.Toast("ui.toasts.cryoFreezing", ToastKind.Good, ("name", SerumTexts.Name(State.cryo.serum, Cfg, Texts, State)));
        }
    }
}
