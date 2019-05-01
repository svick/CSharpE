using System;
using System.Diagnostics;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class MemberInitializer : SyntaxNode, ISyntaxWrapper<AssignmentExpressionSyntax>
    {
        private AssignmentExpressionSyntax syntax;

        internal MemberInitializer(AssignmentExpressionSyntax syntax, ObjectInitializer parent)
        {
            Debug.Assert(syntax.Kind() == SyntaxKind.SimpleAssignmentExpression);

            this.syntax = syntax;
            Parent = parent;
        }

        public MemberInitializer(MemberInitializerTarget target, MemberInitializerValue value)
        {
            Target = target;
            Value = value;
        }

        private MemberInitializerTarget target;
        public MemberInitializerTarget Target
        {
            get
            {
                if (target == null)
                {
                    switch (syntax.Left)
                    {
                        case IdentifierNameSyntax identifierName:
                            target = new NameMemberInitializerTarget(identifierName, this);
                            break;
                        case ImplicitElementAccessSyntax implicitElementAccess:
                            target = new ElementAccessMemberInitializerTarget(implicitElementAccess, this);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }

                return target;
            }
            set => SetNotNull(ref target, value);
        }

        private MemberInitializerValue value;
        public MemberInitializerValue Value
        {
            get
            {
                if (target == null)
                {
                    switch (syntax.Right)
                    {
                        case InitializerExpressionSyntax initializer:
                            value = new InitializerMemberInitializerValue(initializer, this);
                            break;
                        case var expression:
                            value = new ExpressionMemberInitializerValue(expression, this);
                            break;
                    }
                }

                return value;
            }
            set => SetNotNull(ref this.value, value);
        }

        AssignmentExpressionSyntax ISyntaxWrapper<AssignmentExpressionSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newTarget = target?.GetWrapped(ref thisChanged) ?? syntax.Left;
            var newValue = value?.GetWrapped(ref thisChanged) ?? syntax.Right;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, newTarget, newValue);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Set(ref target, null);
            Set(ref value, null);
            syntax = (AssignmentExpressionSyntax)newSyntax;
        }

        private protected override SyntaxNode CloneImpl() => new MemberInitializer(Target, Value);
    }
}
