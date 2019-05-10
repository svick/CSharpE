using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ConstructorInitializer : SyntaxNode, ISyntaxWrapper<ConstructorInitializerSyntax>
    {
        private ConstructorInitializerSyntax syntax;

        internal ConstructorInitializer(ConstructorInitializerSyntax syntax, ConstructorDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ConstructorInitializerSyntax syntax)
        {
            this.syntax = syntax;
            Kind = GetSyntaxKind();
        }

        public ConstructorInitializer(ConstructorInitializerKind kind, params Argument[] arguments)
            : this(kind, arguments.AsEnumerable()) { }

        public ConstructorInitializer(ConstructorInitializerKind kind, IEnumerable<Argument> arguments)
        {
            Kind = kind;
            this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);
        }

        private ConstructorInitializerKind GetSyntaxKind() =>
            syntax.ThisOrBaseKeyword.Kind() == SyntaxKind.ThisKeyword
                ? ConstructorInitializerKind.This
                : ConstructorInitializerKind.Base;

        public ConstructorInitializerKind Kind { get; set; }

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

        internal ConstructorInitializerSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList.Arguments;

            if (syntax == null || thisChanged == true || Kind != GetSyntaxKind())
            {
                syntax = RoslynSyntaxFactory.ConstructorInitializer(
                    Kind == ConstructorInitializerKind.This
                        ? SyntaxKind.ThisConstructorInitializer
                        : SyntaxKind.BaseConstructorInitializer,
                    RoslynSyntaxFactory.ArgumentList(newArguments));

                SetChanged(ref changed);
            }

            return syntax;
        }

        ConstructorInitializerSyntax ISyntaxWrapper<ConstructorInitializerSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref arguments, null);
            Init((ConstructorInitializerSyntax)newSyntax);
        }

        private protected override SyntaxNode CloneImpl() => new ConstructorInitializer(Kind, Arguments);

        public void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) where T : Expression
        {
            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }
        }
    }

    public enum ConstructorInitializerKind
    {
        This,
        Base
    }
}