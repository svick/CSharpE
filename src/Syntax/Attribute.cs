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
            : base(syntax)
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
            target = new Identifier(syntax.Target?.Identifier);
        }

        public Attribute(NamedTypeReference type, params AttributeArgument[] arguments)
            : this(type, arguments.AsEnumerable()) { }

        public Attribute(NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
            : this(null, type, arguments) { }

        public Attribute(string target, NamedTypeReference type, params AttributeArgument[] arguments)
            : this(target, type, arguments.AsEnumerable()) { }

        public Attribute(string target, NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
        {
            this.target = new Identifier(target, true);
            Type = type;
            Arguments = arguments?.ToList();
        }

        // TODO: Create a tiny type for AttributeTarget
        private Identifier target;
        public string Target
        {
            get => target.Text;
            set => target.Text = value;
        }

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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newTarget = target.GetWrapped(ref thisChanged);
            var newType = (NameSyntax)type?.GetWrapped(ref thisChanged) ?? syntax.Attributes.Single().Name;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ??
                               syntax.Attributes.Single().ArgumentList?.Arguments ??
                               default;

            if (syntax == null || thisChanged == true)
            {
                var targetSpecifier = newTarget == default
                    ? null
                    : RoslynSyntaxFactory.AttributeTargetSpecifier(newTarget);

                var argumentList = newArguments.Any() ? RoslynSyntaxFactory.AttributeArgumentList(newArguments) : null;

                var attributeSyntax = RoslynSyntaxFactory.Attribute(newType, argumentList);

                syntax = RoslynSyntaxFactory.AttributeList(
                    targetSpecifier, RoslynSyntaxFactory.SingletonSeparatedList(attributeSyntax));

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((AttributeListSyntax)newSyntax);
            Set(ref type, null);
            SetList(ref arguments, null);
        }

        private protected override SyntaxNode CloneImpl() => new Attribute(Target, Type, Arguments);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var argument in Arguments)
            {
                argument.Expression = Expression.ReplaceExpressions(argument.Expression, filter, projection);
            }
        }
    }
}