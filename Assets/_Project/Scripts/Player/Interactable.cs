using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Basis für alle Stationen, die man benutzen kann. Meldet sich in einer Liste an (kein Find im Update)
    /// und liest Reichweite und Position aus layout.stations (stationId).
    /// </summary>
    public abstract class Interactable : MonoBehaviour, IInteractable
    {
        static readonly List<Interactable> all = new List<Interactable>();
        /// <summary>Alle aktiven Interaktionsziele.</summary>
        public static IReadOnlyList<Interactable> All => all;

        [SerializeField] protected string stationId;

        StationLayout layout;

        public virtual string Key => stationId;

        protected GameManager GM => GameManager.Instance;
        protected GameConfig Cfg => GM.Config;
        protected TextDatabase Texts => GM.Texts;
        protected GameState State => GM.State;
        protected StationLayout Layout => layout ?? (layout = Cfg.layout.Station(stationId));

        protected virtual void OnEnable() => all.Add(this);
        protected virtual void OnDisable() => all.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatic() => all.Clear();

        /// <summary>Für Editor-Werkzeuge.</summary>
        public void SetStationId(string id) => stationId = id;

        /// <summary>Standard: Kreis mit layout.reach um die Station, Wertung = Abstand.</summary>
        public virtual bool TryGetScore(Vector2 p, out float score)
        {
            score = float.MaxValue;
            if (GM == null || Layout == null || Layout.reach <= 0f) return false;
            float d = Vector2.Distance(p, Layout.ProtoXZ);
            if (d >= Layout.reach) return false;
            score = d;
            return true;
        }

        public abstract void GetPrompt(out string text, out bool canUse, out float holdTime);
        public abstract void Execute();

        /// <summary>Haltezeit für ausgegraute Prompts.</summary>
        protected float DisabledHold => Cfg.interaction.disabledHoldSec;

        /// <summary>Platzhalter, bis die Station in ihrer Phase umgesetzt ist.</summary>
        protected void Placeholder(string phase) =>
            GM.Toast("ui.toasts.placeholder", ToastKind.Info, ("name", Texts.Get("ui.names." + stationId)), ("phase", phase));

        /// <summary>Liegt der Punkt in einer |x|-Zone (Pult, Laken)?</summary>
        protected static bool InAbsZone(Vector2 p, ZoneLayout z) =>
            Mathf.Abs(p.x) >= z.absXMin && Mathf.Abs(p.x) < z.absXMax && p.y >= z.zMin && p.y < z.zMax;
    }
}
