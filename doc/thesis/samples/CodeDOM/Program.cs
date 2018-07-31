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

            foreach (var entityKind in EntityKinds.ToGenerate)
            {
                var entityType = new CodeTypeDeclaration(entityKind.Name)
                {
                    BaseTypes =
                    {
                        new CodeTypeReference(typeof(IComparable<>))
                        {
                            TypeArguments = { new CodeTypeReference(entityKind.Name) }
                        }
                    }
                };

                var ctor = new CodeConstructor();
                entityType.Members.Add(ctor);

                foreach (var property in entityKind.Properties)
                {
                    var propertyType = new CodeTypeReference(property.Type);

                    entityType.Members.Add(new CodeMemberField { Name = property.LowercaseName, Type = propertyType });

                    var fieldReference = new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), property.LowercaseName);

                    entityType.Members.Add(
                        new CodeMemberProperty
                        {
                            Attributes = Public | Final,
                            Name = property.Name,
                            Type = propertyType,
                            GetStatements =
                            {
                                new CodeMethodReturnStatement(fieldReference)
                            }
                        });
                    ctor.Parameters.Add(new CodeParameterDeclarationExpression(property.Type, property.LowercaseName));

                    ctor.Statements.Add(
                        new CodeAssignStatement(
                            fieldReference, new CodeArgumentReferenceExpression(property.LowercaseName)));
                }

                string otherName = "other";
                string compareToName = "CompareTo";
                var compareToMethod = new CodeMemberMethod
                {
                    Attributes = Public | Final,
                    ReturnType = new CodeTypeReference(typeof(int)),
                    Name = compareToName,
                    Parameters = { new CodeParameterDeclarationExpression(entityKind.Name, otherName) }
                };

                var otherReference = new CodeArgumentReferenceExpression(otherName);

                compareToMethod.Statements.Add(
                    new CodeConditionStatement(
                        new CodeBinaryOperatorExpression(
                            otherReference, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null)),
                        new CodeMethodReturnStatement(new CodePrimitiveExpression(1))));

                var resultName = "result";
                compareToMethod.Statements.Add(new CodeVariableDeclarationStatement(typeof(int), resultName));

                var resultReference = new CodeVariableReferenceExpression(resultName);

                foreach (var property in entityKind.Properties)
                {
                    compareToMethod.Statements.Add(
                        new CodeAssignStatement(
                            resultReference,
                            new CodeMethodInvokeExpression(
                                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), property.Name),
                                compareToName, new CodePropertyReferenceExpression(otherReference, property.Name))));

                    compareToMethod.Statements.Add(
                        new CodeConditionStatement(
                            new CodeBinaryOperatorExpression(
                                resultReference, CodeBinaryOperatorType.IdentityInequality,
                                new CodePrimitiveExpression(0)), new CodeMethodReturnStatement(resultReference)));
                }

                compareToMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(0)));

                entityType.Members.Add(compareToMethod);

                ns.Types.Add(entityType);
            }

            var compileUnit = new CodeCompileUnit { Namespaces = { ns } };

            var stringWriter = new StringWriter();

            new CSharpCodeProvider().GenerateCodeFromCompileUnit(compileUnit, stringWriter, null);

            Console.WriteLine(stringWriter);
        }
    }
}
