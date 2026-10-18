namespace PlanErfuellt
{
    /// <summary>Tagesquote (GDD 7.1): Soll, Ist, Restzeit, Prämie, erfüllt, Übererfüllung je Ware.</summary>
    public class Quota
    {
        public GoodsValues target = new GoodsValues();
        public readonly GoodsValues prod = new GoodsValues();
        public float left;
        public float duration;
        public float prize;
        public bool met;
        /// <summary>Übererfüllung schon gemeldet (Milch, Eier, Fleisch).</summary>
        public readonly bool[] over = new bool[3];

        public bool Reached => prod.m >= target.m && prod.e >= target.e && prod.f >= target.f;

        public static int Index(char good) => good == 'm' ? 0 : good == 'e' ? 1 : 2;

        public static float Get(GoodsValues g, char good) => good == 'm' ? g.m : good == 'e' ? g.e : g.f;

        public static void Add(GoodsValues g, char good, float v)
        {
            if (good == 'm') g.m += v;
            else if (good == 'e') g.e += v;
            else g.f += v;
        }
    }
}
