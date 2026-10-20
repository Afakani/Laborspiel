using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Nebenzimmer: Kantine (Serumstärke), Umkleide (Kittel), Schlafraum (früher Feierabend), Direktorbüro (Kaviar).</summary>
    public class RoomTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void MealsCostBetween200And500AndBoostUpTo15()
        {
            var meals = cfg.sideRooms.canteen.meals;
            Assert.AreEqual(3, meals.Length);
            foreach (var m in meals)
            {
                Assert.That(m.price, Is.InRange(200f, 500f));
                Assert.That(m.boost, Is.InRange(1.1f, 1.5f));
            }
            Assert.AreEqual(1.1f, meals[0].boost, 0.001f);
            Assert.AreEqual(1.5f, meals[meals.Length - 1].boost, 0.001f);
        }

        [Test]
        public void EatingBoostsSerumsUntilTheShiftEnds()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(1000f);
            Assert.IsTrue(RoomLogic.Eat(s, cfg, 0).IsOk);
            Assert.AreEqual(1000f - cfg.sideRooms.canteen.meals[0].price, s.Money, 0.01f);
            Assert.AreEqual(1.1f, s.mealBoost, 0.001f);
            Assert.IsFalse(RoomLogic.Eat(s, cfg, 0).IsOk, "gleiches Gericht hilft nicht mehr");
            Assert.IsTrue(RoomLogic.Eat(s, cfg, 2).IsOk, "besseres Gericht ersetzt");
            Assert.AreEqual(1.5f, s.mealBoost, 0.001f);
            DayLogic.StartDay(s, cfg, () => 0.5f);
            Assert.AreEqual(1f, s.mealBoost, "neue Schicht, neuer Hunger");
        }

        [Test]
        public void MealNeedsMoney()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(150f);
            Assert.IsFalse(RoomLogic.Eat(s, cfg, 0).IsOk);
            Assert.AreEqual(1f, s.mealBoost);
        }

        [Test]
        public void BoostScalesTheSerumGain()
        {
            var d = new Dose(1, 1, 0);
            float b = cfg.Animal(AnimalKind.Cow).baseRate;
            float plain = Formula.AnimalRate(AnimalKind.Cow, d, null, RoomLogic.BoostedConc(0f, 1f), cfg);
            float fed = Formula.AnimalRate(AnimalKind.Cow, d, null, RoomLogic.BoostedConc(0f, 1.5f), cfg);
            Assert.AreEqual((plain - b) * 1.5f, fed - b, 0.001f);
            Assert.AreEqual(1.5f * 1.5f, RoomLogic.BoostedConc(1.5f, 1.5f), 0.001f, "mit Zentrifuge multipliziert");
        }

        [Test]
        public void CoatProtectsOneShift()
        {
            var s = GameState.Create(cfg);
            Assert.IsFalse(RoomLogic.CoatBlocks(s, cfg, 0f));
            Assert.IsTrue(RoomLogic.PutOnCoat(s).IsOk);
            Assert.IsFalse(RoomLogic.PutOnCoat(s).IsOk);
            Assert.IsTrue(RoomLogic.CoatBlocks(s, cfg, cfg.sideRooms.locker.protectChance - 0.01f));
            Assert.IsFalse(RoomLogic.CoatBlocks(s, cfg, cfg.sideRooms.locker.protectChance + 0.01f));
            DayLogic.StartDay(s, cfg, () => 0.5f);
            Assert.IsFalse(s.coatOn);
        }

        [Test]
        public void SleepOnlyWithPlanMet()
        {
            var s = GameState.Create(cfg);
            DayLogic.StartDay(s, cfg, () => 0.5f);
            Assert.IsFalse(RoomLogic.Sleep(s).IsOk);
            s.quota.met = true;
            Assert.IsTrue(RoomLogic.Sleep(s).IsOk);
            Assert.LessOrEqual(s.quota.left, 0.01f);
        }

        [Test]
        public void CaviarInPersonIsStrongerButRisky()
        {
            var s = GameState.Create(cfg);
            s.caviar = 2;
            s.anger = 60f;
            Assert.IsTrue(RoomLogic.DeliverCaviar(s, cfg, 0.99f, out bool shouted).IsOk);
            Assert.IsFalse(shouted);
            Assert.AreEqual(60f - cfg.sideRooms.desk.angerReduce, s.anger, 0.01f);
            Assert.Greater(cfg.sideRooms.desk.angerReduce, -cfg.anger.caviar, "stärker als die Rohrpost");
            Assert.IsTrue(RoomLogic.DeliverCaviar(s, cfg, 0f, out shouted).IsOk);
            Assert.IsTrue(shouted);
            Assert.AreEqual(0, s.caviar);
            Assert.IsFalse(RoomLogic.DeliverCaviar(s, cfg, 0.5f, out _).IsOk);
        }
    }
}
