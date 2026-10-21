using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Baumodus als reine Regeln (GDD 6.10, Prototyp mPrice/placeMachine/buildClick/machineTick):
    /// Raster, Preise ×priceFactor pro gleichem Typ, Freischaltung, Platzieren, Abriss, Wirkung der Maschinen.
    /// Koordinaten in Prototyp-Werten (x, z).
    /// </summary>
    public static class BuildLogic
    {
        public const string Pump = "pump";
        public const string Layer = "layer";
        public const string Spray = "spray";
        public const string Cool = "cool";
        public const string Doc = "doc";

        public static BuildMachineType Type(GameConfig cfg, string id) => cfg.buildMachineTypes.Find(t => t.id == id);

        public static int Count(GameState s, string id)
        {
            int n = 0;
            foreach (var m in s.machines) if (m.typeId == id) n++;
            return n;
        }

        /// <summary>Preis des nächsten Exemplars: Grundpreis × priceFactor^Anzahl.</summary>
        public static float Price(GameState s, GameConfig cfg, BuildMachineType t) =>
            Mathf.Round(t.price * Mathf.Pow(cfg.buildMachines.priceFactor, Count(s, t.id)));

        public static bool Unlocked(GameState s, BuildMachineType t) => s.planIdx >= t.unlockPlanIdx;

        // ---------- Raster ----------

        public static bool InGrid(GameConfig cfg, int gx, int gz)
        {
            var g = cfg.buildMachines.grid;
            return gx >= 0 && gx < g.cols && gz >= 0 && gz < g.rows;
        }

        /// <summary>Gesperrte Felder (Gang zum Pult).</summary>
        public static bool Blocked(GameConfig cfg, int gx, int gz)
        {
            var g = cfg.buildMachines.grid;
            return gz == g.blockedRow && gx >= g.blockedFrom && gx <= g.blockedTo;
        }

        /// <summary>Feldmitte in Prototyp-Koordinaten.</summary>
        public static Vector2 CellCenter(GameConfig cfg, int gx, int gz)
        {
            var g = cfg.buildMachines.grid;
            return new Vector2(g.x0 + g.cell * gx, g.z0 + g.cell * gz);
        }

        /// <summary>Feld unter einem Punkt (Prototyp-Koordinaten). false = außerhalb des Rasters.</summary>
        public static bool CellAt(GameConfig cfg, Vector2 p, out int gx, out int gz)
        {
            var g = cfg.buildMachines.grid;
            gx = Mathf.FloorToInt((p.x - g.x0) / g.cell + 0.5f);
            gz = Mathf.FloorToInt((p.y - g.z0) / g.cell + 0.5f);
            return InGrid(cfg, gx, gz);
        }

        public static PlacedMachine At(GameState s, int gx, int gz) => s.machines.Find(m => m.gx == gx && m.gz == gz);

        /// <summary>Ist das Feld frei und baubar?</summary>
        public static bool Free(GameState s, GameConfig cfg, int gx, int gz) =>
            InGrid(cfg, gx, gz) && !Blocked(cfg, gx, gz) && At(s, gx, gz) == null;

        // ---------- Bauen und Abriss ----------

        public static Feedback Place(GameState s, GameConfig cfg, BuildMachineType t, int gx, int gz)
        {
            if (!Unlocked(s, t)) return Feedback.Error("ui.toasts.buildLocked", ("name", t.displayName));
            if (!Free(s, cfg, gx, gz)) return Feedback.Error("ui.toasts.buildCellTaken");
            float price = Price(s, cfg, t);
            if (!s.TrySpend(price)) return Feedback.Error("ui.toasts.buildNoMoney", ("name", t.displayName));
            s.machines.Add(new PlacedMachine { typeId = t.id, gx = gx, gz = gz, paid = price });
            s.machinesBuilt++;
            return Feedback.Ok;
        }

        /// <summary>Abriss: sellRefund des bezahlten Preises zurück. Liefert die Maschine oder null.</summary>
        public static PlacedMachine Sell(GameState s, GameConfig cfg, int gx, int gz)
        {
            var m = At(s, gx, gz);
            if (m == null) return null;
            s.machines.Remove(m);
            s.AddMoney(Mathf.Round(m.paid * cfg.buildMachines.sellRefund));
            return m;
        }

        // ---------- Wirkung ----------

        /// <summary>Melkpumpen: zusätzliche Milch in t/s bei stehender Kuh (Stromausfall: halbe Leistung).</summary>
        public static float PumpRate(GameState s, GameConfig cfg, bool cowOk)
        {
            if (!cowOk) return 0f;
            float r = 0f;
            foreach (var m in s.machines)
                if (m.typeId == Pump) r += Type(cfg, Pump).milkPerSec;
            return r * (s.blackoutTimer > 0f ? cfg.risks.blackoutProductionFactor : 1f);
        }

        /// <summary>Legebatterien: zusätzliche Eier in t/s bei stehendem Huhn (Stromausfall: halbe Leistung).</summary>
        public static float LayerRate(GameState s, GameConfig cfg, bool henOk)
        {
            if (!henOk) return 0f;
            float r = 0f;
            foreach (var m in s.machines)
                if (m.typeId == Layer) r += Type(cfg, Layer).eggsPerSec;
            return r * (s.blackoutTimer > 0f ? cfg.risks.blackoutProductionFactor : 1f);
        }

        /// <summary>Kühlaggregate: Hitzeabbau pro s (alle zusammen).</summary>
        public static float CoolRate(GameState s, GameConfig cfg)
        {
            float r = 0f;
            foreach (var m in s.machines)
                if (m.typeId == Cool) r += Type(cfg, Cool).heatPerSec;
            return r;
        }

        /// <summary>Zeitgeber einer Maschine weiterzählen; true, wenn das Intervall erreicht ist (Zeitgeber bleibt stehen).</summary>
        public static bool Due(PlacedMachine m, BuildMachineType t, float dt)
        {
            m.timer += dt;
            return m.timer >= t.intervalSec;
        }

        /// <summary>Soll der Notfall-Automat eingreifen (instabil oder Krise Säure/Überhitzung, Gegenmittel da)?</summary>
        public static bool DocNeeded(GameState s, AnimalState state, CrisisKind crisis) =>
            s.antidote > 0 && (state == AnimalState.Unstable || (state == AnimalState.Ok && (crisis == CrisisKind.Acid || crisis == CrisisKind.Overheat)));

        /// <summary>Index des zuletzt gesprühten Serums im Regal (gleiche Dosis bzw. gleiches Ultimativ) oder −1.</summary>
        public static int FindLastSprayed(GameState s)
        {
            var last = s.lastSprayed;
            if (last == null) return -1;
            return s.serums.FindIndex(x => x.ultimateId == last.ultimateId && x.dose.Key == last.dose.Key);
        }
    }
}
