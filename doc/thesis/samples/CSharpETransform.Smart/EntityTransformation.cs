using System;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Samples.CSharpETransformSmart
{
    public class EntityTransformation : SimpleTransformation
    {
        protected override void Process(Project project)
        {
            Smart.ForEach(project.GetClasses(), classDefinition =>
            {
                classDefinition.BaseTypes.Add(
                    TypeReference(typeof(IEquatable<>), classDefinition));

                foreach (var field in classDefinition.Fields)
                {
                    classDefinition.AddAutoProperty(
                        Public, field.Type, field.Name);
                }

                classDefinition.Fields.Clear();
            });
        }
    }
}
