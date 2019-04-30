using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class Attribute : SyntaxNode, ISyntaxWrapper<AttributeListSyntax>
    {
        private AttributeListSyntax syntax;

        internal Attribute(AttributeListSyntax syntax, MemberDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(AttributeListSyntax syntax)
        {
            if (syntax.Attributes.Count > 1)
                throw new ArgumentException(
                    "AttributeListSyntax with more than one attribute is not supported here.", nameof(syntax));

            this.syntax = syntax;
            Target = GetSyntaxTarget();
        }

        public Attribute(NamedTypeReference type, params AttributeArgument[] arguments)
            : this(type, arguments.AsEnumerable()) { }

        public Attribute(NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
            : this(AttributeTarget.None, type, arguments) { }

        public Attribute(AttributeTarget target, NamedTypeReference type, params AttributeArgument[] arguments)
            : this(target, type, arguments.AsEnumerable()) { }

        public Attribute(AttributeTarget target, NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
        {
            Target = target;
            Type = type;
            this.arguments = new SeparatedSyntaxList<AttributeArgument, AttributeArgumentSyntax>(arguments, this);
        }

        private static readonly BiDirectionalDictionary<string, AttributeTarget> AttributeTargetMap =
            new BiDirectionalDictionary<string, AttributeTarget>
            {
                { "assembly", AttributeTarget.Assembly },
                { "module", AttributeTarget.Module },
                { "type", AttributeTarget.Type },
                { "return", AttributeTarget.Return },
                { "method", AttributeTarget.Method },
                { "field", AttributeTarget.Field },
                { "event", AttributeTarget.Event },
                { "param", AttributeTarget.Parameter },
                { "property", AttributeTarget.Property },
                // TODO: what is this?
                { "typevar", AttributeTarget.TypeParameter }
            };

        private AttributeTarget GetSyntaxTarget()
        {
            var text = syntax.Target?.Identifier.ValueText;

            if (text == null)
                return AttributeTarget.None;

            return AttributeTargetMap[text];
        }

        public AttributeTarget Target { get; set; }

        private NamedTypeReference type;
        public NamedTypeReference Type
        {
            get
            {
                if (type == null)
                    type = new NamedTypeReference(syntax.Attributes.Single().Name, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private SeparatedSyntaxList<AttributeArgument, AttributeArgumentSyntax> arguments;
        public IList<AttributeArgument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<AttributeArgument, AttributeArgumentSyntax>(
                        syntax.Attributes.Single().ArgumentList?.Arguments ?? default, this);

                return arguments;
            }
            set => SetList(
                ref arguments, new SeparatedSyntaxList<AttributeArgument, AttributeArgumentSyntax>(value, this));
        }

        AttributeListSyntax ISyntaxWrapper<AttributeListSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = (NameSyntax)type?.GetWrapped(ref thisChanged) ?? syntax.Attributes.Single().Name;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ??
                               syntax.Attributes.Single().ArgumentList?.Arguments ??
                               default;

            if (syntax == null || thisChanged == true || Target != GetSyntaxTarget())
            {
                var targetSpecifier = Target == AttributeTarget.None
                    ? null
                    : RoslynSyntaxFactory.AttributeTargetSpecifier(
                        RoslynSyntaxFactory.Identifier(AttributeTargetMap[Target]));

                var argumentList = newArguments.Any() ? RoslynSyntaxFactory.AttributeArgumentList(newArguments) : null;

                var attributeSyntax = RoslynSyntaxFactory.Attribute(newType, argumentList);

                syntax = RoslynSyntaxFactory.AttributeList(
                    targetSpecifier, RoslynSyntaxFactory.SingletonSeparatedList(attributeSyntax));

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AttributeListSyntax)newSyntax);
            Set(ref type, null);
            SetList(ref arguments, null);
        }

        private protected override SyntaxNode CloneImpl() => new Attribute(Target, Type, Arguments);

        public override IEnumerable<SyntaxNode> GetChildren() => new SyntaxNode[] { Type }.Concat(Arguments);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var argument in Arguments)
            {
                argument.Expression = Expression.ReplaceExpressions(argument.Expression, filter, projection);
            }
        }
    }

    public enum AttributeTarget
    {
        None,
        Assembly,
        Module,
        Type,
        Method,
        Field,
        Property,
        Event,
        Parameter,
        Return,
        TypeParameter
    }
}