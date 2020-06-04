using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class VarPattern : Pattern
    {
        private VarPatternSyntax syntax;

        internal VarPattern(VarPatternSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public VarPattern(VariableDesignation designation) => Designation = designation;

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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newDesignation = designation?.GetWrapped(ref thisChanged) ?? syntax.Designation;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.VarPattern(newDesignation);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (VarPatternSyntax)newSyntax;
            Set(ref designation, null);
        }

        private protected override SyntaxNode CloneImpl() => new VarPattern(Designation);

        public override void ReplaceExpressions<T>(Func<T, bool> filter, Func<T, Expression> projection) { }
    }
}