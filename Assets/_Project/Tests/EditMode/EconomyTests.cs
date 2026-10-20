using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Neues Balancing: Bestellen für morgen, Standardlieferung, Laken als Verbrauchsartikel, Klonkosten.</summary>
    public class EconomyTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void OrderIsPaidNowAndDeliveredNextMorning()
        {
            var s = GameState.Create(cfg);
            float money = s.Money;
            var order = new int[OrderLogic.Items];
            order[1] = 2;
            order[6] = 1;
            Assert.IsTrue(OrderLogic.Place(s, cfg, order));
            Assert.AreEqual(money - 2 * cfg.prices.B - cfg.prices.cloth, s.Money, 0.01f, "sofort bezahlt");
            int b = s.substances[1], cloths = s.cloths;
            Assert.IsTrue(OrderLogic.Place(s, cfg, order), "zweite Bestellung addiert sich");
            var (got, standard) = OrderLogic.Deliver(s, cfg);
            Assert.IsFalse(standard);
            Assert.AreEqual(4, got[1]);
            Assert.AreEqual(b + 4, s.substances[1]);
            Assert.AreEqual(cloths + 2, s.cloths);
            Assert.IsNull(s.pendingOrder);
        }

        [Test]
        public void StandardOrderWhenNothingOrdered()
        {
            var s = GameState.Create(cfg);
            float money = s.Money;
            int a = s.substances[0];
            var (got, standard) = OrderLogic.Deliver(s, cfg);
            Assert.IsTrue(standard);
            CollectionAssert.AreEqual(cfg.day.standardOrder, got);
            Assert.AreEqual(a + cfg.day.standardOrder[0], s.substances[0]);
            Assert.AreEqual(money - OrderLogic.Sum(cfg, cfg.day.standardOrder), s.Money, 0.01f);
        }

        [Test]
        public void StandardOrderShrinksToCash()
        {
            var s = GameState.Create(cfg);
            s.SetMoney(cfg.prices.A);
            var (got, _) = OrderLogic.Deliver(s, cfg);
            Assert.LessOrEqual(OrderLogic.Sum(cfg, got), cfg.prices.A);
            Assert.GreaterOrEqual(s.Money, 0f);
        }

        [Test]
        public void ClothIsUsedUp()
        {
            var s = GameState.Create(cfg);
            s.cloths = 1;
            Assert.AreEqual(ClothLogic.Result.Covered, ClothLogic.Toggle(s));
            Assert.AreEqual(0, s.cloths);
            Assert.IsTrue(s.chamberCovered);
            Assert.AreEqual(ClothLogic.Result.Removed, ClothLogic.Toggle(s));
            Assert.IsFalse(s.chamberCovered);
            Assert.AreEqual(ClothLogic.Result.NoCloth, ClothLogic.Toggle(s), "zerfressen, kein zweites Mal");
            Assert.IsFalse(s.chamberCovered);
        }

        [Test]
        public void BalancingDirections()
        {
            Assert.Greater(cfg.prices.cloneCow, cfg.prices.cloneHen);
            Assert.Greater(OrderLogic.CloneCost(cfg, AnimalKind.Cow), 0f);
            Assert.GreaterOrEqual(cfg.day.overfulfilPayBonus, 1f, "Überschuss zahlt mindestens doppelt");
            Assert.AreEqual(300f, cfg.day.dayLengthSec, "Schicht 5 Minuten");
            Assert.AreEqual(OrderLogic.Items, cfg.day.standardOrder.Length);
            Assert.Greater(cfg.start.cloths, 0);
        }
    }
}
