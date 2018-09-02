using System;
using Microsoft.CodeAnalysis;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal struct Identifier
    {
        private SyntaxToken syntax;

        internal Identifier(SyntaxToken syntax) : this() => this.syntax = syntax;

        public Identifier(string text) : this()
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(nameof(text));

            this.text = text;
        }

        private string text;
        public string Text
        {
            get => text ?? syntax.ValueText;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException(nameof(value));

                text = value;
            }
        }

        // this type does not implement ISyntaxWrapper, because that would lead to temporary boxing
        // which would mean that the change in syntax would not be remembered by the original instance
        internal SyntaxToken GetWrapped(ref bool? changed)
        {
            var newText = text ?? syntax.ValueText;

            if (newText != syntax.ValueText)
            {
                syntax = CSharpSyntaxFactory.Identifier(text);

                changed = true;
            }

            return syntax;
        }
    }
}