﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class NewExpression : Expression
    {
        private ObjectCreationExpressionSyntax syntax;

        internal NewExpression(ObjectCreationExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public NewExpression(TypeReference type, IEnumerable<Argument> arguments, Initializer initializer = null)
        {
            this.Type = type;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
            this.Initializer = initializer;
        }

        public NewExpression(TypeReference type, IEnumerable<Expression> arguments, Initializer initializer = null)
            : this(type, arguments.Select(a => (Argument)a), initializer) { }

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
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(
                        syntax.ArgumentList?.Arguments ?? default, this);

                return arguments;
            }
            set => SetList(ref arguments, new SeparatedSyntaxList<Argument, ArgumentSyntax>(value, this));
        }

        private bool initializerSet;
        private Initializer initializer;
        public Initializer Initializer
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

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList?.Arguments ?? default;
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.Initializer;

            if (syntax == null || thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                syntax = RoslynSyntaxFactory.ObjectCreationExpression(
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