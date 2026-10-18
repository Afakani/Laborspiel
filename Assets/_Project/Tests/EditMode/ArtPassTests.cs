using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Art-Pass: Plakattexte aus der JSON (posters) werden richtig zerlegt.</summary>
    public class ArtPassTests
    {
        [Test]
        public void PosterHeadAndSub()
        {
            PosterText.Split("MEHR|MILCH – für die Republik", false, out var h, out var s);
            Assert.AreEqual("MEHR\nMILCH", h);
            Assert.AreEqual("für die Republik", s);
        }

        [Test]
        public void PosterDotsBreakLines()
        {
            PosterText.Split("MEHR NAHRUNG · STÄRKERE MENSCHEN", false, out var h, out var s);
            Assert.AreEqual("MEHR NAHRUNG\nSTÄRKERE MENSCHEN", h);
            Assert.AreEqual("", s);
        }

        [Test]
        public void WarnSignFirstLineBig()
        {
            PosterText.Split("ACHTUNG · MUTAGENE STOFFE · NICHT TRINKEN", true, out var h, out var s);
            Assert.AreEqual("ACHTUNG", h);
            Assert.AreEqual("MUTAGENE STOFFE\nNICHT TRINKEN", s);
        }

        [Test]
        public void AllPostersFromJsonHaveHeadline()
        {
            var texts = TestData.Texts();
            int n = texts.Count("posters");
            Assert.Greater(n, 1);
            for (int i = 0; i < n; i++)
            {
                PosterText.Split(texts.Get($"posters[{i}]"), i == n - 1, out var h, out _);
                Assert.IsFalse(string.IsNullOrWhiteSpace(h), "Plakat " + i);
            }
        }
    }
}
