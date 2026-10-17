using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Laken über der Kammer (GDD 6.8): seitlich an der Kammer halten zum Werfen/Abnehmen.
    /// Wirkung auf Parteikontrolle und Kommissar folgt in Phase 7.
    /// </summary>
    public class Cloth : Interactable
    {
        [SerializeField] GameObject clothVisual;

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

        protected override void OnEnable()
        {
            base.OnEnable();
            EventBus.ClothChanged += OnClothChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus.ClothChanged -= OnClothChanged;
        }

        void OnClothChanged(bool covered)
        {
            if (clothVisual != null) clothVisual.SetActive(covered);
        }

        public override void Execute()
        {
            State.chamberCovered = !State.chamberCovered;
            EventBus.RaiseClothChanged(State.chamberCovered);
            GM.Toast(State.chamberCovered ? "ui.toasts.clothOn" : "ui.toasts.clothOff", State.chamberCovered ? ToastKind.Good : ToastKind.Info);
        }

        /// <summary>Für Editor-Werkzeuge.</summary>
        public void SetVisual(GameObject visual) => clothVisual = visual;
    }
}
