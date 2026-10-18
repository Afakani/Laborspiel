using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Plane über dem Serum-Regal der Mischstation (GDD 9): senkt sich, wenn das Regal abgedeckt ist.</summary>
    public class RackCover : MonoBehaviour
    {
        [SerializeField] Transform cover;

        float k;

        public void Setup(Transform coverPivot) => cover = coverPivot;

        void Awake() => cover.gameObject.SetActive(false);

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            k = Mathf.Lerp(k, gm.State.rackHidden ? 1f : 0f, Mathf.Min(1f, Time.deltaTime * gm.Config.clothFx.speed));
            bool on = k > 0.02f;
            if (cover.gameObject.activeSelf != on) cover.gameObject.SetActive(on);
            if (!on) return;
            var s = cover.localScale;
            cover.localScale = new Vector3(s.x, Mathf.Max(0.02f, k), s.z);
        }
    }
}
