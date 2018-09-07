using System;
using System.Collections.Generic;
using CSharpE.Syntax;
using CSharpE.Transform;
using CSharpE.Transform.Smart;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Record
{
    public class RecordTransformation : ITransformation
    {
        public void Process(Syntax.Project project)
        {
            project.ForEachTypeWithAttribute<RecordAttribute>(baseType =>
            {
                if (!(baseType is TypeDefinition typeDefinition))
                    return;
                
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
            });
        }

        public IEnumerable<LibraryReference> AdditionalReferences =>
            new[] { new AssemblyReference(typeof(RecordAttribute)) };
    }
}