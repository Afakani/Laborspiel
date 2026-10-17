using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Lässt den Mischvorgang in Spielzeit laufen und meldet das fertige Serum (Toast, Events).</summary>
    public class MixingSystem : MonoBehaviour
    {
        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var serum = MixingLogic.Tick(gm.State, gm.Config, gm.Clock.DeltaTime);
            if (serum == null) return;

            string name = SerumTexts.Name(serum, gm.Config, gm.Texts, gm.State);
            gm.Toast(serum.stage == SerumStage.Raw ? "ui.toasts.mixedRaw" : "ui.toasts.mixed", ToastKind.Good, ("name", name));
            EventBus.RaiseSerumMixed(serum);
            EventBus.RaiseSerumRackChanged();
        }

        /// <summary>Mischknopf: startet oder meldet den Grund, warum es nicht geht.</summary>
        public static bool StartMix()
        {
            var gm = GameManager.Instance;
            var fb = MixingLogic.TryStartMix(gm.State, gm.Config);
            if (!fb.Report(gm)) return false;
            EventBus.RaiseInventoryChanged();
            return true;
        }
    }
}
