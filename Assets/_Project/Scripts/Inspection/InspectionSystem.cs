using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Parteikontrolle und Kommissar im Spiel (GDD 9): rechnet während der Schicht über InspectionLogic,
    /// zeigt Meldungen, lässt den Direktor reagieren und beendet das Spiel beim Skandal.
    /// </summary>
    public class InspectionSystem : MonoBehaviour
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        Chamber chamber;
        Director director;
        CameraShake shake;
        readonly List<InspectionEvent> events = new List<InspectionEvent>();

        GameManager GM => GameManager.Instance;

        void Awake()
        {
            chamber = FindAnyObjectByType<Chamber>();
            director = GetComponent<Director>();
            shake = FindAnyObjectByType<CameraShake>();
        }

        void Update()
        {
            var gm = GM;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null || dc.Mode != DayMode.Play) return;
            float dt = gm.Clock.DeltaTime;
            if (dt <= 0f) return;

            var s = gm.State;
            var cfg = gm.Config;
            var view = chamber != null ? chamber.View : default;
            bool music = s.musicLeft > 0f;
            float mixingX = cfg.layout.Station("mixing").x;
            events.Clear();
            InspectionLogic.Step(s, cfg, dt, view, music, cfg.layout.chamber.x, mixingX, events, out var passed);
            foreach (var e in events) Handle(e, passed);
        }

        void Handle(InspectionEvent e, Inspection passed)
        {
            var gm = GM;
            var s = gm.State;
            switch (e)
            {
                case InspectionEvent.Started:
                    bool boss = s.inspection.boss;
                    gm.Toast(boss ? "ui.toasts.inspectionBoss" : "ui.toasts.inspection", ToastKind.Bad);
                    if (director != null) director.SayAny("director.inspect");
                    if (shake != null) shake.Add(gm.Config.inspector.startShake);
                    EventBus.RaiseInspectionStarted(boss);
                    break;
                case InspectionEvent.RackWarning:
                    gm.Toast("ui.toasts.inspectorRack", ToastKind.Bad);
                    break;
                case InspectionEvent.Passed:
                    float prize = InspectionLogic.Prize(passed.boss, gm.Config);
                    gm.Toast("ui.toasts.inspectionPassed", ToastKind.Good,
                        ("money", gm.Texts.Format("ui.common.money", ("n", Mathf.RoundToInt(prize).ToString("N0", German)))));
                    if (director != null) director.SayAny("director.passed");
                    break;
                case InspectionEvent.Scandal:
                    DayCycle.Instance.GameOver(GameOverKind.Scandal);
                    break;
            }
        }

        /// <summary>Debug: Kontrolle sofort starten.</summary>
        public void ForceStart(bool boss)
        {
            var gm = GM;
            var s = gm.State;
            if (s.inspection != null) return;
            if (boss) s.bossAt = s.playTime;
            s.inspectionDoneAt = float.NegativeInfinity;
            InspectionLogic.Start(s, gm.Config);
            Handle(InspectionEvent.Started, null);
        }
    }
}
