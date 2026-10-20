namespace PlanErfuellt
{
    /// <summary>
    /// Funktionen der Nebenzimmer (sideRooms): Kantine (Essen verstärkt die Seren bis Schichtende), Umkleide
    /// (Schutzkittel gegen Ansteckung), Schlafraum (Schicht früher beenden, wenn der Plan erfüllt ist) und
    /// Direktorbüro (Kaviar persönlich: mehr Wirkung, aber er kann brüllen). Reine Regeln.
    /// </summary>
    public static class RoomLogic
    {
        /// <summary>Essen kaufen: kostet den Preis, die Serumstärke steigt auf den Faktor des Gerichts.</summary>
        public static Feedback Eat(GameState s, GameConfig cfg, int meal)
        {
            var meals = cfg.sideRooms.canteen.meals;
            if (meal < 0 || meal >= meals.Length) return Feedback.Error("ui.errors.mealUnknown");
            var m = meals[meal];
            if (m.boost <= s.mealBoost) return Feedback.Error("ui.errors.mealFull");
            if (!s.TrySpend(m.price)) return Feedback.Error("ui.errors.noMoneyMeal");
            s.mealBoost = m.boost;
            return Feedback.Ok;
        }

        /// <summary>Schutzkittel für diese Schicht anziehen.</summary>
        public static Feedback PutOnCoat(GameState s)
        {
            if (s.coatOn) return Feedback.Error("ui.errors.coatAlready");
            s.coatOn = true;
            return Feedback.Ok;
        }

        /// <summary>Hält der Kittel die Ansteckung bei dieser Explosion ab? roll in [0, 1).</summary>
        public static bool CoatBlocks(GameState s, GameConfig cfg, float roll) => s.coatOn && roll < cfg.sideRooms.locker.protectChance;

        /// <summary>Im Schlafraum die Schicht früher beenden – nur mit erfülltem Plan.</summary>
        public static Feedback Sleep(GameState s)
        {
            if (s.quota == null || !s.quota.met) return Feedback.Error("ui.errors.bedNotMet");
            s.quota.left = System.Math.Min(s.quota.left, 0.01f);
            return Feedback.Ok;
        }

        /// <summary>Kaviar persönlich abgeben. shouted: er war schlecht gelaunt und hat gebrüllt.</summary>
        public static Feedback DeliverCaviar(GameState s, GameConfig cfg, float roll, out bool shouted)
        {
            shouted = false;
            if (s.caviar <= 0) return Feedback.Error("ui.errors.noCaviar");
            s.caviar--;
            var d = cfg.sideRooms.desk;
            shouted = roll < d.shoutChance;
            s.AddAnger(shouted ? d.shoutAnger : -d.angerReduce, cfg);
            s.NotifyInventoryChanged();
            return Feedback.Ok;
        }

        /// <summary>Wirkung der Seren mit Kantinen-Schub: Konzentration (≤ 0 = 1) mal Essen.</summary>
        public static float BoostedConc(float conc, float boost) => (conc > 0f ? conc : 1f) * (boost > 0f ? boost : 1f);
    }
}
