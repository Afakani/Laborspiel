using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Kommissar-Figur (GDD 9): läuft während der Kontrolle vor der Kammer von rechts nach links, Schild
    /// „KOMMISSAR“ und Verdachtsbalken über dem Kopf, schaut zur Kammer, schwankt beim Wodka.
    /// Zugleich Interaktionsziel „Kommissar Wodka anbieten“.
    /// </summary>
    public class Inspector : Interactable
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] Transform figure;
        [SerializeField] Transform legL, legR, armL, armR, head;
        [SerializeField] GameObject bottle;
        [SerializeField] Transform overhead;
        [SerializeField] TMP_Text sign;
        [SerializeField] Renderer signPlate;
        [SerializeField] Transform barFill;
        [SerializeField] Color signColor = new Color(0.82f, 0.1f, 0.1f);
        [SerializeField] Color bossSignColor = new Color(0.48f, 0.04f, 0.06f);
        [SerializeField] float headTurnDeg = 55f;
        [SerializeField] float drinkArmDeg = 125f;

        MaterialPropertyBlock mpb;
        float phase, drinkK;
        bool shownBoss, visible;
        Camera cam;

        public override string Key => "vodka";

        public void Setup(Transform figureRoot, Transform legLeft, Transform legRight, Transform armLeft, Transform armRight,
            Transform headPivot, GameObject bottleObj, Transform overheadRoot, TMP_Text signText, Renderer plate, Transform fill)
        {
            figure = figureRoot;
            legL = legLeft;
            legR = legRight;
            armL = armLeft;
            armR = armRight;
            head = headPivot;
            bottle = bottleObj;
            overhead = overheadRoot;
            sign = signText;
            signPlate = plate;
            barFill = fill;
        }

        void Awake()
        {
            mpb = new MaterialPropertyBlock();
            SetVisible(false);
        }

        // ---------- Wodka ----------

        public override bool TryGetScore(Vector2 p, out float score)
        {
            score = float.MaxValue;
            if (GM == null || State.inspection == null) return false;
            var c = Cfg.inspector;
            float d = Vector2.Distance(p, new Vector2(State.inspection.x, c.pathZ));
            if (d >= c.vodkaReach) return false;
            score = d - c.vodkaScoreOffset;
            return true;
        }

        public override void GetPrompt(out string text, out bool canUse, out float holdTime)
        {
            var ins = State.inspection;
            if (ins != null && ins.Drinking)
            {
                text = Texts.Get("ui.prompts.vodkaDrinking");
                canUse = false;
            }
            else if (State.vodka > 0)
            {
                text = Texts.Format("ui.prompts.vodkaOffer", ("n", State.vodka));
                canUse = true;
            }
            else
            {
                text = Texts.Get("ui.prompts.vodkaNone");
                canUse = false;
            }
            holdTime = canUse ? Cfg.interaction.hold.vodka : DisabledHold;
        }

        public override void Execute()
        {
            if (!InspectionLogic.OfferVodka(State, Cfg).Report(GM)) return;
            var v = Cfg.inspection.vodka;
            GM.Toast("ui.toasts.vodka", ToastKind.Good, ("sec", Mathf.RoundToInt(v.pauseSec)), ("n", Mathf.RoundToInt(v.suspReduce)));
        }

        // ---------- Darstellung ----------

        void SetVisible(bool on)
        {
            visible = on;
            figure.gameObject.SetActive(on);
            overhead.gameObject.SetActive(on);
        }

        void LateUpdate()
        {
            var gm = GM;
            if (gm == null) return;
            var ins = gm.State.inspection;
            if (ins == null)
            {
                if (visible) SetVisible(false);
                return;
            }
            var cfg = gm.Config;
            var c = cfg.inspector;
            if (!visible || shownBoss != ins.boss) ApplyLook(ins.boss, gm);

            float dt = gm.Clock.DeltaTime;
            float t = gm.State.playTime;
            transform.position = LayoutConfig.ToUnity(ins.x, 0f, c.pathZ);

            // Gehen: Beine und Arme pendeln, solange er nicht trinkt
            bool walking = !ins.Drinking && dt > 0f;
            if (walking) phase += dt * c.stepFreq * Mathf.PI * 2f;
            float swing = walking ? Mathf.Sin(phase) * c.legSwingDeg : 0f;
            legL.localRotation = Quaternion.Euler(swing, 0f, 0f);
            legR.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            armL.localRotation = Quaternion.Euler(-swing * 0.7f, 0f, 0f);

            // Trinken: rechter Arm mit Flasche zum Mund, ganzer Körper schwankt
            drinkK = Mathf.MoveTowards(drinkK, ins.Drinking ? 1f : 0f, dt * 3f);
            armR.localRotation = Quaternion.Euler(swing * 0.7f - drinkArmDeg * drinkK, 0f, 0f);
            bottle.SetActive(drinkK > 0.05f);
            float sway = ins.Drinking ? Mathf.Sin(t * c.swayFreq) * c.swayDeg : 0f;
            figure.localRotation = Quaternion.Euler(0f, 0f, sway);

            // Kopf zur Kammer drehen, wenn sie in Reichweite ist
            float yaw = 0f;
            if (Mathf.Abs(ins.x - cfg.layout.chamber.x) < cfg.inspection.range)
            {
                var chamberPos = LayoutConfig.ToUnity(cfg.layout.chamber.x, 0f, cfg.layout.chamber.z);
                var local = transform.InverseTransformDirection(chamberPos - transform.position);
                yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -headTurnDeg, headTurnDeg);
            }
            head.localRotation = Quaternion.Slerp(head.localRotation, Quaternion.Euler(0f, yaw, 0f), Mathf.Min(1f, dt * 4f));

            // Verdachtsbalken und Schild zur Kamera
            float k = Mathf.Clamp01(ins.suspicion / cfg.inspection.suspMax);
            var s = barFill.localScale;
            barFill.localScale = new Vector3(Mathf.Max(0.001f, k), s.y, s.z);
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                var dir = overhead.position - cam.transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f) overhead.rotation = Quaternion.LookRotation(dir);
            }
        }

        void ApplyLook(bool boss, GameManager gm)
        {
            SetVisible(true);
            shownBoss = boss;
            float scale = boss ? gm.Config.inspector.bossScale : 1f;
            transform.localScale = Vector3.one * scale;
            sign.text = gm.Texts.Get(boss ? "ui.signs.inspectorBoss" : "ui.signs.inspector");
            signPlate.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, boss ? bossSignColor : signColor);
            signPlate.SetPropertyBlock(mpb);
            // Laufrichtung: nach links (−X)
            transform.rotation = Quaternion.LookRotation(Vector3.left);
        }
    }
}
