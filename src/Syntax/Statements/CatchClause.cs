using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class CatchClause : SyntaxNode, ISyntaxWrapper<CatchClauseSyntax>
    {
        CatchClauseSyntax ISyntaxWrapper<CatchClauseSyntax>.GetWrapped(ref bool? changed)
        {
            throw new NotImplementedException();
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

        private TryStatement parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                if (value is TryStatement tryStatement)
                    parent = tryStatement;
                else
                    throw new ArgumentException(nameof(value));
            }
        }

    }
}