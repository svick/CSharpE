using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class CatchClause : SyntaxNode, ISyntaxWrapper<CatchClauseSyntax>
    {
        public TryStatement ParentStatement { get; private set; }

        public CatchClauseSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren()
        {
            throw new System.NotImplementedException();
        }

        public override SyntaxNode Parent
        {
            get => ParentStatement;
            internal set
            {
                if (value is TryStatement tryStatement)
                    ParentStatement = tryStatement;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}