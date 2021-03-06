﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax.Internals
{
    internal struct Identifier
    {
        private SyntaxToken syntax;

        private readonly bool canBeNull;

        public Identifier(SyntaxToken? syntax) : this(syntax ?? default, true) { }

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
            get => textSet ? text : (string)syntax.Value;
            set
            {
                if (!canBeNull && value == null)
                    throw new ArgumentNullException(nameof(value));

                if (value == string.Empty)
                    throw new ArgumentException("Identifier can't be the empty string.", nameof(value));

                text = value;
                textSet = true;
            }
        }

        // this type does not implement ISyntaxWrapper, because that would lead to temporary boxing
        // which would mean that the change in syntax would not be remembered by the original instance
        internal SyntaxToken GetWrapped(ref bool? changed)
        {
            if (Text != (string)syntax.Value)
            {
                if (text == null)
                    syntax = default;
                else if (text == "_")
                    syntax = RoslynSyntaxFactory.Identifier(default, SyntaxKind.UnderscoreToken, text, text, default);
                else
                    syntax = RoslynSyntaxFactory.Identifier(text);

                changed = true;
            }

            return syntax;
        }
    }
}