using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Samples.Core
{
    public sealed class EntityKind
    {
        public EntityKind(string name, IEnumerable<Property> properties)
        {
            Name = name;
            Properties = properties.ToList();
        }

        public EntityKind(string name, IEnumerable<(string type, string name)> properties)
            : this(name, properties.Select(p => new Property(p.type, p.name))) { }

        public string Name { get; }
        public IReadOnlyList<Property> Properties { get; }
    }

    public sealed class Property
    {
        public Property(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public string Type { get; }
        public string Name { get; }
        public string LowercaseName => Name.ToLowerInvariant();
    }

    public static class EntityKinds
    {
        public static IEnumerable<EntityKind> ToGenerate { get; } =
            new[] { new EntityKind("Person", new[] { ("String", "Name"), ("Int32", "Age") }) };

        public static string ToGenerateFromSource { get; } =
            @"class Person
{
    string Name;
    int Age;
}";
    }
}
