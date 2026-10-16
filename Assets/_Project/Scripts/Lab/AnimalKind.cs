using System.Runtime.Serialization;

namespace PlanErfuellt
{
    /// <summary>Tierart in der Kammer.</summary>
    public enum AnimalKind
    {
        [EnumMember(Value = "cow")] Cow,
        [EnumMember(Value = "hen")] Hen
    }
}
