using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Rotlicht in der Halle (D157): solange der Kommissar vor der Tür steht (inspection.arriveSec), pulsieren rote Lampen
    /// unter der Decke wie eine Rundumleuchte. Farbe, Stärke und Takt aus announce.
    /// </summary>
    public class HallAlarm : MonoBehaviour
    {
        [SerializeField] Light[] lights = new Light[0];

        bool lit;

        public void Setup(Light[] all) => lights = all;

        void Start()
        {
            var a = GameManager.Instance.Config.announce;
            ColorUtility.TryParseHtmlString(a.lightColor, out var c);
            foreach (var l in lights)
            {
                l.color = c;
                l.range = a.lightRange;
            }
            Set(false, 0f);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            var ins = gm.State.inspection;
            if (ins == null || !ins.Arriving)
            {
                if (lit) Set(false, 0f);
                return;
            }
            var a = gm.Config.announce;
            float k = 0.5f + 0.5f * Mathf.Sin(gm.State.playTime * a.lightPulseHz * 2f * Mathf.PI);
            Set(true, a.lightIntensity * (0.2f + 0.8f * k));
        }

        void Set(bool on, float intensity)
        {
            lit = on;
            foreach (var l in lights)
            {
                l.enabled = on;
                l.intensity = intensity;
            }
        }
    }
}
