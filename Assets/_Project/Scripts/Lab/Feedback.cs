namespace PlanErfuellt
{
    /// <summary>
    /// Ergebnis einer Spieleraktion: leer = erfolgreich, sonst Textpfad + Platzhalter für die Fehlermeldung
    /// (wie toastR im Prototyp).
    /// </summary>
    public readonly struct Feedback
    {
        public readonly string key;
        public readonly (string key, object value)[] args;

        Feedback(string key, (string, object)[] args)
        {
            this.key = key;
            this.args = args;
        }

        public static readonly Feedback Ok = default;
        public static Feedback Error(string key, params (string, object)[] args) => new Feedback(key, args);

        public bool IsOk => string.IsNullOrEmpty(key);

        /// <summary>Fehler als Warn-Kurzmeldung anzeigen; gibt IsOk zurück.</summary>
        public bool Report(GameManager gm)
        {
            if (!IsOk) gm.Toast(key, ToastKind.Warn, args ?? new (string, object)[0]);
            return IsOk;
        }
    }
}
