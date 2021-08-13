using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

#nullable enable

namespace CSharpE.Syntax
{
    public sealed class NewExpression : Expression
    {
        private BaseObjectCreationExpressionSyntax syntax = null!;

        internal NewExpression(BaseObjectCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public NewExpression(TypeReference? type, IEnumerable<Argument> arguments, Initializer? initializer = null)
        {
            this.Type = type;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
            this.Initializer = initializer;
        }

        public NewExpression(TypeReference? type, IEnumerable<Expression> arguments, Initializer? initializer = null)
            : this(type, arguments.Select(a => (Argument)a), initializer) { }

        public NewExpression(TypeReference? type, params Argument[] arguments)
            : this(type, (IEnumerable<Argument>)arguments) { }

        public NewExpression(IEnumerable<Expression> arguments, Initializer? initializer = null)
            : this(null, arguments.Select(a => (Argument)a), initializer) { }

        public NewExpression(params Argument[] arguments)
            : this(null, arguments) { }

        private bool typeSet;
        private TypeReference? type;
        public TypeReference? Type
        {
            get
            {
                if (!typeSet)
                {
                    var syntaxType = GetSyntaxType();
                    type = syntaxType == null ? null : new NamedTypeReference(syntaxType, this);
                    typeSet = true;
                }

                return type;
            }
            set
            {
                Set(ref type, value);
                typeSet = true;
            }
        }

        private TypeSyntax? GetSyntaxType() =>
            syntax is ObjectCreationExpressionSyntax oce ? oce.Type : null;

        private SeparatedSyntaxList<Argument, ArgumentSyntax>? arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(
                        syntax.ArgumentList?.Arguments ?? default, this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        private bool initializerSet;
        private Initializer? initializer;
        public Initializer? Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    initializer = FromRoslyn.Initializer(syntax.Initializer, this);
                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }

        private protected override ExpressionSyntax GetWrappedExpression(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newType = type?.GetWrapped(ref thisChanged) ?? GetSyntaxType();
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList?.Arguments ?? default;
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = newType == null
                    ? RoslynSyntaxFactory.ImplicitObjectCreationExpression(
                        RoslynSyntaxFactory.ArgumentList(newArguments), newInitializer)
                    : RoslynSyntaxFactory.ObjectCreationExpression(
                        newType, RoslynSyntaxFactory.ArgumentList(newArguments), newInitializer);

                syntax = Annotate(syntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ObjectCreationExpressionSyntax)newSyntax;

            Set(ref type, null);
            SetList(ref arguments, null);
            Set(ref initializer, null);
        }

        private protected override SyntaxNode CloneImpl() => new NewExpression(Type, Arguments, Initializer);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }

            Initializer?.ReplaceExpressions(filter, projection);
        }
    }
}