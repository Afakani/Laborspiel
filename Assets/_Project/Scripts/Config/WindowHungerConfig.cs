using System;

namespace PlanErfuellt
{
    /// <summary>Fenster zeigt Warteschlange, wenn Volk oder Nahrung darunter.</summary>
    [Serializable]
    public class WindowHungerConfig
    {
        public float popBelow;
        public float foodBelow;
    }
}
