using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Mischstation (GDD 4.2) und Serum-Namen/-Farben.</summary>
    public class MixingTests
    {
        GameConfig cfg;
        TextDatabase texts;

        [OneTimeSetUp]
        public void Setup()
        {
            cfg = TestData.Config();
            texts = TestData.Texts();
        }

        [Test]
        public void SliderIsLimitedByStock()
        {
            var s = GameState.Create(cfg); // Lager 3/3/2
            MixingLogic.SetDose(s, cfg, 0, 8);
            MixingLogic.SetDose(s, cfg, 2, 5);
            Assert.AreEqual(new Dose(3, 0, 2), s.mix);
            s.substances[0] = 20;
            MixingLogic.SetDose(s, cfg, 0, 20);
            Assert.AreEqual(cfg.mixing.sliderMax, s.mix.A, "höchstens sliderMax");
        }

        [Test]
        public void MixConsumesStockAndTakesMixTime()
        {
            var s = GameState.Create(cfg);
            MixingLogic.SetDose(s, cfg, 0, 1);
            MixingLogic.SetDose(s, cfg, 1, 1);
            Assert.IsTrue(MixingLogic.TryStartMix(s, cfg).IsOk);
            CollectionAssert.AreEqual(new[] { 2, 2, 2 }, s.substances);
            Assert.AreEqual(Dose.Zero, s.mix, "Regler werden geleert");
            Assert.IsTrue(MixingLogic.IsMixing(s));
            Assert.AreEqual("ui.errors.mixBusy", MixingLogic.TryStartMix(s, cfg).key);

            Assert.IsNull(MixingLogic.Tick(s, cfg, cfg.mixing.mixTimeSec - 0.1f));
            var serum = MixingLogic.Tick(s, cfg, 0.2f);
            Assert.NotNull(serum);
            Assert.AreEqual(new Dose(1, 1, 0), serum.dose);
            Assert.AreEqual(SerumStage.Ready, serum.stage);
            Assert.AreEqual(1, s.serums.Count);
            Assert.IsFalse(MixingLogic.IsMixing(s));
        }

        [Test]
        public void UltimateRecipeIsRaw()
        {
            var s = GameState.Create(cfg);
            s.substances[0] = 2; s.substances[1] = 3; s.substances[2] = 5;
            MixingLogic.SetDose(s, cfg, 0, 2);
            MixingLogic.SetDose(s, cfg, 1, 3);
            MixingLogic.SetDose(s, cfg, 2, 5);
            Assert.IsTrue(MixingLogic.TryStartMix(s, cfg).IsOk);
            var serum = MixingLogic.Tick(s, cfg, 10f);
            Assert.AreEqual(SerumStage.Raw, serum.stage);
        }

        [Test]
        public void Errors()
        {
            var s = GameState.Create(cfg);
            Assert.AreEqual("ui.errors.mixEmpty", MixingLogic.TryStartMix(s, cfg).key);
            for (int i = 0; i < cfg.mixing.rackSize; i++) s.serums.Add(new Serum(new Dose(1, 0, 0), SerumStage.Ready));
            MixingLogic.SetDose(s, cfg, 0, 1);
            Assert.AreEqual("ui.errors.rackFull", MixingLogic.TryStartMix(s, cfg).key);
            Assert.IsFalse(MixingLogic.CanMix(s, cfg));
        }

        [Test]
        public void SerumNameUnknownUntilDiscovered()
        {
            var s = GameState.Create(cfg);
            var serum = new Serum(new Dose(1, 1, 1), SerumStage.Ready);
            Assert.AreEqual("Mischung A1·B1·C1", SerumTexts.Name(serum, cfg, texts, s));
            s.discoveredRecipes.Add("1-1-1");
            Assert.AreEqual("Planformel", SerumTexts.Name(serum, cfg, texts, s));
            serum.conc = 1.5f;
            serum.frozen = true;
            Assert.AreEqual("Planformel · konz. ×1,5 · tiefgekühlt", SerumTexts.Name(serum, cfg, texts, s));
        }

        [Test]
        public void SerumColor()
        {
            var b = SerumTexts.Color(new Serum(new Dose(1, 2, 0), SerumStage.Ready), cfg);
            Assert.AreEqual("3AA0FF", UnityEngine.ColorUtility.ToHtmlStringRGB(b), "stärkster Stoff B");
            var tie = SerumTexts.Color(new Serum(new Dose(2, 2, 0), SerumStage.Ready), cfg);
            Assert.AreEqual("46D14F", UnityEngine.ColorUtility.ToHtmlStringRGB(tie), "Gleichstand → erster Stoff");
            var z = SerumTexts.Color(new Serum(new Dose(2, 3, 5), SerumStage.Centrifuged), cfg);
            Assert.AreEqual("9FE8FF", UnityEngine.ColorUtility.ToHtmlStringRGB(z));
        }
    }
}
