namespace PlanErfuellt.EditorTools
{
    /// <summary>Ein Körperteil der Haut: Abstandsfeld (Modellraum) und der Knochen, der es bewegt.</summary>
    public struct SkinPart
    {
        public SdfMesher.Field field;
        public int bone;

        public SkinPart(SdfMesher.Field f, int b)
        {
            field = f;
            bone = b;
        }
    }
}
