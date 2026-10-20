using NUnit.Framework;
using UnityEngine;

namespace PlanErfuellt.Tests
{
    /// <summary>Begehbare Fläche: Halle, Türen in der Rückwand und Nebenzimmer (Anbau).</summary>
    public class WalkAreaTests
    {
        GameConfig cfg;

        [OneTimeSetUp]
        public void Setup() => cfg = TestData.Config();

        [Test]
        public void RoomsAreConfigured()
        {
            var L = cfg.layout;
            Assert.AreEqual(5, L.rooms.Length);
            CollectionAssert.AreEquivalent(new[] { "storage", "locker", "bedroom", "canteen", "director" }, System.Array.ConvertAll(L.rooms, r => r.id));
            foreach (var r in L.rooms)
            {
                Assert.Less(r.xMin, r.doorX - L.wing.doorWidth * 0.5f, r.id);
                Assert.Greater(r.xMax, r.doorX + L.wing.doorWidth * 0.5f, r.id);
            }
            // Terminal und Lagerausgabe stehen im Lager
            var storage = System.Array.Find(L.rooms, r => r.id == "storage");
            foreach (var id in new[] { "terminal", "storage" })
            {
                var s = L.Station(id);
                Assert.AreEqual(storage, WalkArea.RoomAt(L, new Vector2(s.x, s.z)), id);
            }
        }

        [Test]
        public void HallPointsStay()
        {
            var p = new Vector2(3f, 8f);
            Assert.AreEqual(p, WalkArea.Clamp(cfg.layout, p));
        }

        [Test]
        public void WallBetweenDoorsBlocks()
        {
            var L = cfg.layout;
            // Mitten in der Rückwand zwischen zwei Türen: zurück in die Halle oder ins Zimmer, nicht in der Wand
            float wallZ = L.room.backWallZ + L.room.wallThickness * 0.5f;
            var q = WalkArea.Clamp(L, new Vector2(-9.5f, wallZ));
            Assert.IsTrue(q.y <= L.playerArea.zMax + 0.001f || q.y >= L.WingZMin, q.ToString());
        }

        [Test]
        public void DoorLeadsIntoRoom()
        {
            var L = cfg.layout;
            foreach (var r in L.rooms)
            {
                // Schritt für Schritt durch die Tür bis in die Zimmermitte
                var p = new Vector2(r.doorX, L.playerArea.zMax - 0.5f);
                for (int i = 0; i < 60; i++)
                    p = WalkArea.Clamp(L, p + new Vector2(0f, 0.15f));
                Assert.Greater(p.y, L.WingZMin + 1f, r.id);
                Assert.AreEqual(r, WalkArea.RoomAt(L, p), r.id);
            }
        }

        [Test]
        public void RoomsAreClosedAtTheBackAndSides()
        {
            var L = cfg.layout;
            var q = WalkArea.Clamp(L, new Vector2(-20f, L.WingZMax + 3f));
            Assert.Less(q.y, L.WingZMax);
            Assert.Greater(q.x, -L.room.sideWallX);
            // Trennwand zwischen zwei Zimmern ist nicht begehbar
            var locker = System.Array.Find(L.rooms, r => r.id == "locker");
            var w = WalkArea.Clamp(L, new Vector2(locker.xMax, (L.WingZMin + L.WingZMax) * 0.5f));
            Assert.Greater(Mathf.Abs(w.x - locker.xMax), L.wing.partition * 0.5f);
        }
    }
}
