using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Infektion des Spielers in der Szene (GDD 8): Ansteckung bei Explosionen, Symptome als Steuerungs- und
    /// Post-Processing-Effekte (grün, schnell, bleischwer, verschwommen, schwankend, invertiert), Heilung mit G.
    /// Die Regeln stehen in InfectionLogic.
    /// </summary>
    [RequireComponent(typeof(FirstPersonController))]
    public class InfectionSystem : MonoBehaviour
    {
        readonly InfectionLogic.Symptoms symptoms = new InfectionLogic.Symptoms();
        FirstPersonController fpc;
        float greenW, blurW;

        GameManager GM => GameManager.Instance;

        void Awake() => fpc = GetComponent<FirstPersonController>();
        void OnEnable() => EventBus.Explosion += OnExplosion;
        void OnDisable() => EventBus.Explosion -= OnExplosion;

        void OnExplosion()
        {
            var gm = GM;
            var c = gm.Config.layout.chamber;
            var center = LayoutConfig.ToUnity(c.x, 0f, c.z);
            var p = transform.position;
            float dist = new Vector2(p.x - center.x, p.z - center.z).magnitude;
            switch (InfectionLogic.OnExplosion(gm.State, gm.Config, dist, Random.value, symptoms))
            {
                case InfectionLogic.InfectResult.Infected:
                    gm.Toast("ui.toasts.infected", ToastKind.Bad, ("n", gm.State.infectedUntilDay));
                    EventBus.RaiseInfectionChanged(true);
                    break;
                case InfectionLogic.InfectResult.More:
                    gm.Toast("ui.toasts.infectedMore", ToastKind.Bad);
                    break;
                case InfectionLogic.InfectResult.Missed:
                    gm.Toast("ui.toasts.infectionMissed", ToastKind.Warn);
                    break;
            }
        }

        /// <summary>Für den Tagesablauf (Phase 6): Frist prüfen; true = Zusammenbruch.</summary>
        public bool CheckCollapse()
        {
            var gm = GM;
            if (!InfectionLogic.TryCollapse(gm.State, gm.Config, symptoms)) return false;
            gm.Toast("ui.toasts.collapse", ToastKind.Bad);
            EventBus.RaiseInfectionChanged(false);
            return true;
        }

        /// <summary>Für Debug und Tests: sofort anstecken (ohne Tag- und Abstandsprüfung).</summary>
        public void ForceInfect()
        {
            var gm = GM;
            int day = gm.State.dayNum;
            gm.State.dayNum = Mathf.Max(day, gm.Config.risks.infectionFromDay);
            InfectionLogic.OnExplosion(gm.State, gm.Config, 0f, 0f, symptoms);
            gm.State.dayNum = day;
            gm.Toast("ui.toasts.infected", ToastKind.Bad, ("n", gm.State.infectedUntilDay));
            EventBus.RaiseInfectionChanged(true);
        }

        void Update()
        {
            var gm = GM;
            if (gm == null) return;
            var s = gm.State;
            float dt = gm.Clock.DeltaTime;

            if (InputReader.AntidotePressed && gm.PlayerControlEnabled)
            {
                var fb = InfectionLogic.SelfInject(s, symptoms);
                if (fb.Report(gm))
                {
                    gm.Toast("ui.toasts.cured", ToastKind.Good);
                    s.NotifyInventoryChanged();
                    EventBus.RaiseInfectionChanged(false);
                }
            }

            string started = InfectionLogic.Tick(s, gm.Config, symptoms, dt, () => Random.value);
            if (started != null)
                gm.Toast("ui.toasts.symptom", ToastKind.Bad, ("text", gm.Texts.Get("symptoms." + started)));
            s.infectionSymptom = symptoms.current;
            Apply(gm, dt);
        }

        /// <summary>Symptom auf Steuerung und Bild übertragen (weich ein- und ausblenden).</summary>
        void Apply(GameManager gm, float dt)
        {
            var c = gm.Config.infection;
            string sym = gm.State.infected ? symptoms.current : null;
            fpc.SpeedMultiplier = sym == "fast" ? c.fast : sym == "slow" ? c.slow : 1f;
            fpc.InvertControls = sym == "invert";
            fpc.WobbleFreq = c.wobbleFreq;
            fpc.WobbleDeg = Mathf.MoveTowards(fpc.WobbleDeg, sym == "wobble" ? c.wobbleDeg : 0f, dt * c.wobbleDeg);
            greenW = Mathf.MoveTowards(greenW, sym == "green" ? 1f : 0f, dt);
            blurW = Mathf.MoveTowards(blurW, sym == "blur" ? 1f : 0f, dt);
            var vols = InfectionVolumes.Instance;
            if (vols != null) vols.Set(greenW, blurW);
        }
    }
}
