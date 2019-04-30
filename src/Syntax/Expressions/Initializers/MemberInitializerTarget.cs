using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class MemberInitializerTarget : SyntaxNode, ISyntaxWrapper<ExpressionSyntax>
    {
        internal abstract ExpressionSyntax GetWrapped(ref bool? changed);

        ExpressionSyntax ISyntaxWrapper<ExpressionSyntax>.GetWrapped(ref bool? changed)
            => GetWrapped(ref changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class NameMemberInitializerTarget : MemberInitializerTarget
    {
        private IdentifierNameSyntax syntax;

        internal NameMemberInitializerTarget(IdentifierNameSyntax syntax, MemberInitializer parent)
        {
            Init(syntax);
            Parent = parent;
        }

        public NameMemberInitializerTarget(string name) => Name = name;

        private void Init(IdentifierNameSyntax syntax)
        {
            this.syntax = syntax;

            name = new Identifier(syntax.Identifier);
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.IdentifierName(newName);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
            => Init((IdentifierNameSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new NameMemberInitializerTarget(Name);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }

    public sealed class ElementAccessMemberInitializerTarget : MemberInitializerTarget
    {
        private ImplicitElementAccessSyntax syntax;

        internal ElementAccessMemberInitializerTarget(ImplicitElementAccessSyntax syntax, MemberInitializer parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ElementAccessMemberInitializerTarget(IEnumerable<Argument> arguments)
            => this.arguments = new SeparatedSyntaxList<Argument, ArgumentSyntax>(arguments, this);

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

        internal override ExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newArguments = arguments?.GetWrapped(ref thisChanged) ?? syntax.ArgumentList.Arguments;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ImplicitElementAccess(
                    RoslynSyntaxFactory.BracketedArgumentList(newArguments));

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref arguments, null);
            syntax = (ImplicitElementAccessSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ElementAccessMemberInitializerTarget(Arguments);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var argument in Arguments)
            {
                argument.ReplaceExpressions(filter, projection);
            }
        }
    }
}
