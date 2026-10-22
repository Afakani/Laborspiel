using NUnit.Framework;

namespace PlanErfuellt.Tests
{
    /// <summary>Spielstand: alle wichtigen Teile überstehen Speichern und Laden (JSON).</summary>
    public class SaveGameTests
    {
        [Test]
        public void RoundTripKeepsProgress()
        {
            var cfg = TestData.Config();
            var s = GameState.Create(cfg);
            s.SetMoney(1234.5f);
            s.substances[1] = 7;
            s.antidote = 2;
            s.dayNum = 3;
            s.planIdx = 1;
            s.anger = 42f;
            s.serums.Add(new Serum(new Dose(1, 2, 0), SerumStage.Raw) { conc = 1.5f, frozen = true, ultimateId = UltimateIds.Omega });
            s.machines.Add(new PlacedMachine { typeId = "pump", gx = 3, gz = 1, paid = 400f });
            s.discoveredRecipes.Add("1-2-0");
            s.foundFunRecipes.Add(UltimateIds.SingingCow);
            s.records[RecordLogic.Rate] = 3.9f;
            s.infected = true;
            s.infectedUntilDay = 5;
            s.totals.m = 280f;
            s.newsFacts.Add("newsFacts.flyAway");
            // ±Unendlich ist kein gültiges JSON-Zahlenformat und muss trotzdem durchkommen
            Assert.IsTrue(float.IsNegativeInfinity(s.inspectionDoneAt));

            var d = SaveGame.FromJson(SaveGame.ToJson(s));
            Assert.IsNotNull(d);
            var l = d.state;
            Assert.AreEqual(1234.5f, l.Money, 0.001f);
            Assert.AreEqual(7, l.substances[1]);
            Assert.AreEqual(2, l.antidote);
            Assert.AreEqual(3, l.dayNum);
            Assert.AreEqual(1, l.planIdx);
            Assert.AreEqual(42f, l.anger, 0.001f);
            Assert.AreEqual(1, l.serums.Count);
            Assert.AreEqual(new Dose(1, 2, 0), l.serums[0].dose);
            Assert.AreEqual(UltimateIds.Omega, l.serums[0].ultimateId);
            Assert.IsTrue(l.serums[0].frozen);
            Assert.AreEqual(1, l.machines.Count);
            Assert.AreEqual("pump", l.machines[0].typeId);
            Assert.AreEqual(3, l.machines[0].gx);
            CollectionAssert.Contains(l.discoveredRecipes, "1-2-0");
            CollectionAssert.Contains(l.foundFunRecipes, UltimateIds.SingingCow);
            Assert.AreEqual(3.9f, l.records[RecordLogic.Rate], 0.001f);
            Assert.IsTrue(l.infected);
            Assert.AreEqual(5, l.infectedUntilDay);
            Assert.AreEqual(280f, l.totals.m, 0.001f);
            Assert.AreEqual("newsFacts.flyAway", l.newsFacts[0]);
            Assert.IsTrue(float.IsNegativeInfinity(l.inspectionDoneAt));
            Assert.IsTrue(float.IsPositiveInfinity(l.nextInspectionAt));
        }

        [Test]
        public void RoundTripKeepsNewerFields()
        {
            // Felder aus den Erweiterungen (Nebenzimmer, Bestellung für morgen, Hinweise, Warnungen, Maschinen)
            var cfg = TestData.Config();
            var s = GameState.Create(cfg);
            s.mealBoost = 1.3f;
            s.coatOn = true;
            s.pendingOrder = new[] { 3, 5, 0, 0, 0, 0, 1 };
            s.lastOrder = new[] { 1, 2, 3, 0, 0, 0, 0 };
            s.hintsOn = true;
            s.hintsSeen.Add("belt");
            s.hint = "inspection";
            s.angerWarned = 2;
            s.timeWarnedDay = 4;
            s.inspectionAnnounced = true;
            s.beltBroken = true;
            s.cloths = 3;
            s.probedRecipes.Add("2-0-1");
            s.machines.Add(new PlacedMachine { typeId = BuildLogic.Layer, gx = 5, gz = 2, paid = 400f });
            s.centrifuge = new CentrifugeJob { serum = new Serum(new Dose(1, 1, 0), SerumStage.Raw), rpm = 0.4f };
            s.cryo = new CryoJob { serum = new Serum(new Dose(2, 3, 5), SerumStage.Centrifuged), freezeLeft = 3f, windowLeft = 12f };

            var l = SaveGame.FromJson(SaveGame.ToJson(s)).state;
            Assert.AreEqual(1.3f, l.mealBoost, 0.001f);
            Assert.IsTrue(l.coatOn);
            CollectionAssert.AreEqual(s.pendingOrder, l.pendingOrder);
            CollectionAssert.AreEqual(s.lastOrder, l.lastOrder);
            Assert.IsTrue(l.hintsOn);
            CollectionAssert.Contains(l.hintsSeen, "belt");
            Assert.AreEqual("inspection", l.hint);
            Assert.AreEqual(2, l.angerWarned);
            Assert.AreEqual(4, l.timeWarnedDay);
            Assert.IsTrue(l.inspectionAnnounced);
            Assert.IsTrue(l.beltBroken);
            Assert.AreEqual(3, l.cloths);
            CollectionAssert.Contains(l.probedRecipes, "2-0-1");
            Assert.AreEqual(BuildLogic.Layer, l.machines[0].typeId);
            Assert.AreEqual(0.4f, l.centrifuge.rpm, 0.001f);
            Assert.AreEqual(new Dose(2, 3, 5), l.cryo.serum.dose);
            Assert.AreEqual(3f, l.cryo.freezeLeft, 0.001f);
        }

        [Test]
        public void BrokenOrOldFileIsIgnored()
        {
            Assert.IsNull(SaveGame.FromJson("kein json"));
            Assert.IsNull(SaveGame.FromJson("{\"version\": 0, \"state\": {}}"));
        }
    }
}
