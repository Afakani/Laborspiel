using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Hallen-Durchsagen der Partei (D157): Gong und Stimme (Tools/gen_announce.py, Datei announce_&lt;Schlüssel&gt;) mit
    /// Untertitel „DURCHSAGE: …“. Anlass über EventBus.Announce (Kommissar kommt gleich, tritt ein, ist fertig) und
    /// AnnounceLogic (Parteikontrolle steigt). Nacheinander, nie übereinander; das Eintreten des Kommissars unterbricht.
    /// </summary>
    public class Announcer : MonoBehaviour
    {
        readonly Queue<string> queue = new Queue<string>();
        float busy;

        void OnEnable() => EventBus.Announce += OnAnnounce;
        void OnDisable() => EventBus.Announce -= OnAnnounce;

        void OnAnnounce(string key)
        {
            if (key == AnnounceLogic.Arrive || key == AnnounceLogic.ArriveBoss)
            {
                queue.Clear();
                busy = 0f;
            }
            if (!queue.Contains(key)) queue.Enqueue(key);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            var dc = DayCycle.Instance;
            if (gm == null || dc == null || gm.Clock.Paused) return;
            if (dc.Mode == DayMode.Play)
            {
                string key = AnnounceLogic.Tick(gm.State, gm.Config);
                if (key != null) OnAnnounce(key);
            }
            else queue.Clear();

            busy -= Time.unscaledDeltaTime;
            if (busy > 0f || queue.Count == 0) return;
            string next = queue.Dequeue();
            float len = AudioService.Instance != null ? AudioService.Instance.PlayAnnounce("announce_" + next, gm.Config.announce.volume) : 0f;
            EventBus.RaiseAnnounceLine(gm.Texts.Get("announce." + next));
            busy = len + gm.Config.announce.gapSec;
        }
    }
}
