using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class Pattern : SyntaxNode, ISyntaxWrapper<PatternSyntax>
    {
        private protected Pattern() { }
        private protected Pattern(PatternSyntax syntax) : base(syntax) { }

        PatternSyntax ISyntaxWrapper<PatternSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        protected abstract PatternSyntax GetWrapped(ref bool? changed);

        public abstract void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection)
            where T : Expression;
    }

    public sealed class ConstantPattern : Pattern
    {
        private ConstantPatternSyntax syntax;

        internal ConstantPattern(ConstantPatternSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public ConstantPattern(Expression expression) => Expression = expression;

        private Expression expression;
        public Expression Expression
        {
            get
            {
                if (expression == null)
                    expression = FromRoslyn.Expression(syntax.Expression, this);

                return expression;
            }
            set => SetNotNull(ref expression, value);
        }

        protected override PatternSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newExpression = expression?.GetWrapped(ref thisChanged) ?? syntax.Expression;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ConstantPattern(newExpression);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ConstantPatternSyntax)newSyntax;
            Set(ref expression, null);
        }

        private protected override SyntaxNode CloneImpl() => new ConstantPattern(Expression);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) =>
            Expression = Expression.ReplaceExpressions(Expression, filter, projection);
    }

    public sealed class TypePattern : Pattern
    {
        private DeclarationPatternSyntax syntax;

        internal TypePattern(DeclarationPatternSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        internal TypePattern(TypeReference type, VariableDesignation designation)
        {
            Type = type;
            Designation = designation;
        }

        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(syntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        private VariableDesignation designation;
        public VariableDesignation Designation
        {
            get
            {
                if (designation == null)
                    designation = FromRoslyn.VariableDesignation(syntax.Designation, this);

                return designation;
            }
            set => SetNotNull(ref designation, value);
        }

        protected override PatternSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newDesignation = designation?.GetWrapped(ref thisChanged) ?? syntax.Designation;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.DeclarationPattern(newType, newDesignation);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (DeclarationPatternSyntax)newSyntax;
            Set(ref type, null);
            Set(ref designation, null);
        }

        private protected override SyntaxNode CloneImpl() => new TypePattern(Type, Designation);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}