using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public class Attribute : SyntaxNode, ISyntaxWrapper<AttributeSyntax>
    {
        private AttributeSyntax syntax;

        public MemberDefinition ParentMember { get; private set; }

        internal Attribute(AttributeSyntax syntax, MemberDefinition parent)
        {
            this.syntax = syntax;
            ParentMember = parent;
        }

        public AttributeSyntax GetWrapped()
        {
            throw new NotImplementedException();
        }

        // TODO
        protected override IEnumerable<IEnumerable<SyntaxNode>> GetChildren() => Enumerable.Empty<IEnumerable<SyntaxNode>>();

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