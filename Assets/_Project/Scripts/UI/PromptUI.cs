using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Interaktions-Prompt unten mittig mit Haltebalken (GDD 2, 7.6). Ausgegraut, wenn die Aktion nicht möglich ist.
    /// Text wird nur bei Änderung neu gesetzt. An der hochdrehenden Zentrifuge zusätzlich der Drehzahlbalken mit
    /// Zonen grau/gelb/grün/rot und Nadel (GDD 6.3, Prototyp zbar).
    /// </summary>
    public class PromptUI : MonoBehaviour
    {
        [SerializeField] InteractionSystem source;
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text label;
        [SerializeField] Image frame;
        [SerializeField] Image fill;
        [SerializeField] Color activeFrame = new Color(1f, 0.83f, 0f);
        [SerializeField] Color disabledFrame = new Color(0.35f, 0.4f, 0.44f);
        [SerializeField] Color activeText = new Color(0.96f, 0.93f, 0.82f);
        [SerializeField] Color disabledText = new Color(0.62f, 0.69f, 0.66f);
        [SerializeField] GameObject gauge;
        [SerializeField] RectTransform yellowZone;
        [SerializeField] RectTransform greenZone;
        [SerializeField] RectTransform redZone;
        [SerializeField] RectTransform needle;

        string shownText;
        bool shownUse;

        public void SetSource(InteractionSystem s) => source = s;

        public void SetGauge(GameObject g, RectTransform yellow, RectTransform green, RectTransform red, RectTransform needleRt)
        {
            gauge = g;
            yellowZone = yellow;
            greenZone = green;
            redZone = red;
            needle = needleRt;
        }

        void Start()
        {
            // Zonen aus der Config (Anteile des Balkens)
            var z = GameManager.Instance.Config.machinesFixed.centrifuge.zones;
            Span(yellowZone, z.yellow[0], z.yellow[1]);
            Span(greenZone, z.green[0], z.green[1]);
            Span(redZone, z.red[0], z.red[1]);
            gauge.SetActive(false);
        }

        static void Span(RectTransform rt, float from, float to)
        {
            rt.anchorMin = new Vector2(from, 0f);
            rt.anchorMax = new Vector2(to, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        void LateUpdate()
        {
            if (source == null) return;
            string text = source.PromptText;
            bool visible = !string.IsNullOrEmpty(text);
            if (root.activeSelf != visible) root.SetActive(visible);
            if (!visible) return;

            if (text != shownText || source.CanUse != shownUse)
            {
                shownText = text;
                shownUse = source.CanUse;
                label.text = text;
                label.color = shownUse ? activeText : disabledText;
                frame.color = shownUse ? activeFrame : disabledFrame;
            }
            fill.fillAmount = source.Progress;

            var gm = GameManager.Instance;
            bool showGauge = source.Current is Centrifuge c && c.Spinning;
            if (gauge.activeSelf != showGauge) gauge.SetActive(showGauge);
            if (showGauge)
            {
                float rpm = Mathf.Clamp01(gm.State.centrifuge.rpm);
                needle.anchorMin = new Vector2(rpm, 0f);
                needle.anchorMax = new Vector2(rpm, 1f);
            }
        }
    }
}
