namespace PlanErfuellt
{
    /// <summary>Serum in der Zentrifuge (GDD 6.3): Drehzahl 0..1; nach dem Bremsen läuft der Rotor aus.</summary>
    public class CentrifugeJob
    {
        public Serum serum;
        public float rpm;
        /// <summary>true = dreht hoch (Bremsen möglich), false = läuft aus.</summary>
        public bool spinning = true;
    }
}
