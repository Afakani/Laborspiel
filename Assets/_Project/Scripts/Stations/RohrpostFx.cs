using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Rohrpost sichtbar (Prototyp rohrCap): Die Kapsel saust nach dem Absenden das Rohr hoch.</summary>
    public class RohrpostFx : MonoBehaviour
    {
        [SerializeField] Transform capsule;
        [SerializeField] float startY = 1.4f;
        [SerializeField] float endY = 8f;

        public void Setup(Transform capsuleT, float fromY, float toY)
        {
            capsule = capsuleT;
            startY = fromY;
            endY = toY;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            float left = gm.State.rohrpostLeft;
            bool flying = left > 0f;
            if (capsule.gameObject.activeSelf != flying) capsule.gameObject.SetActive(flying);
            if (!flying) return;
            float k = 1f - left / gm.Config.machinesFixed.rohrpost.capsuleSec;
            var p = capsule.localPosition;
            capsule.localPosition = new Vector3(p.x, Mathf.Lerp(startY, endY, k * k), p.z);
        }
    }
}
