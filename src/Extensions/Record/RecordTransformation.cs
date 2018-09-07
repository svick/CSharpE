using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform;
using CSharpE.Transform.Smart;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Record
{
    public class RecordTransformation : Transformation
    {
        public override void Process(Syntax.Project project, bool designTime)
        {
            project.ForEachTypeWithAttribute<RecordAttribute, bool>(designTime, (isDesignTime, baseType) =>
            {
                if (!(baseType is TypeDefinition typeDefinition))
                    return;
                
                typeDefinition.Segment(type =>
                {
                    type.BaseTypes.Add(TypeReference(typeof(IEquatable<>), type.GetReference()));
                });
                
                var fieldsList = typeDefinition.ForEachField(field =>
                {
                    // TODO: errors for initializers and attributes

                    return (field.Name, Type: field.Type.Clone());
                });

                typeDefinition.Segment(fieldsList, (fields, type) =>
                {
                    type.Fields.Clear();

                    foreach (var field in fields)
                    {
                        type.AddAutoProperty(Public, field.Type, field.Name);
                    }
                });

                if (isDesignTime)
                {
                    typeDefinition.Segment(type =>
                    {
                        type.AddMethod(Public, typeof(bool), nameof(IEquatable<object>.Equals),
                            new[] {Parameter(type.GetReference(), "other")}, NotImplementedStatement);

                        type.AddMethod(Public | Override, typeof(bool), nameof(object.Equals),
                            new[] {Parameter(typeof(object), "obj")}, NotImplementedStatement);

                        type.AddMethod(Public | Override, typeof(int), nameof(object.GetHashCode), null,
                            NotImplementedStatement);
                    });
                }
            });
        }

        public override IEnumerable<LibraryReference> AdditionalReferences =>
            new[] { new AssemblyReference(typeof(RecordAttribute)) };
    }
}