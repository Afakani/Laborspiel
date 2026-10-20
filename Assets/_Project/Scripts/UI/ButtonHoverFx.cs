using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Sichtbare Rückmeldung für Knöpfe: Beim Überfahren leuchtet eine warme Fläche auf und der Knopf wächst leicht,
    /// beim Drücken wird er dunkler und kleiner. Unabhängig von den Farben, die der Code am Knopf setzt (eigene Ebene).
    /// </summary>
    public class ButtonHoverFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Image glow;
        [SerializeField] Color hoverColor = new Color(1f, 0.82f, 0.45f, 0.22f);
        [SerializeField] Color pressColor = new Color(0f, 0f, 0f, 0.3f);
        [SerializeField] float hoverScale = 1.04f;
        [SerializeField] float pressScale = 0.96f;
        [SerializeField] float speed = 14f;

        Selectable sel;
        Vector3 baseScale;
        bool over, down;
        float k, p;

        public void Setup(Image glowImage) => glow = glowImage;

        void Awake()
        {
            sel = GetComponent<Selectable>();
            baseScale = transform.localScale;
        }

        void OnDisable()
        {
            over = down = false;
            k = p = 0f;
            transform.localScale = baseScale;
            if (glow != null) glow.color = Color.clear;
        }

        public void OnPointerEnter(PointerEventData e) => over = true;
        public void OnPointerExit(PointerEventData e) { over = false; down = false; }
        public void OnPointerDown(PointerEventData e) => down = true;
        public void OnPointerUp(PointerEventData e) => down = false;

        void Update()
        {
            bool active = sel == null || sel.IsInteractable();
            float dt = Time.unscaledDeltaTime * speed;
            k = Mathf.MoveTowards(k, active && over ? 1f : 0f, dt);
            p = Mathf.MoveTowards(p, active && down ? 1f : 0f, dt);
            transform.localScale = baseScale * Mathf.Lerp(Mathf.Lerp(1f, hoverScale, k), pressScale, p);
            if (glow != null) glow.color = Color.Lerp(hoverColor * new Color(1f, 1f, 1f, k), pressColor, p);
        }
    }
}
