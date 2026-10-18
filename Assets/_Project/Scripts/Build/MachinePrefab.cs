using System;
using UnityEngine;

namespace PlanErfuellt
{
    /// <summary>Modell und Rohrmaterial eines Maschinentyps für den Baumodus.</summary>
    [Serializable]
    public class MachinePrefab
    {
        public string id;
        public GameObject prefab;
        public Material pipeMaterial;
    }
}
