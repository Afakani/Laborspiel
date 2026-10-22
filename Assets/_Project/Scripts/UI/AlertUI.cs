using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Große Warnmeldung oben in der Bildmitte (auf Wunsch nach Spieltest): „Der Direktor wird wütend“, „Noch 60 s“,
    /// „Gleich kommt der Kommissar“. Hört auf EventBus.Alert, steht ui.alertSec und blinkt am Anfang kurz.
    /// </summary>
    public class AlertUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image frame;
        [SerializeField] TMP_Text text;
        [SerializeField] CanvasGroup group;
        [SerializeField] Color warn = new Color(1f, 0.69f, 0.13f);
        [SerializeField] Color bad = new Color(1f, 0.3f, 0.25f);
        [SerializeField] float blinkSec = 1.2f;

        float age = -1f;
        Color color;

        void Awake() => root.SetActive(false);
        void OnEnable() => EventBus.Alert += Show;
        void OnDisable() => EventBus.Alert -= Show;

        void Show(string message, ToastKind kind)
        {
            text.text = message;
            color = kind == ToastKind.Bad ? bad : warn;
            text.color = color;
            age = 0f;
            root.SetActive(true);
        }

        void Update()
        {
            if (age < 0f) return;
            var gm = GameManager.Instance;
            float total = gm != null ? gm.Config.ui.alertSec : 5f;
            age += Time.unscaledDeltaTime;
            if (age >= total)
            {
                age = -1f;
                root.SetActive(false);
                return;
            }
            // erst blinken, am Ende ausblenden
            float blink = age < blinkSec ? 0.5f + 0.5f * Mathf.Cos(age * Mathf.PI * 6f) : 1f;
            var c = color;
            c.a = Mathf.Lerp(0.35f, 1f, blink);
            frame.color = c;
            group.alpha = Mathf.Clamp01((total - age) / 0.6f);
        }
    }
}
