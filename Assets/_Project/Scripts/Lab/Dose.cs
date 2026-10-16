using System;

namespace PlanErfuellt
{
    /// <summary>Dosis A (Wachstum), B (Laktation), C (Stabilität).</summary>
    [Serializable]
    public struct Dose : IEquatable<Dose>
    {
        public int A;
        public int B;
        public int C;

        public Dose(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public static readonly Dose Zero = new Dose(0, 0, 0);

        public int Total => A + B + C;
        public int AB => A + B;

        /// <summary>Schlüssel "A-B-C" wie in der JSON (comboNames).</summary>
        public string Key => $"{A}-{B}-{C}";

        public int this[int i] => i == 0 ? A : i == 1 ? B : C;

        public static Dose FromArray(int[] a) =>
            a == null || a.Length < 3 ? Zero : new Dose(a[0], a[1], a[2]);

        public static Dose operator +(Dose x, Dose y) => new Dose(x.A + y.A, x.B + y.B, x.C + y.C);
        public static bool operator ==(Dose x, Dose y) => x.Equals(y);
        public static bool operator !=(Dose x, Dose y) => !x.Equals(y);

        public bool Equals(Dose o) => A == o.A && B == o.B && C == o.C;
        public override bool Equals(object obj) => obj is Dose d && Equals(d);
        public override int GetHashCode() => (A * 397 ^ B) * 397 ^ C;
        public override string ToString() => Key;
    }
}
