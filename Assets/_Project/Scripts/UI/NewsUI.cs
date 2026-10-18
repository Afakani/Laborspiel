using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Bauchbinde des Staats-TV (GDD 11, Prototyp #newsUI): Kennung (rot), Schlagzeile, Zeitzeile, Laufband mit den
    /// Meldungen (zweimal hintereinander, läuft nach links) und „Weiter“, sobald übersprungen werden darf.
    /// Das übrige Labor-HUD ist während der Sendung ausgeblendet.
    /// </summary>
    public class NewsUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text tagLabel;
        [SerializeField] TMP_Text head;
        [SerializeField] TMP_Text year;
        [SerializeField] TMP_Text channel;
        [SerializeField] RectTransform tickerTrack;
        [SerializeField] TMP_Text ticker;
        [SerializeField] Button skip;
        [SerializeField] TMP_Text skipLabel;
        [SerializeField] Image tagBox;
        [SerializeField] Color tagColor = new Color(0.82f, 0.1f, 0.1f);
        [SerializeField] Color specialColor = new Color(1f, 0.69f, 0.13f);
        [SerializeField] CanvasGroup[] hud = new CanvasGroup[0];

        bool shown;
        float trackWidth;

        public void Setup(GameObject rootObj, TMP_Text tagText, TMP_Text headText, TMP_Text yearText, TMP_Text channelText,
            RectTransform track, TMP_Text tickerText, Button skipButton, Image box)
        {
            root = rootObj;
            tagLabel = tagText;
            head = headText;
            year = yearText;
            channel = channelText;
            tickerTrack = track;
            ticker = tickerText;
            skip = skipButton;
            skipLabel = skipButton.GetComponentInChildren<TMP_Text>();
            tagBox = box;
        }

        /// <summary>Labor-HUD, das während der Sendung verschwindet.</summary>
        public void SetHud(CanvasGroup[] groups) => hud = groups;

        void Awake()
        {
            skip.onClick.AddListener(() => DayCycle.Instance.SkipNews());
            root.SetActive(false);
        }

        void Update()
        {
            var dc = DayCycle.Instance;
            bool want = dc != null && dc.Mode == DayMode.News && dc.CurrentNews != null;
            if (want != shown)
            {
                shown = want;
                root.SetActive(want);
                foreach (var g in hud) g.alpha = want ? 0f : 1f;
                if (want) Fill(dc.CurrentNews);
            }
            if (!shown) return;
            skip.gameObject.SetActive(dc.CanSkipNews);
            // Laufband: zwei Durchgänge hintereinander, nach der Hälfte von vorn
            var p = tickerTrack.anchoredPosition;
            p.x -= GameManager.Instance.Config.news.tickerSpeed * Time.unscaledDeltaTime;
            if (trackWidth > 0f && p.x < -trackWidth * 0.5f) p.x += trackWidth * 0.5f;
            tickerTrack.anchoredPosition = p;
        }

        void Fill(NewsCopy c)
        {
            var t = GameManager.Instance.Texts;
            tagLabel.text = c.tag;
            head.text = c.head;
            year.text = c.year;
            channel.text = t.Get("ui.news.channel");
            skipLabel.text = t.Get("ui.news.skip");
            tagBox.color = c.special ? specialColor : tagColor;
            var once = "+++ " + string.Join("   +++ ", c.ticker) + "   ";
            ticker.text = once + once;
            ticker.ForceMeshUpdate();
            trackWidth = ticker.preferredWidth;
            tickerTrack.sizeDelta = new Vector2(trackWidth, tickerTrack.sizeDelta.y);
            tickerTrack.anchoredPosition = new Vector2(0f, tickerTrack.anchoredPosition.y);
        }
    }
}
