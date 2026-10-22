namespace PlanErfuellt
{
    /// <summary>
    /// Geld-Zahlen an der Kasse (Gruppe 2): Einnahmen kommen bei Milch und Eiern in vielen kleinen Beträgen und werden
    /// gesammelt, höchstens alle feel.moneyIntervalSec und ab feel.moneyMin gezeigt; Ausgaben erscheinen sofort.
    /// </summary>
    public class MoneyPopupLogic
    {
        float gained, timer;

        /// <summary>Kassenänderung melden. Ausgaben (negativ) werden sofort zurückgegeben, sonst 0.</summary>
        public float Add(float delta)
        {
            if (delta < 0f) return delta;
            gained += delta;
            return 0f;
        }

        /// <summary>Zeitschritt: gesammelte Einnahmen, wenn sie gezeigt werden sollen, sonst 0.</summary>
        public float Tick(float dt, FeelConfig cfg)
        {
            timer += dt;
            if (timer < cfg.moneyIntervalSec || gained < cfg.moneyMin) return 0f;
            float shown = gained;
            gained = 0f;
            timer = 0f;
            return shown;
        }

        /// <summary>Gesammeltes verwerfen (Laden, neues Spiel).</summary>
        public void Clear()
        {
            gained = 0f;
            timer = 0f;
        }
    }
}
