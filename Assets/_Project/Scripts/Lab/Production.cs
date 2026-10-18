namespace PlanErfuellt
{
    /// <summary>
    /// Lieferung von Waren (Prototyp deliver): Bezahlung pro Tonne mit Quote, Übererfüllung, Sonderauftrag und
    /// Sonderwunsch (DayLogic.Pay), Gesamtmengen, Durchschnittsqualität.
    /// </summary>
    public static class Production
    {
        /// <summary>Ware: 'm' Milch, 'e' Eier, 'f' Fleisch.</summary>
        public static void Deliver(GameState s, GameConfig cfg, char good, float tons, float quality)
        {
            if (tons <= 0f) return;
            float pay = DayLogic.Pay(s, cfg, good, tons, quality, out bool newlyOver);
            Add(s.totals, good, tons);
            s.qualityTons += tons * quality;
            s.deliveredTons += tons;
            s.earned += pay;
            s.AddMoney(pay);
            EventBus.RaiseDelivered(good, tons);
            if (newlyOver) EventBus.RaiseOverfulfilled(good);
        }

        public static float PayPerTon(GameConfig cfg, char good) =>
            good == 'm' ? cfg.day.payPerTon.m : good == 'e' ? cfg.day.payPerTon.e : cfg.day.payPerTon.f;

        /// <summary>Durchschnittsqualität aller Lieferungen (Start world.avgQStart).</summary>
        public static float AverageQuality(GameState s, GameConfig cfg) =>
            s.deliveredTons > 0f ? s.qualityTons / s.deliveredTons : cfg.world.avgQStart;

        static void Add(GoodsValues g, char good, float v)
        {
            if (good == 'm') g.m += v;
            else if (good == 'e') g.e += v;
            else g.f += v;
        }
    }
}
