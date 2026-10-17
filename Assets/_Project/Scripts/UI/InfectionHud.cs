using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Infektion im HUD (GDD 8, Prototyp updateInfection): grüner Bildschirm-Schleier, der mit dem Infektionswert
    /// stärker wird und pulsiert (beim Symptom „grün“ deutlich), dazu Anzeige „INFEKTION · Frist Tag n“.
    /// </summary>
    public class InfectionHud : MonoBehaviour
    {
        [SerializeField] Image veil;
        [SerializeField] GameObject badge;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text deadline;
        [SerializeField] Color veilColor = new Color(0.27f, 1f, 0.35f, 0.55f);

        int shownDay = -1;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var s = gm.State;
            var c = gm.Config.infection;
            bool on = s.infected;
            badge.SetActive(on);
            float a = 0f;
            if (on)
            {
                a = s.infectionSymptom == "green"
                    ? c.greenVeil
                    : (c.veilBase + s.infectionLevel * c.veilPerLevel) * (1f - c.veilPulse + c.veilPulse * Mathf.Sin(Time.unscaledTime * 2f));
                if (shownDay != s.infectedUntilDay)
                {
                    shownDay = s.infectedUntilDay;
                    title.text = gm.Texts.Get("ui.hud.infection");
                    deadline.text = gm.Texts.Format("ui.hud.infectionDeadline", ("n", s.infectedUntilDay));
                }
            }
            var col = veilColor;
            col.a = Mathf.Clamp01(a) * veilColor.a;
            veil.color = col;
            veil.enabled = col.a > 0.001f;
        }
    }
}
