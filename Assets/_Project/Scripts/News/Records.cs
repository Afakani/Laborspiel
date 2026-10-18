using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Meldet neue Rekorde im Spiel: Kurzmeldung „NEUER REKORD: …“ und manchmal der Direktor (GDD 12).</summary>
    public static class Records
    {
        public static void Report(string key, float value)
        {
            var gm = GameManager.Instance;
            if (gm == null || !RecordLogic.Check(gm.State, gm.Config, key, value)) return;
            string text = gm.Texts.Format(RecordLogic.ToastPath(key), ("n", RecordLogic.FormatValue(key, value)));
            gm.Toast("ui.toasts.record", ToastKind.Good, ("text", text));
            EventBus.RaiseStation("record");
            if (Random.value < gm.Config.records.directorChance)
            {
                var director = Object.FindAnyObjectByType<Director>();
                if (director != null) director.SayAny("director.record");
            }
        }
    }
}
