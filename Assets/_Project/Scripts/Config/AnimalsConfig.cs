using System;

namespace PlanErfuellt
{
    /// <summary>Kuh und Huhn.</summary>
    [Serializable]
    public class AnimalsConfig
    {
        public AnimalBaseConfig cow = new AnimalBaseConfig();
        public AnimalBaseConfig hen = new AnimalBaseConfig();
    }
}
