using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Pause mit P oder Esc (GDD 2). Ist ein Panel offen, schließt Esc/P zuerst das Panel (ab Phase 4a über PanelCloseRequested).
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text title;
        [SerializeField] TMP_Text hint;

        void Start()
        {
            var gm = GameManager.Instance;
            title.text = gm.Texts.Get("ui.hud.pause");
            hint.text = gm.Texts.Get("ui.hud.pauseHint");
            overlay.SetActive(gm.Clock.Paused);
            EventBus.PauseChanged += OnPauseChanged;
        }

        void OnDestroy() => EventBus.PauseChanged -= OnPauseChanged;

        void Update()
        {
            if (!InputReader.PausePressed) return;
            var gm = GameManager.Instance;
            if (gm.PanelOpen)
            {
                EventBus.RaisePanelCloseRequested();
                return;
            }
            gm.Clock.TogglePause();
        }

        void OnPauseChanged(bool paused) => overlay.SetActive(paused);
    }
}
