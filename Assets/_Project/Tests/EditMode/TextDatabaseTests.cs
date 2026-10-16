using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Textzugriff, Wut-Stufen und Platzhalter.</summary>
    public class TextDatabaseTests
    {
        TextDatabase texts;

        [OneTimeSetUp]
        public void Setup() => texts = TestData.Texts();

        [Test]
        public void GetSimpleText()
        {
            Assert.AreEqual("Plan verfehlt! Der Tag wird wiederholt.", texts.Get("ui.toasts.planFailed"));
            Assert.AreEqual("DANKE, GENOSSE BIOCHEMIKER!", texts.Get("windowBanner"));
        }

        [Test]
        public void FillReplacesPlaceholdersGerman()
        {
            Assert.AreEqual("Verarbeitung: +10,8 t Fleisch", texts.Format("ui.toasts.processed", ("n", 10.8f)));
            Assert.AreEqual("A, B", TextDatabase.Fill("{x}, {y}", ("x", "A"), ("y", "B")));
        }

        [TestCase(0f, 0)]
        [TestCase(33.9f, 0)]
        [TestCase(34f, 1)]
        [TestCase(66.9f, 1)]
        [TestCase(67f, 2)]
        [TestCase(100f, 2)]
        public void AngerTiers(float anger, int tier)
        {
            Assert.AreEqual(tier, texts.AngerTier(anger));
        }

        [Test]
        public void PickByAngerUsesTierList()
        {
            var calm = texts.GetList("director.shouts[0]");
            var mad = texts.GetList("director.shouts[2]");
            for (int i = 0; i < 20; i++)
            {
                CollectionAssert.Contains(calm, texts.PickByAnger("director.shouts", 10f));
                CollectionAssert.Contains(mad, texts.PickByAnger("director.shouts", 90f));
            }
        }

        [Test]
        public void PickByAngerFallsBackForFlatList()
        {
            var list = texts.GetList("director.unstable");
            CollectionAssert.Contains(list, texts.PickByAnger("director.unstable", 80f));
        }

        [Test]
        public void MissingTextIsMarked()
        {
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("Text fehlt"));
            Assert.AreEqual("[ui.gibtEsNicht]", texts.Get("ui.gibtEsNicht"));
        }

        [Test]
        public void ObsoletePrototypeTextsRemoved()
        {
            Assert.IsFalse(texts.Has("prototypeTexts"));
            Assert.IsTrue(texts.Has("news.boom"));
            Assert.IsTrue(texts.Has("endings.fail.title"));
        }

        [Test]
        public void ComboTextKeysExist()
        {
            foreach (var key in new[] { "baseStabilizer", "baseFatten", "baseMilk", "baseCombi", "suffixStabilized", "suffixExtreme",
                         "proMeat", "proRate", "proQuality", "conQuality", "proStabilized", "conDampened", "conLowStab",
                         "conHeat", "conAcid", "conFloat", "conBelt", "conNoGain", "proFallback", "conFallback" })
                Assert.IsTrue(texts.Has("ui.combo." + key), key);
        }
    }
}
