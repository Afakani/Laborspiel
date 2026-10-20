using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>
    /// Spielstand als JSON-Datei. Gespeichert wird automatisch zu Beginn jeder Schicht (Bestellterminal), dort sind
    /// Kammer, Maschinenläufe und Kontrollen ruhig; „Weiterspielen“ im Hauptmenü lädt diesen Stand.
    /// </summary>
    public static class SaveGame
    {
        public const int Version = 1;
        const string FileName = "spielstand.json";

        /// <summary>Kopf und Inhalt der Datei.</summary>
        public class Data
        {
            public int version;
            public string savedAt;
            public GameState state;
        }

        static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            // ±Unendlich (z. B. inspectionDoneAt) als Text, Schleifen ignorieren, fehlende Felder sind kein Fehler
            FloatFormatHandling = FloatFormatHandling.String,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            ContractResolver = new DataOnlyResolver()
        };

        /// <summary>Nur echte Daten: berechnete Eigenschaften ohne Setter (z. B. Contract.Good) weglassen, readonly-Felder laden.</summary>
        class DataOnlyResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var p = base.CreateProperty(member, memberSerialization);
                if (member is PropertyInfo pi && !pi.CanWrite) p.ShouldSerialize = _ => false;
                // readonly-Felder (z. B. Lager-Array, Rezeptlisten) beim Laden trotzdem setzen
                if (member is FieldInfo fi && fi.IsInitOnly) p.Writable = true;
                return p;
            }
        }

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);

        public static bool Exists => File.Exists(Path);

        public static string ToJson(GameState s) =>
            JsonConvert.SerializeObject(new Data { version = Version, savedAt = DateTime.Now.ToString("s"), state = s }, Formatting.Indented, Settings);

        /// <summary>Spielstand aus JSON; null, wenn die Datei kaputt oder zu alt ist.</summary>
        public static Data FromJson(string json)
        {
            try
            {
                var d = JsonConvert.DeserializeObject<Data>(json, Settings);
                return d != null && d.version == Version && d.state != null ? d : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand nicht lesbar: " + e.Message);
                return null;
            }
        }

        public static void Save(GameState s)
        {
            try
            {
                File.WriteAllText(Path, ToJson(s));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand nicht gespeichert: " + e.Message);
            }
        }

        /// <summary>Gespeicherter Stand oder null.</summary>
        public static Data Load()
        {
            if (!Exists) return null;
            try
            {
                return FromJson(File.ReadAllText(Path));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Spielstand nicht lesbar: " + e.Message);
                return null;
            }
        }
    }
}
