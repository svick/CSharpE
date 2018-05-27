using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public class Attribute : SyntaxNode, ISyntaxWrapper<AttributeSyntax>
    {
        private AttributeSyntax syntax;

        internal MemberDefinition ParentMember { get; private set; }

        internal Attribute(AttributeSyntax syntax, MemberDefinition parent)
        {
            this.syntax = syntax;
            ParentMember = parent;
        }

        AttributeSyntax ISyntaxWrapper<AttributeSyntax>.GetWrapped(ref bool changed)
        {
            throw new NotImplementedException();
        }

        protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) => syntax = (AttributeSyntax)newSyntax;

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Parent
        {
            get => ParentMember;
            set
            {
                if (value is MemberDefinition memberDefinition)
                    ParentMember = memberDefinition;
                else
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}