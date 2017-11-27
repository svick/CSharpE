﻿using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    // TODO: object initializer, anonymous types
    public class NewExpression : Expression
    {
        private ObjectCreationExpressionSyntax syntax;
        private SyntaxContext context;

        internal NewExpression(ObjectCreationExpressionSyntax syntax, SyntaxContext context)
        {
            this.syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            this.context = context;
        }

        public NewExpression(TypeReference type, IEnumerable<Argument> arguments)
        {
            this.Type = type;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments);
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
                {
                    type = new NamedTypeReference(syntax.Type, context);
                    context = default;
                }

                return type;
            }
            set => type = value ?? throw new ArgumentNullException(nameof(value));
        }

        private SeparatedSyntaxList<Argument, ArgumentSyntax> arguments;
        public IList<Argument> Arguments
        {
            get
            {
                if (arguments == null)
                    arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(syntax.ArgumentList.Arguments);

                return arguments;
            }
            set => arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(value);
        }

        internal override ExpressionSyntax GetWrapped()
        {
            var newType = type?.GetWrapped() ?? syntax.Type;
            var newArguments = arguments?.GetWrapped() ?? syntax.ArgumentList.Arguments;

            if (syntax == null || newType != syntax.Type || newArguments != syntax.ArgumentList.Arguments)
            {
                syntax = CSharpSyntaxFactory.ObjectCreationExpression(
                    newType, CSharpSyntaxFactory.ArgumentList(newArguments), null);
            }

            return syntax;
        }
    }
}