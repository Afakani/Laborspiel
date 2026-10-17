using TMPro;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Fadenkreuz in der Bildmitte; zeigt einen Hinweis, solange die Maus nicht gefangen ist.</summary>
    public class Crosshair : MonoBehaviour
    {
        [SerializeField] GameObject dot;
        [SerializeField] TMP_Text mouseHint;

        bool hintSet;

        void LateUpdate()
        {
            var gm = GameManager.Instance;
            bool control = gm != null && gm.PlayerControlEnabled;
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            if (dot.activeSelf != control) dot.SetActive(control);
            bool hint = control && !locked;
            if (mouseHint.gameObject.activeSelf != hint) mouseHint.gameObject.SetActive(hint);
            if (hint && !hintSet)
            {
                mouseHint.text = gm.Texts.Get("ui.prompts.clickToLook");
                hintSet = true;
            }
        }
    }
}
