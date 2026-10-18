using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Verkostungsautomat (GDD 6.6), ab Plan 2: gegen Gebühr Siegel „STAATLICH GEPRÜFT“ (+Qualität) und ein Spruch
    /// je nach Qualität. Der Stempel bewegt sich (TastingFx).
    /// </summary>
    public class TastingMachine : Interactable
    {
        Chamber chamber;

        Chamber Chamber => chamber != null ? chamber : chamber = FindAnyObjectByType<Chamber>();

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var t = Cfg.machinesFixed.tasting;
            if (State.planIdx < t.unlockPlanIdx)
            {
                text = Texts.Format("ui.prompts.tastingLocked", ("n", t.unlockPlanIdx + 1));
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            var c = Chamber;
            if (c != null && c.State == AnimalState.Ok && c.Stats.stamped)
            {
                text = Texts.Get("ui.prompts.tastingDone");
                canUse = false;
                holdTime = DisabledHold;
                return;
            }
            text = Texts.Format("ui.prompts.tasting", ("cost", t.cost), ("bonus", t.qualityBonus));
            canUse = c != null && c.State == AnimalState.Ok;
            holdTime = canUse ? Cfg.interaction.hold.tasting : DisabledHold;
        }

        public override void Execute()
        {
            var c = Chamber;
            if (c == null) return;
            if (!StationLogic.Taste(State, Cfg, c.Stats, c.State == AnimalState.Ok).Report(GM)) return;
            float q = c.Stats.Quality(Cfg);
            GM.Toast("ui.toasts.tasting", ToastKind.Good, ("text", Texts.Pick(StationLogic.TastingList(q, Cfg))),
                ("n", Mathf.RoundToInt(Cfg.machinesFixed.tasting.qualityBonus)));
            EventBus.RaiseStation("tasting");
        }
    }
}
