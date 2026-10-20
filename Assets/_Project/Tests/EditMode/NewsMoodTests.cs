using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Stimmung des Sprechers aus Tagesergebnis und Lage.</summary>
    public class NewsMoodTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void PlanMetMakesHimHappy()
        {
            var s = GameState.Create(cfg);
            s.lastDayResult = 1;
            Assert.AreEqual(NewsMood.Joy, NewsLogic.Mood(s, NewsLevel.Boom));
            Assert.AreEqual(NewsMood.Calm, NewsLogic.Mood(s, NewsLevel.Crisis), "in der Krise nur gefasst");
        }

        [Test]
        public void FailureMakesHimSweatThenCry()
        {
            var s = GameState.Create(cfg);
            s.lastDayResult = -1;
            s.failedDays = 1;
            Assert.AreEqual(NewsMood.Sweat, NewsLogic.Mood(s, NewsLevel.Boom));
            s.failedDays = 2;
            Assert.AreEqual(NewsMood.Cry, NewsLogic.Mood(s, NewsLevel.Boom));
        }

        [Test]
        public void WithoutResultTheSituationDecides()
        {
            var s = GameState.Create(cfg);
            Assert.AreEqual(0, s.lastDayResult);
            Assert.AreEqual(NewsMood.Calm, NewsLogic.Mood(s, NewsLevel.Boom));
            Assert.AreEqual(NewsMood.Sweat, NewsLogic.Mood(s, NewsLevel.Warn));
            Assert.AreEqual(NewsMood.Cry, NewsLogic.Mood(s, NewsLevel.Crisis));
        }

        [Test]
        public void ComposeSetsTheMood()
        {
            var s = GameState.Create(cfg);
            s.lastDayResult = 1;
            var copy = NewsLogic.Compose(s, cfg, TestData.Texts(), () => 0f);
            Assert.AreEqual(NewsLogic.Mood(s, copy.level), copy.mood);
        }
    }
}
