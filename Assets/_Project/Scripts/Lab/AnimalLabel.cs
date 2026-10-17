using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Schild über dem Tier (GDD 5.5): Dosen bzw. Name des Ultimativs/der Kuriosität und Status.
    /// Dreht sich zur Kamera; Text wird nur bei Änderung gesetzt.
    /// </summary>
    public class AnimalLabel : MonoBehaviour
    {
        [SerializeField] TMP_Text line1;
        [SerializeField] TMP_Text line2;
        [SerializeField] TMP_Text stamp;
        [SerializeField] Renderer frame;
        [SerializeField] Color normalFrame = new Color(0.42f, 0.35f, 0.25f);
        [SerializeField] Color hotFrame = new Color(1f, 0.42f, 0.42f);
        [SerializeField] Color statusColor = new Color(1f, 0.82f, 0.29f);

        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        MaterialPropertyBlock mpb;
        string key;
        Transform cam;

        void Awake() => mpb = new MaterialPropertyBlock();

        public void Show(bool visible)
        {
            if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
        }

        /// <summary>Inhalt setzen; key verhindert unnötiges Neusetzen.</summary>
        public void Set(string newKey, string top, Color topColor, string status, bool hot, Color? frameColor, bool stamped, string stampText)
        {
            if (newKey == key) return;
            key = newKey;
            line1.text = top;
            line1.color = topColor;
            line2.text = status;
            line2.color = hot ? hotFrame : statusColor;
            stamp.gameObject.SetActive(stamped);
            if (stamped) stamp.text = stampText;
            frame.GetPropertyBlock(mpb);
            mpb.SetColor(BaseColorId, hot ? hotFrame : frameColor ?? normalFrame);
            frame.SetPropertyBlock(mpb);
        }

        void LateUpdate()
        {
            if (cam == null && Camera.main != null) cam = Camera.main.transform;
            if (cam == null) return;
            var dir = transform.position - cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
