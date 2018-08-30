using System;
using CSharpE.Syntax.Internals;

namespace CSharpE.Syntax
{
    public abstract class BaseTypeDefinition : MemberDefinition
    {
        private protected Identifier name;

        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private SyntaxNode parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                switch (value)
                {
                    case TypeDefinition _:
                    case NamespaceDefinition _:
                    case SourceFile _:
                        parent = value;
                        return;
                }

                throw new ArgumentException(nameof(value));
            }
        }
    }
}