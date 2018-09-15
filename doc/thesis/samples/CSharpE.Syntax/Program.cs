using System;
using System.IO;
using System.Linq;
using CSharpE.Samples.Core;
using CSharpE.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Samples.CSharpESyntax
{
    static class Program
    {
        static void Main()
        {
            var project = new Project(new SourceFile(
                "Entities.cs", EntityKinds.ToGenerateFromSource));

            foreach (var classDefinition in project.GetClasses())
            {
                classDefinition.BaseTypes.Add(
                    TypeReference(typeof(IEquatable<>), classDefinition));

                foreach (var field in classDefinition.Fields)
                {
                    classDefinition.AddAutoProperty(Public, field.Type, field.Name);
                }

                classDefinition.Fields.Clear();
            }

            File.WriteAllText("Entities.cs", project.SourceFiles.Single().GetText());
        }
    }
}
