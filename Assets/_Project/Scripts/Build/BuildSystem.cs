using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Gebaute Maschinen (GDD 6.10, Prototyp machineTick/placeMachine): hält die Modelle mit Rohr zur Kammer passend
    /// zum Spielstand und lässt sie während der Schicht wirken (Melkpumpe, Auto-Sprüher, Kühlaggregat, Notfall-Automat).
    /// </summary>
    public class BuildSystem : MonoBehaviour
    {
        [SerializeField] MachinePrefab[] prefabs = new MachinePrefab[0];
        [SerializeField] GameObject pipePrefab;
        [SerializeField] Transform container;

        readonly Dictionary<PlacedMachine, (GameObject machine, GameObject pipe)> spawned = new Dictionary<PlacedMachine, (GameObject, GameObject)>();
        readonly List<PlacedMachine> toRemove = new List<PlacedMachine>();
        Chamber chamber;

        public void Setup(MachinePrefab[] entries, GameObject pipe, Transform parent)
        {
            prefabs = entries;
            pipePrefab = pipe;
            container = parent;
        }

        void Start() => chamber = FindAnyObjectByType<Chamber>();

        public MachinePrefab Entry(string id) => System.Array.Find(prefabs, p => p.id == id);

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            Sync(gm);
            var dc = DayCycle.Instance;
            if (dc == null || dc.Mode != DayMode.Play || chamber == null) return;
            float dt = gm.Clock.DeltaTime;
            if (dt <= 0f) return;
            Tick(gm, dt);
        }

        // ---------- Wirkung ----------

        void Tick(GameManager gm, float dt)
        {
            var s = gm.State;
            var cfg = gm.Config;
            bool ok = chamber.State == AnimalState.Ok;
            bool living = chamber.HasLivingAnimal;

            // Melkpumpen: Milch mit fester Qualität
            float pump = BuildLogic.PumpRate(s, cfg, ok && chamber.Kind == AnimalKind.Cow);
            if (pump > 0f)
            {
                Production.Deliver(s, cfg, cfg.animals.cow.produces[0], pump * dt, BuildLogic.Type(cfg, BuildLogic.Pump).quality);
                s.currentRate += pump;
            }

            // Kühlaggregate
            if (living)
            {
                var st = chamber.Stats;
                st.heat = Mathf.Max(0f, st.heat - BuildLogic.CoolRate(s, cfg) * dt);
            }

            foreach (var m in s.machines)
            {
                var t = BuildLogic.Type(cfg, m.typeId);
                if (m.typeId == BuildLogic.Spray && BuildLogic.Due(m, t, dt)) AutoSpray(gm, m, t);
                else if (m.typeId == BuildLogic.Doc && BuildLogic.Due(m, t, dt) && BuildLogic.DocNeeded(s, chamber.State, chamber.Stats.crisis))
                {
                    m.timer = 0f;
                    if (chamber.AntidoteToAnimal().IsOk) gm.Toast("ui.toasts.emergencyDoc", ToastKind.Good);
                }
            }
        }

        /// <summary>Auto-Sprüher (Prototyp autoSpray): zuletzt benutztes Serum erneut sprühen, sonst später noch einmal.</summary>
        void AutoSpray(GameManager gm, PlacedMachine m, BuildMachineType t)
        {
            var s = gm.State;
            var cfg = gm.Config;
            float retry = t.intervalSec - cfg.buildMachines.autoSprayRetrySec;
            if (chamber.State != AnimalState.Ok || s.lastSprayed == null || s.chamberCovered || chamber.IsUltimate)
            {
                m.timer = retry;
                return;
            }
            int idx = BuildLogic.FindLastSprayed(s);
            if (idx < 0)
            {
                if (s.playTime - s.autoSprayWarnedAt > cfg.buildMachines.autoSprayWarnGapSec)
                {
                    s.autoSprayWarnedAt = s.playTime;
                    gm.Toast("ui.toasts.autoSprayNone", ToastKind.Warn);
                }
                m.timer = retry;
                return;
            }
            m.timer = chamber.Spray(idx, true).IsOk ? 0f : retry;
        }

        // ---------- Modelle ----------

        /// <summary>Modelle an die Liste im Spielstand anpassen (Bauen, Abriss, neues Spiel).</summary>
        void Sync(GameManager gm)
        {
            var list = gm.State.machines;
            toRemove.Clear();
            foreach (var kv in spawned)
                if (!list.Contains(kv.Key)) toRemove.Add(kv.Key);
            foreach (var m in toRemove)
            {
                var (machine, pipe) = spawned[m];
                Destroy(machine);
                Destroy(pipe);
                spawned.Remove(m);
            }
            foreach (var m in list)
                if (!spawned.ContainsKey(m)) Spawn(gm.Config, m);
        }

        void Spawn(GameConfig cfg, PlacedMachine m)
        {
            var entry = Entry(m.typeId);
            if (entry == null) return;
            var c = BuildLogic.CellCenter(cfg, m.gx, m.gz);
            var pos = LayoutConfig.ToUnity(c.x, 0f, c.y);
            var chamberPos = LayoutConfig.ToUnity(cfg.layout.chamber.x, 0f, cfg.layout.chamber.z);
            var go = Instantiate(entry.prefab, pos, FacingChamber(pos, chamberPos), container);
            go.name = "Machine_" + m.typeId;
            spawned[m] = (go, SpawnPipe(cfg, entry, pos, chamberPos));
        }

        /// <summary>Vorderseite (+Z des Modells) zur Kammer.</summary>
        public static Quaternion FacingChamber(Vector3 pos, Vector3 chamberPos)
        {
            var d = chamberPos - pos;
            d.y = 0f;
            return d.sqrMagnitude > 0.001f ? Quaternion.LookRotation(d) : Quaternion.identity;
        }

        /// <summary>Rohr von der Maschine bis kurz vor den Kammerrand (Einheitszylinder entlang Y, Länge 1).</summary>
        GameObject SpawnPipe(GameConfig cfg, MachinePrefab entry, Vector3 pos, Vector3 chamberPos)
        {
            var b = cfg.buildMachines;
            var dir = pos - chamberPos;
            dir.y = 0f;
            dir.Normalize();
            var from = pos + Vector3.up * b.pipeY;
            var to = chamberPos + dir * (cfg.layout.chamber.radius + b.pipeGap) + Vector3.up * b.pipeY;
            var d = to - from;
            var pipe = Instantiate(pipePrefab, (from + to) * 0.5f, Quaternion.FromToRotation(Vector3.up, d.normalized), container);
            pipe.name = "Pipe_" + entry.id;
            var sc = pipe.transform.localScale;
            pipe.transform.localScale = new Vector3(sc.x, d.magnitude, sc.z);
            foreach (var r in pipe.GetComponentsInChildren<Renderer>()) r.sharedMaterial = entry.pipeMaterial;
            return pipe;
        }
    }
}
