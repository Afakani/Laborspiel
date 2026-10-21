using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Anzeige der Einführung: Zielkasten oben („EINFÜHRUNG 3/9“ und Aufgabe) und ein Pfeil, der auf die Zielstation
    /// zeigt (über der Station mit Entfernung, außerhalb des Bildes am Rand in Blickrichtung). Ein neuer Schritt
    /// blinkt kurz auf. Vor großen Panels (Bestellung, Logbuch) tritt der Kasten zurück.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [SerializeField] GameObject box;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text text;
        [SerializeField] RectTransform marker;
        [SerializeField] RectTransform arrow;
        [SerializeField] TMP_Text distance;
        [SerializeField] float markerHeight = 2.6f;
        [SerializeField] float edgeMargin = 70f;
        [Tooltip("Oberer Bereich (Canvas-Einheiten) bleibt für Leiste, Chips und Einführungskasten frei")]
        [SerializeField] float topReserve = 300f;
        [SerializeField] float bobAmp = 8f;
        [SerializeField] float bobFreq = 4f;
        [SerializeField] Image frame;
        [SerializeField] Color flashColor = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] float flashSec = 1.2f;
        [SerializeField] float flashScale = 0.06f;

        string shownKey = "";
        Transform target;
        Camera cam;
        RectTransform canvasRect;
        Color frameColor;
        float flash;

        /// <summary>Ziel des aktuellen Schritts (für die Bodenpfeile), sonst null.</summary>
        public Transform Target => target;
        /// <summary>Sollen Hinweise im Raum gezeigt werden (Einführung aktiv, Spiel läuft, kein Panel offen)?</summary>
        public bool Guiding { get; private set; }

        void Start()
        {
            canvasRect = (RectTransform)GetComponentInParent<Canvas>().transform;
            if (frame != null) frameColor = frame.color;
            box.SetActive(false);
            marker.gameObject.SetActive(false);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null) return;
            var s = gm.State;
            bool tutorial = dc.Mode == DayMode.Play && TutorialLogic.Active(s, gm.Config);
            bool hint = !tutorial && dc.Mode == DayMode.Play && s.hint != null;
            bool active = tutorial || hint;
            // Schlüssel des Inhalts: Schritt der Einführung oder Id des Hinweises
            string key = tutorial ? "step" + s.tutorialStep : hint ? "hint" + s.hint : "";
            if (key != shownKey)
            {
                shownKey = key;
                if (tutorial) Fill(gm, s.tutorialStep);
                else if (hint) FillHint(gm, s.hint);
                if (active) flash = 1f;
            }
            // große Panels überdecken den Kasten nicht
            string open = PanelBase.OpenId;
            bool covered = open == "order" || open == "logbook" || open == "canteen" || open == "dayStart";
            if (box.activeSelf != (active && !covered)) box.SetActive(active && !covered);
            Flash();
            Guiding = active && !gm.Clock.Paused && !gm.PanelOpen && target != null;
            UpdateMarker(active && !gm.Clock.Paused && !gm.PanelOpen);
        }

        /// <summary>Neuer Schritt: Rahmen leuchtet golden auf, Kasten federt kurz.</summary>
        void Flash()
        {
            if (flash <= 0f || frame == null) return;
            flash = Mathf.Max(0f, flash - Time.unscaledDeltaTime / flashSec);
            float pulse = flash * (0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 14f));
            frame.color = Color.Lerp(frameColor, flashColor, pulse);
            box.transform.localScale = Vector3.one * (1f + flashScale * flash * flash);
        }

        void Fill(GameManager gm, int step)
        {
            var t = gm.Texts;
            title.text = t.Format("ui.tutorial.title", ("n", step + 1), ("max", gm.Config.tutorial.steps.Length));
            text.text = t.Get($"ui.tutorial.steps[{step}].text");
            target = FindTarget(gm.Config.tutorial.steps[step].target);
        }

        /// <summary>Hinweis bei erstem Auftreten: Titel „HINWEIS“, Text und Ziel aus tutorial.hints.</summary>
        void FillHint(GameManager gm, string id)
        {
            var t = gm.Texts;
            title.text = t.Get("ui.tutorial.hintTitle");
            text.text = t.Get($"ui.tutorial.hints.{id}");
            var h = HintLogic.Find(id, gm.Config);
            target = h != null ? FindTarget(h.target) : null;
        }

        /// <summary>Ziel einmal pro Schritt suchen (nicht jedes Bild): Station_&lt;id&gt; oder die Kammer.</summary>
        static Transform FindTarget(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (id == "chamber")
            {
                var c = FindAnyObjectByType<Chamber>();
                return c != null ? c.transform : null;
            }
            var go = GameObject.Find("Station_" + id);
            return go != null ? go.transform : null;
        }

        void UpdateMarker(bool show)
        {
            if (cam == null) cam = Camera.main;
            if (!show || target == null || cam == null)
            {
                if (marker.gameObject.activeSelf) marker.gameObject.SetActive(false);
                return;
            }
            if (!marker.gameObject.activeSelf) marker.gameObject.SetActive(true);

            var world = target.position + Vector3.up * markerHeight;
            var sp = cam.WorldToScreenPoint(world);
            bool behind = sp.z < 0f;
            if (behind) sp = new Vector3(Screen.width - sp.x, Screen.height - sp.y, 0f);
            float topM = Mathf.Max(edgeMargin, topReserve * canvasRect.localScale.y);
            bool inside = !behind && sp.x > edgeMargin && sp.x < Screen.width - edgeMargin && sp.y > edgeMargin && sp.y < Screen.height - topM;

            var center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 pos = sp;
            float angle;
            if (inside)
            {
                // über der Station: Pfeil zeigt nach unten und wippt
                pos.y += Mathf.Sin(Time.unscaledTime * bobFreq) * bobAmp;
                angle = 180f;
            }
            else
            {
                // am Bildrand in Richtung des Ziels
                var dir = ((Vector2)sp - center).normalized;
                if (dir.sqrMagnitude < 0.01f) dir = Vector2.down;
                float kx = (Screen.width * 0.5f - edgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.x));
                float ky = (Screen.height * 0.5f - (dir.y > 0f ? topM : edgeMargin)) / Mathf.Max(0.001f, Mathf.Abs(dir.y));
                pos = center + dir * Mathf.Min(kx, ky);
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pos, null, out var local);
            marker.anchoredPosition = local;
            arrow.localEulerAngles = new Vector3(0f, 0f, angle);
            var flat = target.position - cam.transform.position;
            flat.y = 0f;
            distance.text = GameManager.Instance.Texts.Format("ui.tutorial.distance", ("m", Mathf.RoundToInt(flat.magnitude)));
        }
    }
}
