namespace PlanErfuellt
{
    /// <summary>Serum in der Kryokammer (GDD 6.4): erst einfrieren, dann Zeitfenster zum Herausnehmen.</summary>
    public class CryoJob
    {
        public Serum serum;
        public float freezeLeft;
        public float windowLeft;

        public bool Ready => freezeLeft <= 0f;
    }
}
