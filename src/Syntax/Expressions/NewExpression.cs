using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: object initializer, anonymous types
    public sealed class NewExpression : Expression
    {
        private ObjectCreationExpressionSyntax syntax;

        internal NewExpression(ObjectCreationExpressionSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Parent = parent;
        }

        public NewExpression(TypeReference type, IEnumerable<Argument> arguments)
        {
            this.Type = type;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
        }

        public NewExpression(TypeReference type, IEnumerable<Expression> arguments)
            : this(type, arguments.Select(a => (Argument)a)) { }

        public NewExpression(TypeReference type, params Argument[] arguments)
            : this(type, (IEnumerable<Argument>)arguments) { }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = new NamedTypeReference(syntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(syntax.ArgumentList.Arguments, this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        internal override ExpressionSyntax GetWrapped(ref bool changed)
        {
            changed |= GetAndResetSyntaxSet();

            bool thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList.Arguments;

            if (syntax == null || thisChanged)
            {
                syntax = CSharpSyntaxFactory.ObjectCreationExpression(
                    newType, CSharpSyntaxFactory.ArgumentList(newArguments), null);

                changed = true;
            }

            return syntax;
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ObjectCreationExpressionSyntax)newSyntax;

            Set(ref type, null);
            arguments = null;
        }

        internal override SyntaxNode Clone() => new NewExpression(Type, Arguments);

        internal override SyntaxNode Parent { get; set; }
    }
}