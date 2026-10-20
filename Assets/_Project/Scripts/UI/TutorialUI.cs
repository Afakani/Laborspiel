using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Anzeige der Einführung: Zielkasten oben („EINFÜHRUNG 3/9“ und Aufgabe) und ein Pfeil, der auf die Zielstation
    /// zeigt (über der Station mit Entfernung, außerhalb des Bildes am Rand in Blickrichtung).
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
        [SerializeField] float bobAmp = 8f;
        [SerializeField] float bobFreq = 4f;

        int shownStep = -2;
        Transform target;
        Camera cam;
        RectTransform canvasRect;

        void Start()
        {
            canvasRect = (RectTransform)GetComponentInParent<Canvas>().transform;
            box.SetActive(false);
            marker.gameObject.SetActive(false);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null) return;
            var s = gm.State;
            bool active = dc.Mode == DayMode.Play && TutorialLogic.Active(s, gm.Config);
            int step = active ? s.tutorialStep : -1;
            if (step != shownStep)
            {
                shownStep = step;
                box.SetActive(active);
                if (active) Fill(gm, step);
            }
            UpdateMarker(active && !gm.Clock.Paused && !gm.PanelOpen);
        }

        void Fill(GameManager gm, int step)
        {
            var t = gm.Texts;
            title.text = t.Format("ui.tutorial.title", ("n", step + 1), ("max", gm.Config.tutorial.steps.Length));
            text.text = t.Get($"ui.tutorial.steps[{step}].text");
            target = FindTarget(gm.Config.tutorial.steps[step].target);
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
            bool inside = !behind && sp.x > edgeMargin && sp.x < Screen.width - edgeMargin && sp.y > edgeMargin && sp.y < Screen.height - edgeMargin;

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
                float ky = (Screen.height * 0.5f - edgeMargin) / Mathf.Max(0.001f, Mathf.Abs(dir.y));
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
