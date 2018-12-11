using System;
using Microsoft.CodeAnalysis;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal struct Identifier
    {
        private SyntaxToken syntax;

        private bool canBeNull;

        public Identifier(SyntaxToken syntax, bool canBeNull = false) : this()
        {
            this.syntax = syntax;
            this.canBeNull = canBeNull;
        }

        public Identifier(string text, bool canBeNull = false) : this()
        {
            this.canBeNull = canBeNull;
            this.Text = text;
        }

        private bool textSet;
        private string text;
        public string Text
        {
            get => textSet ? text : syntax.ValueText;
            set
            {
                if (!canBeNull && value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value == string.Empty)
                    throw new ArgumentException(nameof(value));

                text = value;
                textSet = true;
            }
        }

        // this type does not implement ISyntaxWrapper, because that would lead to temporary boxing
        // which would mean that the change in syntax would not be remembered by the original instance
        internal SyntaxToken GetWrapped(ref bool? changed)
        {
            if (Text != syntax.ValueText)
            {
                syntax = text == null ? default : CSharpSyntaxFactory.Identifier(text);

                changed = true;
            }

            return syntax;
        }
    }
}