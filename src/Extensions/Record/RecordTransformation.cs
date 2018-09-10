using System;
using System.Collections.Generic;
using System.Linq;
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
                
                typeDefinition.BaseTypes.Add(TypeReference(typeof(IEquatable<>), typeDefinition.GetReference()));
                
                var fieldsList = typeDefinition.ForEachField(field =>
                {
                    // TODO: errors for initializers and attributes

                    return (field.Name, Type: field.Type.Clone());
                });

                typeDefinition.Fields.Clear();

                typeDefinition.LimitedSegment(fieldsList, isDesignTime, (fields, dt, type) =>
                {
                    string paramName(string name) => name.ToLowerInvariant();

                    var constructorBody = dt
                        ? new[] { NotImplementedStatement }
                        : fields.Select(f => (Statement)Assignment(This().MemberAccess(f.Name), Identifier(paramName(f.Name))));

                    type.AddConstructor(Public, fields.Select(f => Parameter(f.Type, paramName(f.Name))), constructorBody);

                    foreach (var field in fields)
                    {
                        type.AddAutoProperty(Public, field.Type, field.Name, getOnly: true);

                        var witherBody = dt
                            ? NotImplementedStatement
                            : Return(New(
                                type.GetReference(),
                                fields.Select(f => f.Name == field.Name ? (Expression)Identifier(paramName(f.Name)) : This().MemberAccess(f.Name))));

                        type.AddMethod(Public, type.GetReference(), $"With{field.Name}", new[] { Parameter(field.Type, paramName(field.Name)) }, witherBody);
                    }
                });

                if (isDesignTime)
                {
                    typeDefinition.LimitedSegment(type =>
                    {
                        type.AddMethod(Public, typeof(bool), nameof(IEquatable<object>.Equals),
                            new[] {Parameter(type.GetReference(), "other")}, NotImplementedStatement);

                        type.AddMethod(Public | Override, typeof(bool), nameof(object.Equals),
                            new[] {Parameter(typeof(object), "obj")}, NotImplementedStatement);

                        type.AddMethod(Public | Override, typeof(int), nameof(GetHashCode), null,
                            NotImplementedStatement);
                    });
                }
                else
                {
                    typeDefinition.LimitedSegment(fieldsList, (fields, type) =>
                    {
                        var other = Identifier("other");

                        // EqualityComparer<T>.Default.Equals(this.F1, other.F1) && ...
                        var comparisons = fields
                            .Select(f => TypeReference(typeof(EqualityComparer<>), f.Type)
                                .MemberAccess(nameof(EqualityComparer<object>.Default))
                                .Call(nameof(EqualityComparer<object>.Equals), This().MemberAccess(f.Name), other.MemberAccess(f.Name)))
                            .Aggregate<Expression>((l, r) => LogicalAnd(l, r));

                        var equalsStatements = new Statement[]
                        {
                            If(TypeReference(typeof(object)).Call(nameof(ReferenceEquals), other, Null),
                                Return(False)),
                            If(TypeReference(typeof(object)).Call(nameof(ReferenceEquals), other, This()),
                                Return(True)),
                            Return(comparisons)
                        };

                        type.AddMethod(Public, typeof(bool), nameof(IEquatable<object>.Equals),
                            new[] {Parameter(type.GetReference(), "other")}, equalsStatements);

                        type.AddMethod(Public | Override, typeof(bool), nameof(object.Equals),
                            new[] {Parameter(typeof(object), "obj")},
                            Return(This().Call("Equals", As(Identifier("obj"), type.GetReference()))));

                        type.AddMethod(Public | Override, typeof(int), nameof(GetHashCode), null,
                            Return(Tuple(fields.Select(f => This().MemberAccess(f.Name))).Call(nameof(GetHashCode))));
                    });
                }
            });
        }

        public override IEnumerable<LibraryReference> AdditionalReferences =>
            new[] { new AssemblyReference(typeof(RecordAttribute)) };
    }
}