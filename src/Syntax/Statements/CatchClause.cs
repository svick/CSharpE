using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class CatchClause : SyntaxNode, ISyntaxWrapper<CatchClauseSyntax>
    {
        public TryStatement ParentStatement { get; private set; }

        public CatchClauseSyntax GetWrapped()
        {
            throw new NotImplementedException();
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Parent
        {
            get => ParentStatement;
            set
            {
                if (value is TryStatement tryStatement)
                    ParentStatement = tryStatement;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}