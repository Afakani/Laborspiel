using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Wanduhr an der Rückwand (Prototyp clockHand): der Zeiger zeigt den Fortschritt der Schicht.</summary>
    public class WallClock : MonoBehaviour
    {
        [SerializeField] Transform hand;

        public void Setup(Transform handTransform) => hand = handTransform;

        void Update()
        {
            var gm = GameManager.Instance;
            var q = gm != null && gm.State != null ? gm.State.quota : null;
            float k = q != null && q.duration > 0f ? 1f - q.left / q.duration : 0f;
            hand.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Clamp01(k) * 360f);
        }
    }
}
