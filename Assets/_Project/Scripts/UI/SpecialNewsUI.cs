using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Sondersendung nach dem ersten Ultimativ (GDD 10): Einblendung im Stil des Staats-TV mit Kennung und Schlagzeile.
    /// Das volle Nachrichtenstudio folgt in Phase 10.
    /// </summary>
    public class SpecialNewsUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text tagLabel;
        [SerializeField] TMP_Text head;
        [SerializeField] RectTransform tagBox;
        [SerializeField] TMP_Text ticker;

        bool shown;

        public void Setup(GameObject rootObj, TMP_Text tagText, TMP_Text headText, RectTransform box, TMP_Text tickerText)
        {
            ticker = tickerText;
            root = rootObj;
            tagLabel = tagText;
            head = headText;
            tagBox = box;
        }

        void Awake() => root.SetActive(false);

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.News;
            if (want != shown)
            {
                shown = want;
                root.SetActive(want);
                if (want)
                {
                    var t = GameManager.Instance.Texts;
                    tagLabel.text = t.Get("ui.news.specialTag");
                    head.text = dc.NewsHead;
                    ticker.text = "+++ " + string.Join(" +++ ", t.GetList("newsFacts.specialTicker")) + " +++";
                }
            }
            // Kennung pulsiert leicht wie eine Live-Einblendung
            if (shown) tagBox.localScale = Vector3.one * (1f + 0.04f * Mathf.Sin(Time.unscaledTime * 4f));
        }
    }
}
