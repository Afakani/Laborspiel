using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Laken über der Kammer (GDD 6.8): seitlich an der Kammer halten zum Werfen/Abnehmen. Jedes Überwerfen
    /// verbraucht ein Laken aus dem Lager, beim Abnehmen ist es zerfressen (ClothLogic).
    /// Abgedeckt: kein Sprühen, Parteikontrolle ×0,4, Kommissar-Verdacht stark reduziert. Darstellung: ClothVisual.
    /// </summary>
    public class Cloth : Interactable
    {
        public override string Key => "cloth";

        public override bool TryGetScore(Vector2 p, out float score)
        {
            score = Cfg.interaction.clothScore;
            return InAbsZone(p, Cfg.layout.clothZone);
        }

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var s = State;
            text = s.chamberCovered ? Texts.Get("ui.prompts.clothOff")
                : s.cloths > 0 ? Texts.Format("ui.prompts.clothOn", ("n", s.cloths)) : Texts.Get("ui.prompts.clothNone");
            canUse = s.chamberCovered || s.cloths > 0;
            holdTime = Cfg.interaction.hold.cloth;
        }

        public override void Execute()
        {
            var r = ClothLogic.Toggle(State);
            if (r == ClothLogic.Result.NoCloth)
            {
                GM.Toast("ui.errors.noCloth", ToastKind.Warn);
                return;
            }
            EventBus.RaiseClothChanged(State.chamberCovered);
            State.NotifyInventoryChanged();
            if (r == ClothLogic.Result.Covered) GM.Toast("ui.toasts.clothOn", ToastKind.Good, ("n", State.cloths));
            else GM.Toast("ui.toasts.clothGone", ToastKind.Info);
        }
    }
}
