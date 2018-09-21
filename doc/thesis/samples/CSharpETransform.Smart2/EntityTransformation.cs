using System;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Samples.CSharpETransformSmart2
{
    public class EntityTransformation : SimpleTransformation
    {
        protected override void Process(Project project)
        {
            Smart.ForEach(project.GetClasses(), classDefinition =>
            {
                classDefinition.BaseTypes.Add(
                    TypeReference(typeof(IEquatable<>), classDefinition));

                var fieldsList = Smart.ForEach(classDefinition.Fields,
                    field => (field.Type, field.Name));

                foreach (var field in fieldsList)
                {
                    Smart.Segment(classDefinition, field, (f, classDef) =>
                        classDef.AddAutoProperty(Public, f.Type, f.Name));
                }

                classDefinition.Fields.Clear();
            });
        }
    }
}
