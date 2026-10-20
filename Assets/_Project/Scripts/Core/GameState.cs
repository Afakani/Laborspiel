using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Der gesamte veränderliche Spielzustand als reine C#-Klasse. Gehalten vom GameManager.
    /// Änderungen an Geld und Lager laufen über Methoden, die Events auslösen.
    /// </summary>
    public class GameState
    {
        // ---------- Kasse und Lager ----------
        // [JsonProperty]: privater Setter, damit der Spielstand (SaveGame) die Kasse wiederherstellen kann
        [JsonProperty] public float Money { get; private set; }
        /// <summary>Grundstoffe im Lager: Index 0 = A, 1 = B, 2 = C.</summary>
        public readonly int[] substances = new int[3];
        public int antidote;
        public int vodka;
        public int caviar;
        /// <summary>Laken im Lager (Verbrauchsartikel: jedes Überwerfen verbraucht eines).</summary>
        public int cloths;
        /// <summary>Bestellung für den nächsten Morgen (schon bezahlt), Reihenfolge wie OrderLogic.Items; null = keine.</summary>
        public int[] pendingOrder;
        /// <summary>Zuletzt gelieferte eigene Bestellung („wie gestern“), sonst null.</summary>
        public int[] lastOrder;
        /// <summary>Serumstärke aus der Kantine für diese Schicht (1 = kein Essen).</summary>
        public float mealBoost = 1f;
        /// <summary>Schutzkittel aus der Umkleide für diese Schicht angezogen?</summary>
        public bool coatOn;

        /// <summary>Seren im Regal (max. mixing.rackSize).</summary>
        public readonly List<Serum> serums = new List<Serum>();

        // ---------- Fortschritt ----------
        /// <summary>Gespielte Schichten (Tag 1 = erste Schicht).</summary>
        public int dayNum;
        /// <summary>Index des aktuellen Plans (0 = Plan 1).</summary>
        public int planIdx;
        public int failedDays;
        /// <summary>Ergebnis des letzten Tages: 1 Plan erfüllt, −1 verfehlt, 0 noch keins (Stimmung in der Sendung).</summary>
        public int lastDayResult;
        public float playTime;

        // ---------- Mischstation ----------
        /// <summary>Aktuelle Reglerstellung.</summary>
        public Dose mix;
        /// <summary>Restzeit des laufenden Mischvorgangs (0 = frei).</summary>
        public float mixTimer;
        /// <summary>Dosis, die gerade gemischt wird.</summary>
        public Dose? mixPending;
        /// <summary>Serum in der Zentrifuge bzw. Kryokammer (GDD 6.3, 6.4), sonst null.</summary>
        public CentrifugeJob centrifuge;
        public CryoJob cryo;
        /// <summary>Restzeit der Ultimativ-Zwischensequenz (GDD 10).</summary>
        public float cinematicLeft;
        /// <summary>Am Pult gewähltes Serum im Regal.</summary>
        public int selectedSerum;
        /// <summary>Zuletzt gesprühtes Serum (für den Auto-Sprüher, GDD 6.10).</summary>
        public Serum lastSprayed;

        // ---------- Labor ----------
        /// <summary>Kammer nach Explosion verseucht (GDD 5.4).</summary>
        public bool contaminated;
        /// <summary>Restzeit der Kammerreinigung.</summary>
        public float cleanTimer;
        /// <summary>Restzeit des Stromausfalls (GDD 5.2).</summary>
        public float blackoutTimer;
        /// <summary>Laken liegt über der Kammer (GDD 6.8).</summary>
        public bool chamberCovered;
        /// <summary>Restzeit der Volksmusik vom Grammophon (GDD 6.5).</summary>
        public float musicLeft;
        /// <summary>Restzeit der Probe unter dem Mikroskop (GDD 6.2).</summary>
        public float probeLeft;
        /// <summary>Mischungen, die schon unter dem Mikroskop waren (Schlüssel "A-B-C").</summary>
        public readonly HashSet<string> probedRecipes = new HashSet<string>();
        /// <summary>Restflugzeit der Rohrpost-Kapsel.</summary>
        public float rohrpostLeft;
        /// <summary>Gebaute Maschinen (GDD 6.10).</summary>
        public readonly List<PlacedMachine> machines = new List<PlacedMachine>();
        /// <summary>Spielzeit der letzten Meldung „Auto-Sprüher: kein Serum“.</summary>
        public float autoSprayWarnedAt = float.NegativeInfinity;
        /// <summary>Förderband ist gerissen (GDD 5.2).</summary>
        public bool beltBroken;

        // ---------- Tagesablauf (GDD 7) ----------
        /// <summary>Quote des laufenden Tages, null zwischen den Schichten.</summary>
        public Quota quota;
        /// <summary>Erfüllte und verfehlte Tage.</summary>
        public int quotasMet;
        public int quotasFailed;
        public Wish wish;
        /// <summary>Zeit bis zum nächsten Sonderwunsch.</summary>
        public float wishTimer;
        public Contract contract;
        /// <summary>Schritt der Einführung an Tag 1 (−1 = keine Einführung mehr).</summary>
        public int tutorialStep;
        /// <summary>Zeit im aktuellen Einführungsschritt (für Lese-Schritte).</summary>
        public float tutorialTime;

        // ---------- Druck von oben ----------
        public float anger;
        public float authority;

        // ---------- Kontrolle (GDD 9) ----------
        /// <summary>Laufende Kontrolle oder null.</summary>
        public Inspection inspection;
        /// <summary>Spielzeit der letzten bestandenen Kontrolle.</summary>
        public float inspectionDoneAt = float.NegativeInfinity;
        /// <summary>Spielzeit der nächsten planmäßigen Kontrolle.</summary>
        public float nextInspectionAt = float.PositiveInfinity;
        /// <summary>Spielzeit, ab der der Oberkommissar kommt (0 = nicht angekündigt).</summary>
        public float bossAt;
        public bool bossDone;
        /// <summary>Serum-Regal an der Mischstation abgedeckt.</summary>
        public bool rackHidden;
        /// <summary>Summe der Fleisch-Tagesziele abgeschlossener Tage (für „Fleisch … % über Plan“ im Staats-TV).</summary>
        public float meatTargetTotal;
        /// <summary>Zeile zum letzten Tagesergebnis für den Lauftext.</summary>
        public string lastPlanLine;
        /// <summary>Ultimativ für die nächste Sondersendung oder null.</summary>
        public string specialNewsUltimate;
        /// <summary>Taten für den Lauftext: Textpfad unter newsFacts oder fertiger Text.</summary>
        public readonly List<string> newsFacts = new List<string>();

        // ---------- Infektion des Spielers (GDD 8) ----------
        public bool infected;
        /// <summary>Frist: bis Ende dieses Tages.</summary>
        public int infectedUntilDay;
        /// <summary>Infektionswert 0..1 (Schleier, Anzeige).</summary>
        public float infectionLevel;
        /// <summary>Gerade wirkendes Symptom (nur zur Laufzeit, für Anzeige und Effekte).</summary>
        public string infectionSymptom;

        // ---------- Entdeckungen und Statistik ----------
        /// <summary>Am Tier getestete Kombinationen (Schlüssel "A-B-C" bzw. "U:id" für Ultimative).</summary>
        public readonly HashSet<string> discoveredRecipes = new HashSet<string>();
        /// <summary>Gefundene Kuriositäten (Id).</summary>
        public readonly HashSet<string> foundFunRecipes = new HashSet<string>();
        public bool ultimateSeen;
        /// <summary>Gesamt erzeugte Waren in t.</summary>
        public readonly GoodsValues totals = new GoodsValues();
        /// <summary>Rekorde nach Schlüssel (rate, meat, day_m, day_e, day_f).</summary>
        public readonly Dictionary<string, float> records = new Dictionary<string, float>();
        public int experiments;
        public int explosions;
        public int totalDoses;
        public int machinesBuilt;
        public float earned;
        /// <summary>Summe Tonnen × Qualität und gelieferte Tonnen (für die Durchschnittsqualität, GDD 7.7).</summary>
        public float qualityTons;
        public float deliveredTons;
        /// <summary>Gesamtertrag aller Quellen in t/s (HUD).</summary>
        public float currentRate;

        /// <summary>Neues Spiel mit den Startwerten aus der Config.</summary>
        public static GameState Create(GameConfig cfg)
        {
            var s = new GameState();
            s.Money = cfg.start.money;
            s.substances[0] = cfg.start.inventory.A;
            s.substances[1] = cfg.start.inventory.B;
            s.substances[2] = cfg.start.inventory.C;
            s.antidote = cfg.start.antidote;
            s.vodka = cfg.start.vodka;
            s.caviar = cfg.start.caviar;
            s.cloths = cfg.start.cloths;
            return s;
        }

        /// <summary>Geld hinzufügen (negativ = abziehen).</summary>
        public void AddMoney(float amount)
        {
            if (Mathf.Approximately(amount, 0f)) return;
            Money += amount;
            EventBus.RaiseMoneyChanged(Money);
        }

        /// <summary>Zieht Geld ab, wenn genug da ist.</summary>
        public bool TrySpend(float amount)
        {
            if (Money < amount) return false;
            AddMoney(-amount);
            return true;
        }

        public void SetMoney(float value)
        {
            Money = value;
            EventBus.RaiseMoneyChanged(Money);
        }

        /// <summary>Grundstoff ändern (Index 0..2), nie unter 0.</summary>
        public void AddSubstance(int index, int amount)
        {
            substances[index] = Mathf.Max(0, substances[index] + amount);
            EventBus.RaiseInventoryChanged();
        }

        /// <summary>Direktor-Wut ändern, begrenzt auf 0..max.</summary>
        public void AddAnger(float amount, GameConfig cfg) => anger = Mathf.Clamp(anger + amount, 0f, cfg.anger.max);

        /// <summary>Parteikontrolle ändern, begrenzt auf 0..max.</summary>
        public void AddAuthority(float amount, GameConfig cfg) => authority = Mathf.Clamp(authority + amount, 0f, cfg.authority.max);

        /// <summary>Lagerartikel (Gegenmittel, Wodka, Kaviar) wurden geändert.</summary>
        public void NotifyInventoryChanged() => EventBus.RaiseInventoryChanged();
    }
}
