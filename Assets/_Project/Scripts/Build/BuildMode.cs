using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PlanErfuellt
{
    /// <summary>
    /// Baumodus (GDD 6.10, Prototyp setBuild/buildMove/buildClick): B schaltet in die Draufsicht, Rückwand, linke
    /// Wand und Decke werden ausgeblendet, das Raster erscheint. Maschine wählen (1–4 oder Leiste), aufs Feld klicken;
    /// X = Abriss, Rechtsklick = Auswahl aufheben, B = zurück; Esc kommt über das Pause-Menü als „Panel schließen“.
    /// Das Spiel läuft weiter.
    /// </summary>
    public class BuildMode : MonoBehaviour
    {
        public const string SellTool = "sell";
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Camera buildCamera;
        [SerializeField] GameObject gridRoot;
        [SerializeField] Renderer marker;
        [SerializeField] Renderer[] hidden = new Renderer[0];
        [SerializeField] BuildSystem system;
        [SerializeField] Material ghostMaterial;
        [SerializeField] Color goodColor = new Color(0.25f, 1f, 0.48f, 0.4f);
        [SerializeField] Color badColor = new Color(1f, 0.3f, 0.23f, 0.4f);
        [SerializeField] Color neutralColor = new Color(1f, 0.82f, 0.29f, 0.4f);

        public static BuildMode Instance { get; private set; }
        public bool Active { get; private set; }
        /// <summary>Gewähltes Werkzeug: Maschinen-Id, SellTool oder null.</summary>
        public string Tool { get; private set; }

        readonly List<BuildMachineType> order = new List<BuildMachineType>();
        MaterialPropertyBlock mpb;
        Camera playerCamera;
        bool[] hiddenWasOn;
        GameObject ghost;
        bool helpShown, entering;

        GameManager GM => GameManager.Instance;

        public void Setup(Camera cam, GameObject grid, Renderer cellMarker, Renderer[] hide, BuildSystem sys, Material ghostMat)
        {
            buildCamera = cam;
            gridRoot = grid;
            marker = cellMarker;
            hidden = hide;
            system = sys;
            ghostMaterial = ghostMat;
        }

        /// <summary>Maschinentypen in der Reihenfolge der Leiste (nach Freischaltung).</summary>
        public IReadOnlyList<BuildMachineType> Types
        {
            get
            {
                if (order.Count == 0 && GM != null)
                {
                    // stabil sortieren: gleiche Freischaltung behält die Reihenfolge der JSON (feste Tasten)
                    var all = GM.Config.buildMachineTypes;
                    order.AddRange(all);
                    order.Sort((a, b) => a.unlockPlanIdx != b.unlockPlanIdx ? a.unlockPlanIdx.CompareTo(b.unlockPlanIdx) : all.IndexOf(a).CompareTo(all.IndexOf(b)));
                }
                return order;
            }
        }

        void Awake()
        {
            Instance = this;
            mpb = new MaterialPropertyBlock();
            buildCamera.enabled = false;
            gridRoot.SetActive(false);
            marker.enabled = false;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void OnEnable() => EventBus.PanelCloseRequested += OnCloseRequested;
        void OnDisable() => EventBus.PanelCloseRequested -= OnCloseRequested;

        /// <summary>Esc (über das Pause-Menü) oder ein Overlay des Tagesablaufs beendet den Baumodus.</summary>
        void OnCloseRequested()
        {
            if (Active && !entering) Exit();
        }

        void Update()
        {
            var gm = GM;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null) return;
            if (!Active)
            {
                if (InputReader.BuildPressed && dc.Mode == DayMode.Play && !gm.PanelOpen && !gm.Clock.Paused) Enter();
                return;
            }
            if (dc.Mode != DayMode.Play || InputReader.BuildPressed)
            {
                Exit();
                return;
            }

            int k = InputReader.ToolPressed;
            if (k >= 0 && k < Types.Count) Select(Types[k].id);
            if (InputReader.SellPressed) Select(SellTool);
            if (InputReader.DeselectPressed) Select(null);

            bool overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool hit = Hover(out int gx, out int gz);
            if (hit && !overUi && InputReader.PlacePressed) Click(gx, gz);
        }

        // ---------- Ein/Aus ----------

        /// <summary>Baumodus öffnen (Taste B, Debug-Menü).</summary>
        public void Enter()
        {
            var gm = GM;
            Active = true;
            entering = true;
            EventBus.RaisePanelCloseRequested();
            entering = false;
            gm.PanelOpen = true;
            var player = FindAnyObjectByType<FirstPersonController>();
            playerCamera = player != null ? player.Camera : Camera.main;
            if (playerCamera != null) playerCamera.enabled = false;
            var v = gm.Config.buildView;
            buildCamera.transform.position = LayoutConfig.ToUnity(v.camX, v.camY, v.camZ);
            buildCamera.transform.LookAt(LayoutConfig.ToUnity(v.lookX, v.lookY, v.lookZ));
            buildCamera.fieldOfView = v.fov;
            buildCamera.enabled = true;
            hiddenWasOn = new bool[hidden.Length];
            for (int i = 0; i < hidden.Length; i++)
            {
                hiddenWasOn[i] = hidden[i].enabled;
                hidden[i].enabled = false;
            }
            gridRoot.SetActive(true);
            if (!helpShown)
            {
                helpShown = true;
                gm.Toast("ui.toasts.buildHelp", ToastKind.Info);
            }
        }

        /// <summary>Baumodus schließen.</summary>
        public void Exit()
        {
            Active = false;
            Select(null);
            marker.enabled = false;
            gridRoot.SetActive(false);
            for (int i = 0; i < hidden.Length; i++) hidden[i].enabled = hiddenWasOn[i];
            buildCamera.enabled = false;
            if (playerCamera != null) playerCamera.enabled = true;
            if (DayCycle.Instance != null && DayCycle.Instance.Mode == DayMode.Play) GM.PanelOpen = false;
        }

        // ---------- Werkzeuge ----------

        /// <summary>Werkzeug wählen (gleiches noch einmal = abwählen).</summary>
        public void Select(string tool)
        {
            if (tool != null && tool == Tool) tool = null;
            Tool = tool;
            if (ghost != null) Destroy(ghost);
            ghost = null;
            if (tool == null || tool == SellTool) return;
            var entry = system.Entry(tool);
            if (entry == null) return;
            ghost = Instantiate(entry.prefab, transform);
            ghost.name = "Ghost";
            foreach (var c in ghost.GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (var fx in ghost.GetComponentsInChildren<MachineFx>()) fx.enabled = false;
            foreach (var r in ghost.GetComponentsInChildren<Renderer>())
            {
                var mats = new Material[r.sharedMaterials.Length];
                for (int i = 0; i < mats.Length; i++) mats[i] = ghostMaterial;
                r.sharedMaterials = mats;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            ghost.SetActive(false);
        }

        /// <summary>Feld unter der Maus markieren (grün = möglich, rot = nicht möglich, gelb = ohne Werkzeug).</summary>
        bool Hover(out int gx, out int gz)
        {
            gx = gz = -1;
            var cfg = GM.Config;
            var ray = buildCamera.ScreenPointToRay(InputReader.Point);
            bool onFloor = new Plane(Vector3.up, Vector3.zero).Raycast(ray, out float dist);
            var p = ray.GetPoint(dist);
            bool inGrid = onFloor && BuildLogic.CellAt(cfg, new Vector2(p.x, -p.z), out gx, out gz);
            marker.enabled = inGrid;
            if (ghost != null) ghost.SetActive(inGrid);
            if (!inGrid) return false;

            var s = GM.State;
            var c = BuildLogic.CellCenter(cfg, gx, gz);
            var pos = LayoutConfig.ToUnity(c.x, 0f, c.y);
            marker.transform.position = pos + Vector3.up * 0.05f;
            bool free = BuildLogic.Free(s, cfg, gx, gz);
            Color col;
            if (Tool == SellTool) col = BuildLogic.At(s, gx, gz) != null ? goodColor : badColor;
            else if (Tool != null)
            {
                var t = BuildLogic.Type(cfg, Tool);
                col = free && BuildLogic.Unlocked(s, t) && s.Money >= BuildLogic.Price(s, cfg, t) ? goodColor : badColor;
            }
            else col = neutralColor;
            marker.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, col);
            marker.SetPropertyBlock(mpb);
            if (ghost != null)
            {
                var chamberPos = LayoutConfig.ToUnity(cfg.layout.chamber.x, 0f, cfg.layout.chamber.z);
                ghost.transform.SetPositionAndRotation(pos, BuildSystem.FacingChamber(pos, chamberPos));
            }
            return true;
        }

        /// <summary>Mit dem gewählten Werkzeug auf ein Feld klicken (bauen bzw. abreißen).</summary>
        public void Click(int gx, int gz)
        {
            var gm = GM;
            var s = gm.State;
            var cfg = gm.Config;
            if (Tool == SellTool)
            {
                var m = BuildLogic.Sell(s, cfg, gx, gz);
                if (m == null) return;
                gm.Toast("ui.toasts.buildRemoved", ToastKind.Info, ("name", BuildLogic.Type(cfg, m.typeId).displayName));
                EventBus.RaiseStation("buildSold");
                return;
            }
            if (Tool == null) return;
            var t = BuildLogic.Type(cfg, Tool);
            if (!BuildLogic.Place(s, cfg, t, gx, gz).Report(gm)) return;
            gm.Toast("ui.toasts.buildPlaced", ToastKind.Good, ("name", t.displayName));
            EventBus.RaiseStation("buildPlaced");
        }
    }
}
