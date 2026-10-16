using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Alle Spieltexte aus Data/Json/Texte_de.json. Zugriff über Punkt-Pfade, z. B. "ui.toasts.planFailed".
    /// </summary>
    public class TextDatabase
    {
        static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

        readonly JObject root;
        readonly float[] angerTiers;
        readonly System.Random rng;

        public TextDatabase(string json, float[] angerTiers, int? seed = null)
        {
            root = JObject.Parse(json);
            this.angerTiers = angerTiers ?? new float[0];
            rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        }

        /// <summary>Gibt es einen Eintrag unter diesem Pfad?</summary>
        public bool Has(string path) => root.SelectToken(path) != null;

        /// <summary>Einzelner Text. Fehlt er, wird [pfad] zurückgegeben und gewarnt.</summary>
        public string Get(string path)
        {
            var token = root.SelectToken(path);
            if (token == null || token.Type != JTokenType.String)
            {
                Debug.LogWarning($"TextDatabase: Text fehlt oder ist keine Zeichenkette: {path}");
                return $"[{path}]";
            }
            return (string)token;
        }

        /// <summary>Text mit ersetzten Platzhaltern.</summary>
        public string Format(string path, params (string key, object value)[] values) => Fill(Get(path), values);

        /// <summary>Liste von Texten (leer, wenn nicht vorhanden).</summary>
        public IReadOnlyList<string> GetList(string path)
        {
            var list = new List<string>();
            if (root.SelectToken(path) is JArray arr)
                foreach (var t in arr)
                    if (t.Type == JTokenType.String) list.Add((string)t);
            return list;
        }

        /// <summary>Zufälliger Text aus einer Liste.</summary>
        public string Pick(string path) => PickFrom(GetList(path), path);

        /// <summary>Wut-Stufe 0 (freundlich), 1 (gereizt) oder 2 (cholerisch).</summary>
        public int AngerTier(float anger)
        {
            int tier = 0;
            foreach (var limit in angerTiers)
                if (anger >= limit) tier++;
            return tier;
        }

        /// <summary>
        /// Zufälliger Spruch passend zur Wut. Erwartet eine Liste von Listen (eine je Stufe);
        /// bei einer einfachen Liste wird daraus gewählt.
        /// </summary>
        public string PickByAnger(string path, float anger)
        {
            if (!(root.SelectToken(path) is JArray arr) || arr.Count == 0)
            {
                Debug.LogWarning($"TextDatabase: Liste fehlt: {path}");
                return $"[{path}]";
            }
            if (arr[0].Type != JTokenType.Array) return Pick(path);
            int tier = Mathf.Min(AngerTier(anger), arr.Count - 1);
            return Pick($"{path}[{tier}]");
        }

        string PickFrom(IReadOnlyList<string> list, string path)
        {
            if (list.Count == 0)
            {
                Debug.LogWarning($"TextDatabase: Liste fehlt oder ist leer: {path}");
                return $"[{path}]";
            }
            return list[rng.Next(list.Count)];
        }

        /// <summary>Ersetzt {schlüssel} durch Werte. Zahlen werden deutsch formatiert (Komma).</summary>
        public static string Fill(string template, params (string key, object value)[] values)
        {
            if (string.IsNullOrEmpty(template) || values == null || values.Length == 0) return template;
            var sb = new StringBuilder(template);
            foreach (var (key, value) in values)
                sb.Replace("{" + key + "}", Convert.ToString(value, German));
            return sb.ToString();
        }
    }
}
