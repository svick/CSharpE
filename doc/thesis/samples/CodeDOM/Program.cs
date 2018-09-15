using System.CodeDom;
using System.IO;
using CSharpE.Samples.Core;
using Microsoft.CSharp;
using static System.CodeDom.MemberAttributes;

namespace CSharpE.Samples.CodeDOM
{
    static class Program
    {
        static void Main()
        {
            var ns = new CodeNamespace();
            ns.Imports.Add(new CodeNamespaceImport("System"));

            foreach (var entityKind in EntityKinds.ToGenerate)
            {
                var entityType = new CodeTypeDeclaration(entityKind.Name);
                entityType.BaseTypes.Add(new CodeTypeReference(
                    "IEquatable", new CodeTypeReference(entityKind.Name)));

                foreach (var property in entityKind.Properties)
                {
                    var propertyType = new CodeTypeReference(property.Type);

                    entityType.Members.Add(new CodeMemberField
                    {
                        Name = property.LowercaseName, Type = propertyType
                    });

                    var fieldReference = new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), property.LowercaseName);

                    entityType.Members.Add(new CodeMemberProperty
                    {
                        Attributes = Public | Final,
                        Name = property.Name,
                        Type = propertyType,
                        GetStatements =
                        {
                            new CodeMethodReturnStatement(fieldReference)
                        },
                        SetStatements =
                        {
                            new CodeAssignStatement(fieldReference,
                                new CodePropertySetValueReferenceExpression())
                        }
                    });
                }

                ns.Types.Add(entityType);
            }

            var compileUnit = new CodeCompileUnit { Namespaces = { ns } };

            using (var writer = new StreamWriter("Entities.cs"))
            {
                new CSharpCodeProvider().GenerateCodeFromCompileUnit(
                    compileUnit, writer, null);
            }
        }
    }
}