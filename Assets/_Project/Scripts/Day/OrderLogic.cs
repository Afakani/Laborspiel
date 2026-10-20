using System.Collections.Generic;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Bestellen für den nächsten Tag (auf Wunsch statt Bestellung vor der Schicht): am Terminal im Lager jederzeit,
    /// sofort bezahlt; geliefert wird zu Schichtbeginn. Ohne Bestellung kommt die Standardbestellung (soweit bezahlbar).
    /// Artikel: 0–2 Grundstoffe A/B/C, 3 Gegenmittel, 4 Wodka, 5 Kaviar, 6 Laken.
    /// </summary>
    public static class OrderLogic
    {
        public const int Items = 7;

        public static float Price(GameConfig cfg, int i)
        {
            var p = cfg.prices;
            return i switch { 0 => p.A, 1 => p.B, 2 => p.C, 3 => p.antidote, 4 => p.vodka, 5 => p.caviar, _ => p.cloth };
        }

        public static float Sum(GameConfig cfg, int[] amounts)
        {
            float sum = 0f;
            for (int i = 0; i < amounts.Length && i < Items; i++) sum += amounts[i] * Price(cfg, i);
            return sum;
        }

        public static int Stock(GameState s, int i) =>
            i < 3 ? s.substances[i] : i == 3 ? s.antidote : i == 4 ? s.vodka : i == 5 ? s.caviar : s.cloths;

        static void AddStock(GameState s, int i, int n)
        {
            if (i < 3) s.substances[i] += n;
            else if (i == 3) s.antidote += n;
            else if (i == 4) s.vodka += n;
            else if (i == 5) s.caviar += n;
            else s.cloths += n;
        }

        /// <summary>Bestellung bezahlen und für morgen vormerken (mehrere Bestellungen addieren sich). false = zu wenig Geld oder leer.</summary>
        public static bool Place(GameState s, GameConfig cfg, int[] amounts)
        {
            float sum = Sum(cfg, amounts);
            if (sum <= 0f || !s.TrySpend(sum)) return false;
            s.pendingOrder ??= new int[Items];
            for (int i = 0; i < Items && i < amounts.Length; i++) s.pendingOrder[i] += amounts[i];
            return true;
        }

        /// <summary>
        /// Lieferung zu Schichtbeginn: vorgemerkte Bestellung, sonst Standardbestellung (wird jetzt bezahlt, von hinten
        /// gekürzt, bis sie in die Kasse passt). Liefert die gelieferten Mengen und ob es die Standardbestellung war.
        /// </summary>
        public static (int[] amounts, bool standard) Deliver(GameState s, GameConfig cfg)
        {
            bool standard = s.pendingOrder == null;
            var amounts = new int[Items];
            if (standard)
            {
                var std = cfg.day.standardOrder;
                for (int i = 0; i < Items && i < std.Length; i++) amounts[i] = std[i];
                while (Sum(cfg, amounts) > s.Money)
                {
                    int last = System.Array.FindLastIndex(amounts, a => a > 0);
                    if (last < 0) break;
                    amounts[last]--;
                }
                s.TrySpend(Sum(cfg, amounts));
            }
            else
            {
                for (int i = 0; i < Items; i++) amounts[i] = s.pendingOrder[i];
                s.pendingOrder = null;
                s.lastOrder = (int[])amounts.Clone();
            }
            for (int i = 0; i < Items; i++) AddStock(s, i, amounts[i]);
            return (amounts, standard);
        }

        /// <summary>„2× Laktation B-08, 1× Laken“ für Meldungen.</summary>
        public static string List(int[] amounts, IReadOnlyList<string> names)
        {
            var parts = new List<string>();
            for (int i = 0; i < amounts.Length && i < names.Count; i++)
                if (amounts[i] > 0) parts.Add($"{amounts[i]}× {names[i]}");
            return string.Join(", ", parts);
        }

        /// <summary>Kosten fürs Klonen.</summary>
        public static float CloneCost(GameConfig cfg, AnimalKind kind) => kind == AnimalKind.Cow ? cfg.prices.cloneCow : cfg.prices.cloneHen;
    }
}
