using System;
using Microsoft.CodeAnalysis;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal struct Identifier : ISyntaxWrapper<SyntaxToken>
    {
        private SyntaxToken syntax;

        // text used by syntax, computed lazily
        private string syntaxText;
        private string text;

        public string Text
        {
            get
            {
                if (syntaxText == null && text == null)
                {
                    syntaxText = syntax.ValueText;
                }

                return text ?? syntaxText;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException(nameof(value));

                text = value;
            }
        }

        public Identifier(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentException(nameof(text));

            syntax = default;
            syntaxText = null;

            this.text = text;
        }

        internal Identifier(SyntaxToken syntax)
        {
            this.syntax = syntax;
            syntaxText = null;

            text = null;
        }

        public SyntaxToken GetWrapped()
        {
            // don't have to do anything if either both are null or both are non-null and equal
            if (syntaxText != text)
            {
                syntax = CSharpSyntaxFactory.Identifier(text);
                syntaxText = text;
            }

            return syntax;
        }

    }
}