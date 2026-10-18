using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Staats-TV-Studio (GDD 11, Prototyp studio): liegt in der additiv geladenen Szene NewsStudio. Während der
    /// Sendung zeigt die Studiokamera statt der Spielerkamera; sie fährt von der Totalen zum Sprecher. Im
    /// Automatikbetrieb sackt der Sprecher zusammen, die Pflanze welkt, Fliegen kreisen, das Licht flackert grünlich.
    /// Diagramm und Senderschild zeigen je nach Stufe Planerfüllung bzw. „Bevölkerung (offiziell)“.
    /// </summary>
    public class NewsStudio : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        public static NewsStudio Instance { get; private set; }

        [Header("Kamera")]
        [SerializeField] Camera cam;

        [Header("Sprecher")]
        [SerializeField] Transform pivot;
        [SerializeField] Transform head;
        [SerializeField] Transform armL, armR;
        [SerializeField] GameObject eyesAlive, mouthAlive, eyesDead, mouthDead;
        [SerializeField] Transform sweat;
        [SerializeField] Renderer[] skin = new Renderer[0];
        [SerializeField] Color skinAlive = new Color(0.95f, 0.79f, 0.63f);
        [SerializeField] Color skinDead = new Color(0.72f, 0.79f, 0.65f);

        [Header("Studio")]
        [SerializeField] Transform[] leaves = new Transform[0];
        [SerializeField] Renderer[] leafRenderers = new Renderer[0];
        [SerializeField] Color leafAlive = new Color(0.18f, 0.66f, 0.29f);
        [SerializeField] Color leafDead = new Color(0.48f, 0.35f, 0.17f);
        [SerializeField] Transform[] flies = new Transform[0];
        [SerializeField] Light key;
        [SerializeField] Color keyAlive = Color.white;
        [SerializeField] Color keyDead = new Color(0.66f, 0.85f, 0.71f);
        [SerializeField] Renderer strip;
        [SerializeField] Renderer wall;
        [SerializeField] Color wallAlive = new Color(0.64f, 0.09f, 0.12f);
        [SerializeField] Color wallDead = new Color(0.23f, 0.05f, 0.06f);

        [Header("Diagramm und Schild")]
        [SerializeField] TMP_Text chartTitle;
        [SerializeField] Transform barLeft, barRight;
        [SerializeField] Renderer barLeftRenderer, barRightRenderer;
        [SerializeField] TMP_Text valueLeft, valueRight, labelLeft, labelRight;
        [SerializeField] float barMaxHeight = 1.1f;
        [SerializeField] float barMaxUnits = 200f;
        [SerializeField] Color barGold = new Color(1f, 0.82f, 0.29f);
        [SerializeField] Color barRed = new Color(1f, 0.42f, 0.42f);
        [SerializeField] Color barPlan = new Color(0.54f, 0.42f, 0.29f);
        [SerializeField] TMP_Text signLogo, signName, liveText;
        [SerializeField] Renderer liveBox;
        [SerializeField] Color liveColor = new Color(0.82f, 0.1f, 0.1f);
        [SerializeField] Color planColor = new Color(1f, 0.69f, 0.13f);

        MaterialPropertyBlock mpb;
        Camera playerCam;
        Vector3[] leafBase;
        bool playing, dead;
        float d, keyBase = 1f;

        void Awake()
        {
            Instance = this;
            mpb = new MaterialPropertyBlock();
            cam.enabled = false;
            keyBase = key.intensity > 0f ? key.intensity : 1f;
            leafBase = new Vector3[leaves.Length];
            for (int i = 0; i < leaves.Length; i++) leafBase[i] = leaves[i].localEulerAngles;
            Pose(0f, 0f);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>Sendung beginnen: Studio herrichten, Kamera umschalten.</summary>
        public void Play(NewsCopy copy)
        {
            var gm = GameManager.Instance;
            var t = gm.Texts;
            var n = gm.Config.news;
            dead = copy.anchorGone;
            d = 0f;
            playing = true;

            Tint(wall, dead ? wallDead : wallAlive, false);
            signLogo.text = t.Get("ui.news.logo");
            signName.text = t.Get("ui.news.signName");
            liveText.text = t.Get(dead ? "ui.news.planBroadcast" : "ui.news.live");
            Tint(liveBox, dead ? planColor : liveColor, true);

            var w = copy.world;
            if (!dead)
            {
                chartTitle.text = t.Get("ui.news.chartTitleAlive");
                float hMeat = Mathf.Clamp(n.meatBarBase + w.meat * n.meatBarPerPct, n.meatBarMin, n.meatBarMax);
                float hQual = Mathf.Clamp(n.qualityBarBase - w.quality * n.qualityBarPerPct, n.qualityBarBase, n.qualityBarMax);
                Bar(barLeft, barLeftRenderer, hMeat, barGold);
                Bar(barRight, barRightRenderer, -hQual, barRed);
                valueLeft.text = NewsLogic.Signed(w.meat) + " %";
                valueRight.text = NewsLogic.Signed(w.quality) + " %";
                labelLeft.text = t.Get("ui.news.chartMeat");
                labelRight.text = t.Get("ui.news.chartQuality");
            }
            else
            {
                chartTitle.text = t.Get("ui.news.chartTitle");
                Bar(barLeft, barLeftRenderer, n.meatBarMax * 0.95f, barPlan);
                Bar(barRight, barRightRenderer, Mathf.Max(2f, w.pop / 100f * n.meatBarMax * 0.95f), barRed);
                valueLeft.text = t.Format("ui.common.percent", ("n", 100));
                valueRight.text = t.Format("ui.common.percent", ("n", Mathf.RoundToInt(w.pop)));
                labelLeft.text = t.Get("ui.news.chartPlan");
                labelRight.text = t.Get("ui.news.chartActual");
            }
            PlaceValue(valueLeft, barLeft);
            PlaceValue(valueRight, barRight);

            playerCam = Camera.main != cam ? Camera.main : playerCam;
            if (playerCam != null) playerCam.enabled = false;
            cam.fieldOfView = gm.Config.newsStudio.fov;
            cam.enabled = true;
            Pose(0f, 0f);
            Update();
        }

        /// <summary>Sendung beenden: zurück zur Spielerkamera.</summary>
        public void Stop()
        {
            playing = false;
            cam.enabled = false;
            if (playerCam != null) playerCam.enabled = true;
        }

        void Update()
        {
            if (!playing) return;
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            var c = gm.Config.newsStudio;
            float time = dc != null ? dc.NewsTime : 0f;
            float target = dead && time > c.slumpDelaySec ? 1f : 0f;
            d += (target - d) * Mathf.Min(1f, Time.unscaledDeltaTime * c.slumpSpeed);
            Pose(d, Time.unscaledTime);

            // Kamerafahrt von der Totalen zum Sprecher (weich ein- und auslaufend)
            float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / gm.Config.news.durationSec));
            var pos = Vector3.Lerp(V(c.widePos), V(dead ? c.closeDeadPos : c.closePos), k);
            var tgt = Vector3.Lerp(V(c.wideTarget), V(dead ? c.closeDeadTarget : c.closeTarget), k);
            cam.transform.position = transform.TransformPoint(pos);
            cam.transform.LookAt(transform.TransformPoint(tgt));
        }

        /// <summary>Haltung des Sprechers und Zustand des Studios: d = 0 lebendig, 1 zusammengesackt.</summary>
        void Pose(float dd, float t)
        {
            float calm = 1f - dd;
            pivot.localRotation = Quaternion.Euler((dd * 1.3f + calm * Mathf.Sin(t * 1.3f) * 0.012f) * Mathf.Rad2Deg, 0f, 0f);
            head.localRotation = Quaternion.Euler((-1.05f * dd + calm * Mathf.Sin(t * 0.9f) * 0.04f) * Mathf.Rad2Deg,
                (calm * Mathf.Sin(t * 0.5f) * 0.18f + dd * 0.25f) * Mathf.Rad2Deg, dd * 0.4f * Mathf.Rad2Deg);
            armL.localRotation = Quaternion.Euler(-1.3f * dd * Mathf.Rad2Deg, 0f, 0f);
            armR.localRotation = Quaternion.Euler(-1.3f * dd * Mathf.Rad2Deg, 0f, 0f);
            foreach (var r in skin) Tint(r, Color.Lerp(skinAlive, skinDead, dd), false);
            eyesAlive.SetActive(dd < 0.5f);
            mouthAlive.SetActive(dd < 0.5f);
            eyesDead.SetActive(dd >= 0.5f);
            mouthDead.SetActive(dd >= 0.5f);
            sweat.gameObject.SetActive(dd < 0.3f);
            var sp = sweat.localPosition;
            sweat.localPosition = new Vector3(sp.x, 0.3f - (t * 0.7f % 1f) * 0.3f, sp.z);

            for (int i = 0; i < leaves.Length; i++)
            {
                var b = leafBase[i];
                leaves[i].localEulerAngles = new Vector3(b.x, b.y, Mathf.Lerp(b.z, (1.25f + i * 0.05f) * Mathf.Rad2Deg, dd));
            }
            foreach (var r in leafRenderers) Tint(r, Color.Lerp(leafAlive, leafDead, dd), false);

            float flick = Mathf.Sin(t * 37f) * Mathf.Sin(t * 11.3f) > 0.93f ? 0.35f : 1f;
            key.color = Color.Lerp(keyAlive, keyDead, dd);
            key.intensity = Mathf.Lerp(1f, 0.8f * flick, dd) * keyBase;
            Tint(strip, new Color(0.78f * (1f - dd * (1f - flick * 0.9f)), 0.16f * (1f - dd), 0.16f * (1f - dd)), true);

            var hp = head.position;
            for (int i = 0; i < flies.Length; i++)
            {
                float a = t * (2f + i * 0.7f) + i * 2f, r = 0.55f + 0.12f * i;
                flies[i].gameObject.SetActive(dd > 0.7f);
                flies[i].position = hp + new Vector3(Mathf.Cos(a) * r, 0.35f + Mathf.Sin(a * 1.7f) * 0.15f, Mathf.Sin(a) * r);
            }
        }

        void Bar(Transform pivotT, Renderer r, float units, Color c)
        {
            float h = Mathf.Max(0.01f, Mathf.Abs(units) / barMaxUnits * barMaxHeight);
            pivotT.localScale = new Vector3(1f, units >= 0f ? h : -h, 1f);
            Tint(r, c, true);
        }

        /// <summary>Wert über (bzw. bei negativem Balken über der Grundlinie) anzeigen.</summary>
        void PlaceValue(TMP_Text label, Transform barPivot)
        {
            // Der Text hängt in einer eigenen Gruppe im Diagramm; die Gruppe wird verschoben
            var group = label.transform.parent;
            float top = Mathf.Max(0f, barPivot.localScale.y);
            var p = group.localPosition;
            group.localPosition = new Vector3(barPivot.localPosition.x, barPivot.localPosition.y + top + 0.18f, p.z);
        }

        void Tint(Renderer r, Color c, bool emissive)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, c);
            if (emissive) mpb.SetColor(EmissionId, c * 0.6f);
            r.SetPropertyBlock(mpb);
        }

        /// <summary>Prototyp-Koordinate (three.js, rechtshändig) ins gespiegelte Studio übertragen: x umdrehen.</summary>
        static Vector3 V(float[] a) => a != null && a.Length >= 3 ? new Vector3(-a[0], a[1], a[2]) : Vector3.zero;
    }
}
