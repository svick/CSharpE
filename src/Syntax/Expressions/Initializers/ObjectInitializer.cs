using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class ObjectInitializer : Initializer
    {
        private InitializerExpressionSyntax syntax;

        internal ObjectInitializer(InitializerExpressionSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.ObjectInitializerExpression);

            this.syntax = syntax;
            Parent = parent;
        }

        public ObjectInitializer(params MemberInitializer[] memberInitializers)
            : this(memberInitializers.AsEnumerable()) { }

        public ObjectInitializer(IEnumerable<MemberInitializer> memberInitializers)
        {
            this.memberInitializers =
                new SeparatedSyntaxList<MemberInitializer, ExpressionSyntax>(memberInitializers, this);
        }

        private SeparatedSyntaxList<MemberInitializer, ExpressionSyntax> memberInitializers;
        public IList<MemberInitializer> MemberInitializers
        {
            get
            {
                if (memberInitializers == null)
                    memberInitializers =
                        new SeparatedSyntaxList<MemberInitializer, ExpressionSyntax>(syntax.Expressions, this);

                return memberInitializers;
            }
            set => SetList(
                ref memberInitializers,
                new SeparatedSyntaxList<MemberInitializer, ExpressionSyntax>(value, this));
        }

        internal override InitializerExpressionSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newMemberInitializers = memberInitializers?.GetWrapped(ref thisChanged) ?? syntax.Expressions;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.InitializerExpression(
                    SyntaxKind.ObjectInitializerExpression, newMemberInitializers);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            SetList(ref memberInitializers, null);
            syntax = (InitializerExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new ObjectInitializer(MemberInitializers);

        public override IEnumerable<SyntaxNode> GetChildren() => MemberInitializers;

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var initializer in MemberInitializers)
            {
                initializer.Target.ReplaceExpressions(filter, projection);
                initializer.Value.ReplaceExpressions(filter, projection);
            }
        }
    }
}
