using NUnit.Framework;
using UnityEngine;

namespace PlanErfuellt.Tests
{
    /// <summary>Einführungspfeile (Weg durch Türen) und UI-Skalierung je Bildformat.</summary>
    public class GuidePathTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void HallToHallIsStraight()
        {
            var pts = GuidePath.Waypoints(cfg.layout, new Vector2(0f, 8f), new Vector2(-5f, 4f));
            Assert.AreEqual(2, pts.Count);
        }

        [Test]
        public void IntoTheStorageGoesThroughItsDoor()
        {
            var L = cfg.layout;
            var term = L.Station("terminal");
            var storage = System.Array.Find(L.rooms, r => r.id == "storage");
            var pts = GuidePath.Waypoints(L, new Vector2(0f, 8f), new Vector2(term.x, term.z));
            Assert.AreEqual(4, pts.Count);
            Assert.AreEqual(storage.doorX, pts[1].x, 0.001f);
            Assert.Less(pts[1].y, L.room.backWallZ);
            Assert.Greater(pts[2].y, L.WingZMin);
            // jeder Abschnitt liegt in der begehbaren Fläche
            var rects = WalkArea.Rects(L);
            foreach (var (p, _) in GuidePath.Samples(pts, 0.5f, 0.5f, 0.5f, 200))
                Assert.AreEqual(p, WalkArea.Clamp(rects, p), p.ToString());
        }

        [Test]
        public void SamplesAreSpacedAndCapped()
        {
            var pts = new System.Collections.Generic.List<Vector2> { Vector2.zero, new Vector2(0f, 10f) };
            var s = GuidePath.Samples(pts, 1f, 2f, 1f, 50);
            Assert.AreEqual(5, s.Count);
            Assert.AreEqual(1f, s[0].pos.y, 0.001f);
            Assert.AreEqual(9f, s[4].pos.y, 0.001f);
            Assert.AreEqual(3, GuidePath.Samples(pts, 1f, 2f, 1f, 3).Count);
        }

        [Test]
        public void CanvasMatchesByFormat()
        {
            float r = 16f / 9f;
            Assert.AreEqual(0f, CanvasAspectFit.Match(16f / 10f, r), "Laptop 16:10 nach Breite");
            Assert.AreEqual(0f, CanvasAspectFit.Match(4f / 3f, r));
            Assert.AreEqual(1f, CanvasAspectFit.Match(16f / 9f, r));
            Assert.AreEqual(1f, CanvasAspectFit.Match(21f / 9f, r), "Breitbild nach Höhe");
        }
    }
}
