using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Mischstation (GDD 6.2). Ziel im Bereich davor; Wertung = seitlicher Abstand. Öffnet das Panel.
    /// Während einer Kontrolle mit verdächtigem (oder abgedecktem) Regal: Serum-Regal abdecken/aufdecken (GDD 9).
    /// </summary>
    public class MixingStation : Interactable
    {
        public override bool TryGetScore(Vector2 p, out float score)
        {
            var z = Cfg.interaction.mixingZone;
            score = Mathf.Abs(p.x - Layout.x);
            return p.x >= z.xMin && p.x < z.xMax && p.y >= z.zMin && p.y < z.zMax;
        }

        /// <summary>Statt des Panels das Regal abdecken bzw. wieder aufdecken?</summary>
        bool RackMode => State.inspection != null && (State.rackHidden || InspectionLogic.RackSuspicious(State, Cfg));

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            canUse = true;
            if (RackMode)
            {
                text = Texts.Get(State.rackHidden ? "ui.prompts.rackShow" : "ui.prompts.rackHide");
                holdTime = Cfg.interaction.hold.rackHide;
                return;
            }
            text = Texts.Get("ui.prompts.mix");
            holdTime = Cfg.interaction.hold.mixing;
        }

        public override void Execute()
        {
            if (!RackMode)
            {
                EventBus.RaisePanelRequested("mixing");
                return;
            }
            State.rackHidden = !State.rackHidden;
            GM.Toast(State.rackHidden ? "ui.toasts.rackHidden" : "ui.toasts.rackOpen", State.rackHidden ? ToastKind.Good : ToastKind.Info);
        }
    }
}
