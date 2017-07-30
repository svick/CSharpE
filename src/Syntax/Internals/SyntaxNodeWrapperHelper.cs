using System;

namespace CSharpE.Syntax.Internals
{
    internal class SyntaxNodeWrapperHelper<TWrapper, TNode>
    {
        public bool Changed { get; private set; }

        public void ResetChanged() => Changed = false;

        private bool syntaxNodeOutdated;

        // TODO: consider adding parameter for this, so that the delegate can be statically cached
        public TNode GetSyntaxNode(ref TNode syntaxNodeField, TWrapper wrapper, Func<TWrapper, TNode> syntaxNodeGenerator)
        {
            if (syntaxNodeOutdated)
            {
                syntaxNodeField = syntaxNodeGenerator(wrapper);
                syntaxNodeOutdated = false;
            }

            return syntaxNodeField;
        }

        public void SetField<TField>(ref TField field, TField value)
        {
            if (!Equals(field, value))
            {
                Changed = true;
                syntaxNodeOutdated = true;
            }

            field = value;
        }
    }
}