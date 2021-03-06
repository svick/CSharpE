using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax;
using CSharpE.Transform;
using static CSharpE.Syntax.SyntaxFactory;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Extensions.Record
{
    public class RecordTransformation : Transformation
    {
        public override void Process(Project project, bool designTime)
        {
            Smart.ForEach(project.GetTypesWithAttribute<RecordAttribute>(), designTime, (isDesignTime, baseType) =>
            {
                if (!(baseType is TypeDefinition typeDefinition))
                    return;
                
                typeDefinition.BaseTypes.Add(NamedType(typeof(IEquatable<>), typeDefinition.GetReference()));

                var fieldsList = Smart.ForEach(typeDefinition.Fields, field => (field.Name, field.Type));

                typeDefinition.Fields.Clear();

                Smart.Segment(typeDefinition, fieldsList, isDesignTime, (fields, dt, type) =>
                {
                    string ParamName(string name) => name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);

                    var constructorBody = dt
                        ? new Statement[] { NotImplementedExpression }
                        : fields.Select(f => (Statement)Assignment(This().MemberAccess(f.Name), Identifier(ParamName(f.Name))));

                    type.AddConstructor(Public, fields.Select(f => Parameter(f.Type, ParamName(f.Name))), Block(constructorBody));

                    foreach (var field in fields)
                    {
                        type.AddAutoProperty(Public, field.Type, field.Name, getOnly: true);

                        var witherBody = dt
                            ? NotImplementedExpression
                            : New(
                                type.GetReference(),
                                fields.Select(f => f.Name == field.Name ? (Expression)Identifier(ParamName(f.Name)) : This().MemberAccess(f.Name)));

                        type.AddMethod(
                            Public, type.GetReference(), "With" + field.Name,
                            new[] { Parameter(field.Type, ParamName(field.Name)) }, witherBody);
                    }
                });

                if (isDesignTime)
                {
                    Smart.Segment(typeDefinition, type =>
                    {
                        type.AddMethod(Public, typeof(bool), nameof(IEquatable<object>.Equals),
                            new[] {Parameter(type.GetReference(), "other")}, NotImplementedExpression);

                        type.AddMethod(Public | Override, typeof(bool), nameof(object.Equals),
                            new[] {Parameter(typeof(object), "obj")}, NotImplementedExpression);

                        type.AddMethod(Public | Override, typeof(int), nameof(GetHashCode), null, NotImplementedExpression);
                    });
                }
                else
                {
                    var other = Identifier("other");

                    // EqualityComparer<T>.Default.Equals(this.F1, other.F1) && ...
                    var comparisons = fieldsList
                        .Select(
                            f => NamedType(typeof(EqualityComparer<>), f.Type)
                                .MemberAccess(nameof(EqualityComparer<object>.Default))
                                .Call(
                                    nameof(EqualityComparer<object>.Equals), This().MemberAccess(f.Name),
                                    other.MemberAccess(f.Name)))
                        .Aggregate<Expression>(LogicalAnd);

                    var equalsStatements = new Statement[]
                    {
                        If(
                            NamedType(typeof(object)).Call(nameof(ReferenceEquals), other, Null()), Return(False())),
                        If(
                            NamedType(typeof(object)).Call(nameof(ReferenceEquals), other, This()), Return(True())),
                        Return(comparisons)
                    };

                    typeDefinition.AddMethod(
                        Public, typeof(bool), nameof(IEquatable<object>.Equals),
                        new[] { Parameter(typeDefinition.GetReference(), "other") }, Block(equalsStatements));

                    typeDefinition.AddMethod(
                        Public | Override, typeof(bool), nameof(object.Equals),
                        new[] { Parameter(typeof(object), "obj") },
                        This().Call("Equals", As(Identifier("obj"), typeDefinition.GetReference())));

                    typeDefinition.AddMethod(
                        Public | Override, typeof(int), nameof(GetHashCode), null,
                        Tuple(fieldsList.Select(f => This().MemberAccess(f.Name))).Call(nameof(GetHashCode)));
                }
            });
        }
    }
}
