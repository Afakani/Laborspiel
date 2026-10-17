using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>Dunkelt einen Panel-Knopf samt Beschriftung ab, solange er nicht bedienbar ist (wie im Prototyp).</summary>
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public class ButtonDimmer : MonoBehaviour
    {
        [SerializeField] float disabledAlpha = 0.35f;

        Button button;
        CanvasGroup group;
        bool last = true;

        void Awake()
        {
            button = GetComponent<Button>();
            group = GetComponent<CanvasGroup>();
        }

        void LateUpdate()
        {
            bool on = button.interactable;
            if (on == last) return;
            last = on;
            group.alpha = on ? 1f : disabledAlpha;
        }
    }
}
