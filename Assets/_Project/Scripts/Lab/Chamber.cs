using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kammer B-12 mit dem Tier (GDD 5, Prototyp updateSubject/cloneAnimal/applySerum/finishProc).
    /// Zustände: Empty → Enter → Ok ↔ Unstable → Exit | Destroy | Flying → Empty.
    /// Risiken kommen aus RiskLogic; Effekte, Kamerawackeln und Infektion hören auf EventBus.Risk.
    /// </summary>
    public class Chamber : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        [SerializeField] AnimalVisuals cowModel;
        [SerializeField] AnimalVisuals henModel;
        [SerializeField] AnimalLabel label;
        [SerializeField] Light glowLight;
        [SerializeField] ChamberDoors doors;

        readonly AnimalStats stats = new AnimalStats();
        readonly List<RiskEvent> riskEvents = new List<RiskEvent>();
        AnimalVisuals model;
        float x, drop, pop = 1f, sink, destroyTimer;

        GameManager GM => GameManager.Instance;
        GameState S => GM.State;
        GameConfig Cfg => GM.Config;

        // ---------- Zustand nach außen ----------
        public AnimalState State { get; private set; } = AnimalState.Empty;
        public AnimalKind Kind => stats.kind;
        public int ExperimentNo { get; private set; }
        public float Stability => stats.stability;
        public float UnstableTimer => stats.unstableTimer;
        public float Heat => stats.heat;
        public float FloatHeight => stats.floatHeight;
        public bool InCrisis => stats.crisis != CrisisKind.None;
        public bool IsUltimate => stats.IsUltimate;
        public AnimalStats Stats => stats;
        /// <summary>Sichtbares Tiermodell (für Effekte, die dem Tier folgen) oder null.</summary>
        public Transform ModelTransform => model != null ? model.transform : null;
        public bool HasLivingAnimal => State == AnimalState.Ok || State == AnimalState.Unstable;
        /// <summary>Lebt ein mutiertes Tier in der Kammer (für Parteikontrolle, Kommissar)?</summary>
        public bool HasMutatedAnimal => HasLivingAnimal && stats.IsMutated;

        /// <summary>Glühen des Tiers 0..1 (für die Parteikontrolle).</summary>
        public float Glow => model != null && HasLivingAnimal ? model.Glow : 0f;

        /// <summary>Sicht von Parteikontrolle und Kommissar auf das Tier.</summary>
        public ChamberView View => new ChamberView
        {
            mutated = HasMutatedAnimal,
            ok = State == AnimalState.Ok,
            unstable = State == AnimalState.Unstable,
            stability = stats.stability,
            floatHeight = stats.floatHeight,
            glow = Glow,
            ultimateId = stats.ultimate != null ? stats.ultimate.id : null,
            funId = stats.fun != null ? stats.fun.id : null,
            doseTotal = stats.dose.Total
        };

        public void SetModels(AnimalVisuals cow, AnimalVisuals hen, AnimalLabel lbl, Light light)
        {
            cowModel = cow;
            henModel = hen;
            label = lbl;
            glowLight = light;
        }

        /// <summary>Hubtüren (Look-Pass). Ohne Türen läuft das Tier wie im Prototyp direkt durch.</summary>
        public void SetDoors(ChamberDoors d) => doors = d;

        void Start()
        {
            cowModel.gameObject.SetActive(false);
            henModel.gameObject.SetActive(false);
            label.Show(false);
            glowLight.intensity = 0f;
        }

        // ---------- Ablauf ----------

        void Update()
        {
            float dt = GM.Clock.DeltaTime;
            UpdateCleaning(dt);
            UpdateBlackout(dt);
            S.currentRate = 0f;
            UpdateDoorWishes();
            if (dt <= 0f || State == AnimalState.Empty || model == null) return;

            var a = Cfg.animal;
            switch (State)
            {
                case AnimalState.Enter:
                    drop = Mathf.Max(0f, drop - dt * a.enterDropSpeed);
                    x = HoldAtDoor(x, Mathf.Min(CenterX, x + dt * a.enterSpeed), LeftDoorX, doors == null || doors.LeftOpen >= 1f);
                    if (x >= CenterX && drop <= 0f) SetState(AnimalState.Ok);
                    break;
                case AnimalState.Exit:
                    x = HoldAtDoor(x, x + dt * a.exitSpeed, RightDoorX, doors == null || doors.RightOpen >= 1f);
                    if (x >= ProcX - a.procStopBefore)
                    {
                        pop = Mathf.Max(0f, pop - dt * a.popOutSpeed);
                        if (pop <= 0.03f) { FinishProcessing(); return; }
                    }
                    break;
                case AnimalState.Destroy:
                    destroyTimer -= dt;
                    sink += dt * a.sinkSpeed;
                    if (destroyTimer <= 0f) { Empty(); return; }
                    break;
                case AnimalState.Flying:
                    stats.floatHeight += a.flyRiseSpeed * dt;
                    x += a.flyDriftSpeed * dt;
                    if (stats.floatHeight > a.flyGoneHeight) { Empty(); return; }
                    break;
                default:
                    UpdateLiving(dt);
                    if (State == AnimalState.Empty) return;
                    break;
            }
            if (State != AnimalState.Exit) pop = Mathf.Min(1f, pop + dt * a.popInSpeed);
            UpdateVisuals(dt);
        }

        // ---------- Hubtüren ----------

        float DoorX => Cfg.layout.chamber.radius + Cfg.chamberDoors.radialOffset;
        float LeftDoorX => CenterX - DoorX;
        float RightDoorX => CenterX + DoorX;
        float SizeK => model != null ? model.transform.localScale.x / (stats.kind == AnimalKind.Cow ? 1f : Cfg.animalVisuals.henScale) : 1f;
        float WaitGap => (stats.kind == AnimalKind.Cow ? Cfg.chamberDoors.waitGapCow : Cfg.chamberDoors.waitGapHen) * SizeK;
        float ClearGap => (stats.kind == AnimalKind.Cow ? Cfg.chamberDoors.clearGapCow : Cfg.chamberDoors.clearGapHen) * SizeK;

        /// <summary>Links auf zum Einlass, rechts auf zum Auslass; zu, sobald das Tier ganz hindurch ist.</summary>
        void UpdateDoorWishes()
        {
            if (doors == null) return;
            doors.WantLeft = State == AnimalState.Enter && x < LeftDoorX + ClearGap;
            doors.WantRight = State == AnimalState.Exit && x < RightDoorX + ClearGap;
        }

        /// <summary>Vor einer noch nicht ganz offenen Tür bleibt das Tier stehen; wer schon hindurch ist, läuft weiter.</summary>
        float HoldAtDoor(float from, float to, float doorX, bool open)
        {
            if (open || from > doorX) return to;
            float stop = doorX - WaitGap;
            return from >= stop ? from : Mathf.Min(to, stop);
        }

        void UpdateLiving(float dt)
        {
            bool music = false; // Grammophon folgt in Phase 9
            riskEvents.Clear();
            RiskLogic.Step(stats, State, S, Cfg, dt, music, RandomValue, riskEvents);
            foreach (var e in riskEvents)
            {
                HandleRisk(e);
                if (State == AnimalState.Flying || State == AnimalState.Empty) return;
            }

            if (State == AnimalState.Unstable)
            {
                stats.unstableTimer -= dt;
                stats.currentRate = 0f;
                if (stats.unstableTimer <= 0f) Explode();
                return;
            }

            float rate = stats.Rate(S.blackoutTimer > 0f, Cfg);
            stats.currentRate = rate;
            stats.maxRate = Mathf.Max(stats.maxRate, rate);
            S.currentRate = rate;
            Production.Deliver(S, Cfg, stats.Produces(Cfg), rate * dt, stats.Quality(Cfg));

            // Eier zählen (Münzhuhn: Stalinmünzen pro Ei)
            if (stats.kind == AnimalKind.Hen && stats.Produces(Cfg) != Cfg.animals.cow.produces[0])
            {
                stats.eggTimer -= dt * rate;
                if (stats.eggTimer <= 0f)
                {
                    stats.eggTimer = Cfg.animal.eggInterval;
                    if (stats.fun != null && stats.fun.coinsPerEgg > 0f) S.AddMoney(stats.fun.coinsPerEgg);
                }
            }
        }

        /// <summary>Risiko-Ereignis in Meldung und Zustand übersetzen, dann an Effekte weitergeben.</summary>
        void HandleRisk(RiskEvent e)
        {
            switch (e)
            {
                case RiskEvent.WarnHeat: GM.Toast("ui.toasts.warnHeat", ToastKind.Warn); break;
                case RiskEvent.WarnAcid: GM.Toast("ui.toasts.warnAcid", ToastKind.Warn); break;
                case RiskEvent.WarnFloat: GM.Toast("ui.toasts.warnFloat", ToastKind.Warn); break;
                case RiskEvent.WarnStab: GM.Toast("ui.toasts.warnStab", ToastKind.Warn); break;
                case RiskEvent.AcidMilk: GM.Toast("ui.toasts.acidMilk", ToastKind.Bad); break;
                case RiskEvent.BeltBroken: GM.Toast("ui.toasts.beltBroken", ToastKind.Bad); break;
                case RiskEvent.CableTorn: GM.Toast("ui.toasts.cableTorn", ToastKind.Bad); break;
                case RiskEvent.FlyAway: FlyAway(); break;
                case RiskEvent.Unstable:
                    SetState(AnimalState.Unstable);
                    GM.Toast("ui.toasts.unstable", ToastKind.Bad);
                    break;
            }
            EventBus.RaiseRisk(e);
        }

        /// <summary>Stromausfall läuft ab (GDD 5.2: 12 s).</summary>
        void UpdateBlackout(float dt)
        {
            if (S.blackoutTimer <= 0f) return;
            S.blackoutTimer -= dt;
            if (S.blackoutTimer > 0f) return;
            S.blackoutTimer = 0f;
            GM.Toast("ui.toasts.powerBack", ToastKind.Good);
            EventBus.RaiseRisk(RiskEvent.PowerBack);
        }

        static float RandomValue() => Random.value;

        void UpdateCleaning(float dt)
        {
            if (S.cleanTimer <= 0f) return;
            S.cleanTimer -= dt;
            if (S.cleanTimer > 0f) return;
            S.cleanTimer = 0f;
            S.contaminated = false;
            GM.Toast("ui.toasts.cleaned", ToastKind.Good);
        }

        void UpdateVisuals(float dt)
        {
            var a = Cfg.animal;
            var v = Cfg.animalVisuals;
            float t = GM.Clock.Time;
            bool walk = State == AnimalState.Enter || State == AnimalState.Exit;
            model.Apply(stats, State, walk, pop, dt, t, Cfg);

            float bob = stats.floatHeight > v.floatBobFrom ? v.floatBobAmp * Mathf.Sin(t * v.floatBobFreq) : 0f;
            float y = a.standY + stats.floatHeight + bob + (State == AnimalState.Enter ? drop : 0f) - (State == AnimalState.Destroy ? sink : 0f);
            model.transform.localPosition = new Vector3(x - CenterX, y, 0f);

            glowLight.color = model.GlowColor;
            glowLight.intensity = model.Glow * v.lightFactor * model.Pulse;
            glowLight.transform.localPosition = new Vector3(x - CenterX, y + v.lightOffsetY, v.lightOffsetZ);

            UpdateLabel(y);
        }

        void UpdateLabel(float y)
        {
            var a = Cfg.animal;
            var t = GM.Texts;
            bool visible = State != AnimalState.Destroy && State != AnimalState.Flying && !(S.chamberCovered && Mathf.Abs(x - CenterX) < Cfg.layout.chamber.radius);
            label.Show(visible);
            if (!visible) return;

            float grow = model.transform.localScale.y / (stats.kind == AnimalKind.Cow ? 1f : Cfg.animalVisuals.henScale);
            label.transform.localPosition = new Vector3(x - CenterX, y + (stats.kind == AnimalKind.Cow ? a.labelHeightCow : a.labelHeightHen) * grow, 0f);

            bool unstable = State == AnimalState.Unstable;
            int stab = Formula.RoundHalfUp(stats.stability);
            int sec = Mathf.CeilToInt(stats.unstableTimer);
            string key = $"{stats.kind}|{stats.dose.Key}|{stats.ultimate?.id}|{stats.fun?.id}|{stats.stamped}|{State}|{stats.crisis}|{stab / 5}|{(unstable ? sec : 0)}";

            string top;
            Color topColor = Color.white;
            Color? frame = null;
            if (stats.IsUltimate)
            {
                top = stats.ultimate.displayName;
                topColor = stats.ultimate.Color;
                frame = topColor;
            }
            else if (stats.fun != null)
            {
                top = stats.fun.displayName;
                ColorUtility.TryParseHtmlString(Cfg.animalVisuals.funLabelColor, out topColor);
            }
            else top = DoseLine(stats.dose);

            string status = unstable ? t.Format("ui.animalLabel.unstable", ("sec", sec))
                : stats.crisis == CrisisKind.Overheat ? t.Get("ui.animalLabel.overheated")
                : stats.crisis == CrisisKind.Acid ? t.Get("ui.animalLabel.acid")
                : stats.crisis == CrisisKind.Escape ? t.Get("ui.animalLabel.escape")
                : t.Format("ui.animalLabel.stability", ("n", stab));
            label.Set(key, top, topColor, status, unstable || stats.crisis != CrisisKind.None, frame, stats.stamped, t.Get("ui.animalLabel.stamp"));
        }

        string DoseLine(Dose d)
        {
            var t = GM.Texts;
            var parts = new string[3];
            for (int i = 0; i < 3; i++)
            {
                var sub = Cfg.substances[i];
                parts[i] = t.Format("ui.animalLabel.dose", ("color", sub.color.TrimStart('#')), ("id", sub.id), ("n", d[i]));
            }
            return string.Join("  ", parts);
        }

        // ---------- Aktionen am Pult ----------

        public Feedback Clone(AnimalKind kind)
        {
            if (State != AnimalState.Empty) return Feedback.Error("ui.errors.chamberBusy");
            if (S.beltBroken) return Feedback.Error("ui.errors.beltBrokenRepair");
            if (S.contaminated) return Feedback.Error("ui.errors.contaminated");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutClone");

            stats.Reset(Cfg.animal.eggInterval);
            stats.kind = kind;
            model = kind == AnimalKind.Cow ? cowModel : henModel;
            cowModel.gameObject.SetActive(kind == AnimalKind.Cow);
            henModel.gameObject.SetActive(kind == AnimalKind.Hen);
            model.ResetLook();
            x = CloneX;
            pop = 0f;
            drop = Cfg.animal.enterDropHeight;
            sink = 0f;
            S.experiments++;
            ExperimentNo = S.experiments;
            SetState(AnimalState.Enter);
            EventBus.RaiseLeverPulled(1);
            GM.Toast("ui.toasts.cloned", ToastKind.Good,
                ("kind", GM.Texts.Get(kind == AnimalKind.Cow ? "ui.common.kindCow" : "ui.common.kindHen")),
                ("id", GM.Texts.Format("ui.common.animalId", ("n", ExperimentNo.ToString("00")))));
            UpdateVisuals(0f);
            return Feedback.Ok;
        }

        public Feedback Spray(int serumIndex)
        {
            if (serumIndex < 0 || serumIndex >= S.serums.Count) return Feedback.Error("ui.errors.noSerum");
            if (State != AnimalState.Ok) return Feedback.Error("ui.errors.noAnimalReady");
            if (doors != null && !doors.BothClosed) return Feedback.Error("ui.errors.doorsMoving");
            if (S.chamberCovered) return Feedback.Error("ui.errors.covered");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutSpray");
            if (IsUltimate) return Feedback.Error("ui.errors.alreadyUltimate");

            var serum = S.serums[serumIndex];
            S.serums.RemoveAt(serumIndex);
            S.selectedSerum = Mathf.Min(S.selectedSerum, Mathf.Max(0, S.serums.Count - 1));
            S.lastSprayed = serum;
            EventBus.RaiseLeverPulled(0);
            EventBus.RaiseSerumSprayed(SerumTexts.Color(serum, Cfg));

            if (serum.IsUltimate)
            {
                var u = Cfg.UltimateById(serum.ultimateId);
                var kind = stats.kind;
                stats.Reset(Cfg.animal.eggInterval);
                stats.kind = kind;
                stats.ultimate = u;
                stats.dose = u.Recipe;
                S.discoveredRecipes.Add(SerumTexts.RecipeKey(serum));
                GM.Toast("ui.toasts.ultimate", ToastKind.Good, ("name", u.displayName));
                bool first = !S.ultimateSeen;
                S.ultimateSeen = true; // Zwischensequenz folgt in Phase 8
                if (first && InspectionLogic.ScheduleBoss(S, Cfg)) GM.Toast("ui.toasts.bossIncoming", ToastKind.Bad);
            }
            else
            {
                stats.dose += serum.dose;
                if (serum.IsConcentrated) stats.conc = Mathf.Max(stats.conc, serum.conc);
                if (serum.frozen) stats.stabBonus += Cfg.formulas.frozenStabBonus;

                var fun = Cfg.FunRecipeFor(stats.kind, stats.dose);
                var before = stats.fun;
                stats.fun = fun;
                if (fun != null && fun != before)
                {
                    S.foundFunRecipes.Add(fun.id);
                    GM.Toast("ui.toasts.curiosity", ToastKind.Good, ("name", fun.displayName), ("pro", fun.pros.Length > 0 ? fun.pros[0] : ""));
                }

                string key = serum.dose.Key;
                bool first = S.discoveredRecipes.Add(key);
                float stab = Formula.Stability(stats.dose, Cfg);
                var a = Cfg.animal;
                var kindToast = stab < a.sprayToastBad ? ToastKind.Bad : stab < a.sprayToastWarn ? ToastKind.Warn : ToastKind.Good;
                GM.Toast(first ? "ui.toasts.sprayGasNew" : "ui.toasts.sprayGas", kindToast,
                    ("name", Formula.ComboName(serum.dose, Cfg, GM.Texts)), ("stab", Formula.RoundHalfUp(stab)));
            }
            S.totalDoses += serum.dose.Total;
            EventBus.RaiseSerumRackChanged();
            return Feedback.Ok;
        }

        public Feedback SendToProcessing()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noProcAnimal");
            if (S.beltBroken) return Feedback.Error("ui.errors.beltBroken");
            if (S.blackoutTimer > 0f) return Feedback.Error("ui.errors.blackoutBelt");
            stats.brave = State == AnimalState.Unstable || stats.minStability < Cfg.processing.braveMinStab;
            if (stats.brave) GM.Toast("ui.toasts.braveStart", ToastKind.Warn);
            SetState(AnimalState.Exit);
            EventBus.RaiseLeverPulled(2);
            return Feedback.Ok;
        }

        void FinishProcessing()
        {
            var p = Cfg.processing;
            // Mut-Bonus (GDD 5.6): instabil oder fast instabil gewesen → Explosion möglich, sonst Bonus
            if (stats.brave && Random.value < p.braveExplodeChance)
            {
                S.explosions++;
                S.AddAnger(Cfg.anger.explode, Cfg);
                GM.Toast("ui.toasts.procExploded", ToastKind.Bad);
                EventBus.RaiseRisk(RiskEvent.ProcessingExplosion);
                Empty();
                return;
            }
            float meat = stats.Meat(Cfg);
            float quality = stats.Quality(Cfg);
            Production.Deliver(S, Cfg, 'f', meat, quality);
            DayLogic.OnProcessed(S, quality);
            GM.Toast("ui.toasts.processed", ToastKind.Good, ("n", meat.ToString("0.0", German)));
            if (stats.brave)
            {
                S.AddMoney(p.braveBonus);
                GM.Toast("ui.toasts.braveBonus", ToastKind.Good, ("money", GM.Texts.Format("ui.common.money", ("n", p.braveBonus))));
            }
            if (stats.ultimate != null && stats.ultimate.id == UltimateIds.Koloss && Random.value < p.kolossBeltBreakChance)
            {
                S.beltBroken = true;
                GM.Toast("ui.toasts.kolossBelt", ToastKind.Bad);
                EventBus.RaiseRisk(RiskEvent.BeltBroken);
            }
            Empty();
        }

        public Feedback NotStop()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noAnimal");
            destroyTimer = Cfg.animal.destroySec;
            sink = 0f;
            SetState(AnimalState.Destroy);
            GM.Toast("ui.toasts.notStop", ToastKind.Warn);
            return Feedback.Ok;
        }

        public Feedback AntidoteToAnimal()
        {
            if (!HasLivingAnimal) return Feedback.Error("ui.errors.noAnimal");
            if (S.antidote <= 0) return Feedback.Error("ui.errors.noAntidote");
            S.antidote--;
            S.NotifyInventoryChanged();
            var kind = stats.kind;
            stats.Reset(Cfg.animal.eggInterval);
            stats.kind = kind;
            SetState(AnimalState.Ok);
            GM.Toast("ui.toasts.antidoteAnimal", ToastKind.Good);
            return Feedback.Ok;
        }

        public Feedback Leash()
        {
            if (HasLivingAnimal && stats.floatHeight > Cfg.pult.leashMinFloat)
            {
                stats.floatHeight = Cfg.risks.leashTo;
                GM.Toast("ui.toasts.leashed", ToastKind.Good);
            }
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

        /// <summary>
        /// Explosion (GDD 5.4): Kammer verseucht, Laken zerfetzt, Parteikontrolle und Wut steigen.
        /// Effekte, Flecken, Kamerawackeln und die Infektion des Spielers hängen an EventBus.Explosion.
        /// </summary>
        public void Explode()
        {
            S.explosions++;
            S.contaminated = true;
            S.AddAuthority(Cfg.authority.explodeAdd, Cfg);
            S.AddAnger(Cfg.anger.explode, Cfg);
            if (S.chamberCovered)
            {
                S.chamberCovered = false;
                EventBus.RaiseClothChanged(false);
                GM.Toast("ui.toasts.clothTorn", ToastKind.Bad);
            }
            GM.Toast("ui.toasts.explosion", ToastKind.Bad);
            EventBus.RaiseRisk(RiskEvent.Explosion);
            EventBus.RaiseExplosion();
            Empty();
        }

        /// <summary>Tier schwebt davon (GDD 5.2), ausgelöst vom Risiko-System.</summary>
        public void FlyAway()
        {
            S.AddMoney(-Cfg.risks.escapeCost);
            GM.Toast("ui.toasts.flyAway", ToastKind.Bad, ("money", GM.Texts.Format("ui.common.money", ("n", Cfg.risks.escapeCost))));
            SetState(AnimalState.Flying);
        }

        /// <summary>Neues Spiel: Kammer sofort leeren.</summary>
        public void Clear() => Empty();

        void Empty()
        {
            SetState(AnimalState.Empty);
            stats.Reset(Cfg.animal.eggInterval);
            cowModel.gameObject.SetActive(false);
            henModel.gameObject.SetActive(false);
            model = null;
            label.Show(false);
            glowLight.intensity = 0f;
        }

        void SetState(AnimalState s)
        {
            if (State == s) return;
            State = s;
            EventBus.RaiseAnimalStateChanged(s);
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

        float CenterX => Cfg.layout.chamber.x;
        float CloneX => Cfg.layout.Station("clone").x;
        float ProcX => Cfg.layout.Station("processing").x;
    }
}
