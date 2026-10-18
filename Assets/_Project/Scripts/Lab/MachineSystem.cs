using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Lässt Zentrifuge, Kryokammer, Musik, Mikroskop und Rohrpost während der Schicht laufen und meldet Ereignisse (Toasts, Effekte).</summary>
    public class MachineSystem : MonoBehaviour
    {
        readonly List<MachineEvent> events = new List<MachineEvent>();

        void Update()
        {
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null || dc.Mode != DayMode.Play) return;
            float dt = gm.Clock.DeltaTime;
            if (dt <= 0f) return;
            StationLogic.Tick(gm.State, dt);
            events.Clear();
            MachineLogic.Tick(gm.State, gm.Config, dt, events);
            foreach (var e in events)
            {
                switch (e)
                {
                    case MachineEvent.CentrifugeOverspeed:
                        gm.Toast("ui.toasts.centrifugeOverspeed", ToastKind.Bad);
                        break;
                    case MachineEvent.CentrifugeReturned:
                        EventBus.RaiseSerumRackChanged();
                        break;
                    case MachineEvent.CryoReady:
                        gm.Toast("ui.toasts.cryoReady", ToastKind.Warn, ("sec", Mathf.RoundToInt(gm.Config.machinesFixed.cryo.takeWindowSec)));
                        break;
                    case MachineEvent.CryoIceBlock:
                        gm.Toast("ui.toasts.cryoIceBlock", ToastKind.Bad);
                        break;
                }
                EventBus.RaiseMachine(e);
            }
        }
    }
}
