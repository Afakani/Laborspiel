using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PlanErfuellt.EditorTools
{
    /// <summary>
    /// Newtonsoft-Resolver, der nur öffentliche Felder der Klasse verwendet. Verhindert Konflikte mit
    /// Unity-Eigenschaften wie Object.name und hideFlags beim Befüllen von ScriptableObjects.
    /// </summary>
    public class FieldsOnlyContractResolver : DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(System.Type objectType) =>
            objectType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Cast<MemberInfo>()
                .ToList();

        protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);
            foreach (var p in props)
            {
                p.Readable = true;
                p.Writable = true;
            }
            return props;
        }
    }
}
