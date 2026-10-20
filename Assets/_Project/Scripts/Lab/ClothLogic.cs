namespace PlanErfuellt
{
    /// <summary>
    /// Laken als Verbrauchsartikel (auf Wunsch, war zu stark): Überwerfen verbraucht ein Laken aus dem Lager,
    /// beim Abnehmen ist es von den Mutagenen zerfressen. Ohne Laken im Lager geht nichts.
    /// </summary>
    public static class ClothLogic
    {
        public enum Result { Covered, Removed, NoCloth }

        public static Result Toggle(GameState s)
        {
            if (s.chamberCovered)
            {
                s.chamberCovered = false;
                return Result.Removed;
            }
            if (s.cloths <= 0) return Result.NoCloth;
            s.cloths--;
            s.chamberCovered = true;
            return Result.Covered;
        }
    }
}
