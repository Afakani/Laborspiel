using UnityEngine;
using UnityEngine.UI;

namespace PlanErfuellt
{
    /// <summary>
    /// Roter Lichtschein über dem Bild, solange der Kommissar vor der Tür steht (D157): pulsiert im Takt der roten
    /// Hallenlampen (HallAlarm), Stärke announce.tintAlpha.
    /// </summary>
    public class AlarmTintUI : MonoBehaviour
    {
        [SerializeField] RawImage tint;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var ins = gm.State.inspection;
            float alpha = 0f;
            if (ins != null && ins.Arriving)
            {
                var a = gm.Config.announce;
                float k = 0.5f + 0.5f * Mathf.Sin(gm.State.playTime * a.lightPulseHz * 2f * Mathf.PI);
                alpha = a.tintAlpha * (0.2f + 0.8f * k);
            }
            if (Mathf.Approximately(tint.color.a, alpha)) return;
            var c = tint.color;
            c.a = alpha;
            tint.color = c;
            tint.enabled = alpha > 0f;
        }
    }
}
