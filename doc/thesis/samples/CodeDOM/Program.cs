using System;
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
                    "IComparable", new CodeTypeReference(entityKind.Name)));

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

                var compareToMethod = new CodeMemberMethod
                {
                    Attributes = Public | Final,
                    ReturnType = new CodeTypeReference(typeof(int)),
                    Name = "CompareTo",
                    Parameters =
                    {
                        new CodeParameterDeclarationExpression(
                            entityKind.Name, "other")
                    }
                };

                var otherReference = new CodeArgumentReferenceExpression("other");

                compareToMethod.Statements.Add(new CodeConditionStatement(
                    new CodeBinaryOperatorExpression(otherReference,
                        CodeBinaryOperatorType.IdentityEquality,
                        new CodePrimitiveExpression(null)),
                    new CodeMethodReturnStatement(new CodePrimitiveExpression(1))));

                compareToMethod.Statements.Add(
                    new CodeVariableDeclarationStatement(typeof(int), "result"));

                var resultReference = new CodeVariableReferenceExpression("result");

                foreach (var property in entityKind.Properties)
                {
                    compareToMethod.Statements.Add(new CodeAssignStatement(
                        resultReference,
                        new CodeMethodInvokeExpression(
                            new CodePropertyReferenceExpression(
                                new CodeThisReferenceExpression(), property.Name),
                            "CompareTo",
                            new CodePropertyReferenceExpression(
                                otherReference, property.Name))));

                    compareToMethod.Statements.Add(new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(resultReference,
                            CodeBinaryOperatorType.IdentityInequality,
                            new CodePrimitiveExpression(0)),
                        new CodeMethodReturnStatement(resultReference)));
                }

                compareToMethod.Statements.Add(
                    new CodeMethodReturnStatement(new CodePrimitiveExpression(0)));

                entityType.Members.Add(compareToMethod);

                ns.Types.Add(entityType);
            }

            var compileUnit = new CodeCompileUnit { Namespaces = { ns } };

            var stringWriter = new StringWriter();
            new CSharpCodeProvider().GenerateCodeFromCompileUnit(
                compileUnit, stringWriter, null);
            Console.WriteLine(stringWriter);
        }
    }
}