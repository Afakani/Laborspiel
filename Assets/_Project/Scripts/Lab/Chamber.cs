using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kammer B-12: Schnittstelle für alle Pult-Aktionen (GDD 6.1) und Statuslampen.
    /// Phase 4a: Prüfungen und Meldungen wie im Prototyp, Reinigung funktioniert;
    /// das Tier selbst (Klonen, Sprühen, Produktion, Verarbeitung) folgt in Phase 4b.
    /// </summary>
    public class Chamber : MonoBehaviour
    {
        GameManager GM => GameManager.Instance;
        GameState S => GM.State;
        GameConfig Cfg => GM.Config;

        // ---------- Tierzustand (Phase 4b füllt diese Werte) ----------
        public AnimalState State { get; private set; } = AnimalState.Empty;
        public AnimalKind Kind { get; private set; }
        public int ExperimentNo { get; private set; }
        public float Stability { get; private set; } = 100f;
        public float UnstableTimer { get; private set; }
        public float Heat { get; private set; }
        public float FloatHeight { get; private set; }
        public bool InCrisis { get; private set; }
        public bool IsUltimate { get; private set; }

        public bool HasLivingAnimal => State == AnimalState.Ok || State == AnimalState.Unstable;

        void Update()
        {
            if (S.cleanTimer > 0f)
            {
                S.cleanTimer -= GM.Clock.DeltaTime;
                if (S.cleanTimer <= 0f)
                {
                    S.cleanTimer = 0f;
                    S.contaminated = false;
                    GM.Toast("ui.toasts.cleaned", ToastKind.Good);
                }
            }
        }

        // ---------- Aktionen ----------

        public Feedback Clone(AnimalKind kind)
        {
            if (State != AnimalState.Empty) return Feedback.Error("ui.errors.chamberBusy");
            if (S.beltBroken) return Feedback.Error("ui.errors.beltBrokenRepair");
            if (S.contaminated) return Feedback.Error("ui.errors.contaminated");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutClone");
            EventBus.RaiseLeverPulled(1);
            Placeholder();
            return Feedback.Ok;
        }

        public Feedback Spray(int serumIndex)
        {
            if (serumIndex < 0 || serumIndex >= S.serums.Count) return Feedback.Error("ui.errors.noSerum");
            if (State != AnimalState.Ok) return Feedback.Error("ui.errors.noAnimalReady");
            if (S.chamberCovered) return Feedback.Error("ui.errors.covered");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutSpray");
            if (IsUltimate) return Feedback.Error("ui.errors.alreadyUltimate");
            EventBus.RaiseLeverPulled(0);
            Placeholder();
            return Feedback.Ok;
        }

        public Feedback SendToProcessing()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noProcAnimal");
            if (S.beltBroken) return Feedback.Error("ui.errors.beltBroken");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutBelt");
            EventBus.RaiseLeverPulled(2);
            Placeholder();
            return Feedback.Ok;
        }

        public Feedback NotStop()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noAnimal");
            Placeholder();
            return Feedback.Ok;
        }

        public Feedback AntidoteToAnimal()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noAnimal");
            if (S.antidote <= 0) return Feedback.Error("ui.errors.noAntidote");
            Placeholder();
            return Feedback.Ok;
        }

        public Feedback Leash()
        {
            if (HasLivingAnimal && FloatHeight > Cfg.pult.leashMinFloat) Placeholder();
            return Feedback.Ok;
        }

        public Feedback Clean()
        {
            if (S.contaminated && S.cleanTimer <= 0f)
            {
                S.cleanTimer = Cfg.pult.cleanSec;
                GM.Toast("ui.toasts.cleaning", ToastKind.Info);
            }
            return Feedback.Ok;
        }

        // ---------- Statuslampen (Prototyp statusState) ----------

        public LampState GasLamp => S.blackoutTimer > 0f ? LampState.Bad : S.contaminated ? LampState.Warn : LampState.Ok;
        public LampState TempLamp => Heat > Cfg.risks.heatCrisis ? LampState.Bad : Heat > Cfg.pult.statusTempWarn ? LampState.Warn : LampState.Ok;
        public LampState BeltLamp => S.beltBroken ? LampState.Bad : LampState.Ok;
        public LampState VitalLamp =>
            !HasLivingAnimal ? LampState.Off
            : State == AnimalState.Unstable || Stability < Cfg.pult.statusVitBad ? LampState.Bad
            : Stability < Cfg.risks.activeBelowStab || InCrisis ? LampState.Warn
            : LampState.Ok;

        void Placeholder() => GM.Toast("ui.toasts.placeholder", ToastKind.Info, ("name", GM.Texts.Get("ui.names.chamber")), ("phase", "4b"));
    }
}
