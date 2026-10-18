using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Laken über der Kammer (GDD 6.8): seitlich an der Kammer halten zum Werfen/Abnehmen.
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
            text = Texts.Get(State.chamberCovered ? "ui.prompts.clothOff" : "ui.prompts.clothOn");
            canUse = true;
            holdTime = Cfg.interaction.hold.cloth;
        }

        public override void Execute()
        {
            State.chamberCovered = !State.chamberCovered;
            EventBus.RaiseClothChanged(State.chamberCovered);
            GM.Toast(State.chamberCovered ? "ui.toasts.clothOn" : "ui.toasts.clothOff", State.chamberCovered ? ToastKind.Good : ToastKind.Info);
        }
    }
}
