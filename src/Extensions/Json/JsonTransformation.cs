using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Syntax.Internals;
using CSharpE.Transform;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Extensions.Json
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class JsonAttribute : System.Attribute
    {
        public JsonAttribute(string json) => Json = json;

        public string Json { get; }
    }

    public class JsonTransformation : Transformation
    {
        public override void Process(Project project, bool designTime)
        {
            // TODO: this should throw, because it's accessing this
            Smart.ForEach(project.GetClassesWithAttribute<JsonAttribute>(), type =>
            {
                var jsonStringLiteral = (StringLiteralExpression)type.GetAttribute<JsonAttribute>().Arguments.Single().Expression;

                var jsonObject = JsonConvert.DeserializeObject<JToken>(jsonStringLiteral.Value);

                var jsonBuilder = new JsonBuilder();

                var jsonObjectType = jsonBuilder.GetValueType(jsonObject);

                type.Members.AddRange(jsonBuilder.ObjectTypes);

                type.AddMethod(
                    Public | Static, jsonObjectType, "Parse", new[] { Parameter(typeof(string), "json") },
                    NotImplementedStatement);
            });
        }
    }

    class JsonBuilder
    {
        private int i;
        private readonly List<ClassDefinition> objectTypes = new List<ClassDefinition>();

        public IEnumerable<ClassDefinition> ObjectTypes => objectTypes;

        private static TypeReference MergeTypes(IEnumerable<TypeReference> elementTypes)
        {
            // for now, only supports the case where all elements have the same type

            TypeReference result = null;

            foreach (var type in elementTypes)
            {
                if (result == null)
                    result = type;
                else if (!type.Equals(result))
                    throw new InvalidOperationException($"JSON array contains incompatible types {result} and {type}.");
            }

            if (result == null)
                throw new InvalidOperationException("JSON array contains no elements, so its type could not be determined.");

            return result;
        }

        public TypeReference GetValueType(JToken token)
        {
            switch (token)
            {
                case JArray array:
                    var elementType = MergeTypes(array.Select(GetValueType));
                    return ArrayType(elementType);
                case JValue value:
                    return NamedType(value.Value.GetType());
                case JObject obj:
                    return GetObjectType(obj);
                default:
                    throw new InvalidOperationException($"JSON token type {token.GetType()} is not supported.");
            }
        }

        public NamedTypeReference GetObjectType(JObject obj)
        {
            var classDefinition = ClassDefinition(Public, "JsonObject" + i++);

            foreach (var (name, value) in obj)
            {
                classDefinition.AddAutoProperty(Public, GetValueType(value), name, true);
            }

            objectTypes.Add(classDefinition);

            return NamedType(null, classDefinition.Name);
        }
    }
}
