using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Schild über einer Station. Text kommt aus der Text-JSON (textKey); optional dreht es sich
    /// um die Hochachse zur Kamera, wie die Schilder im Prototyp, und wird aus der Nähe kleiner, damit es nichts verdeckt.
    /// </summary>
    public class StationSign : MonoBehaviour
    {
        [SerializeField] string textKey;
        [SerializeField] TMP_Text label;
        [SerializeField] bool faceCamera = true;
        [SerializeField] float fullSizeDistance = 7f;
        [SerializeField] float minScale = 0.4f;

        Transform cam;
        Vector3 baseScale;

        public string TextKey => textKey;

        void Start()
        {
            var gm = GameManager.Instance;
            if (label != null && gm != null && gm.Texts != null && !string.IsNullOrEmpty(textKey))
                label.text = gm.Texts.Get(textKey);
            if (Camera.main != null) cam = Camera.main.transform;
            baseScale = transform.localScale;
        }

        void LateUpdate()
        {
            if (!faceCamera || cam == null) return;
            var dir = transform.position - cam.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(dir);
            float k = Mathf.Clamp(Mathf.Sqrt(dir.sqrMagnitude) / fullSizeDistance, minScale, 1f);
            transform.localScale = baseScale * k;
        }

        /// <summary>Für Editor-Werkzeuge: Schlüssel, Textfeld und Ausrichtung setzen.</summary>
        public void Setup(string key, TMP_Text text, bool billboard)
        {
            textKey = key;
            label = text;
            faceCamera = billboard;
        }
    }
}
