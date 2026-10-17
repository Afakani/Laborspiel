using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Interaktions-Prompt unten mittig mit Haltebalken (GDD 2, 7.6). Ausgegraut, wenn die Aktion nicht möglich ist.
    /// Text wird nur bei Änderung neu gesetzt.
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

        string shownText;
        bool shownUse;

        public void SetSource(InteractionSystem s) => source = s;

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
        }
    }
}
