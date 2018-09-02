using System;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    // TODO: attributes
    public sealed class EnumMemberDefinition : SyntaxNode, ISyntaxWrapper<EnumMemberDeclarationSyntax>
    {
        private EnumMemberDeclarationSyntax syntax;

        internal EnumMemberDefinition(EnumMemberDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
        {
            syntax = enumMemberDeclarationSyntax;

            name = new Identifier(enumMemberDeclarationSyntax.Identifier);
        }

        public EnumMemberDefinition(string name, Expression initializer = null)
        {
            Name = name;
            Initializer = initializer;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private bool initializerSet;
        private Expression initializer;
        public Expression Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    var syntaxInitializer = syntax.EqualsValue;

                    if (syntaxInitializer != null)
                        initializer = FromRoslyn.Expression(syntaxInitializer.Value, this);

                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }

        private EnumDefinition parent;
        internal override SyntaxNode Parent
        {
            get => parent;
            set
            {
                if (value is EnumDefinition parentEnum)
                    parent = parentEnum;
                else
                    throw new ArgumentException(nameof(value));
            }
        }

        private protected override void SetSyntaxImpl(Microsoft.CodeAnalysis.SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

        EnumMemberDeclarationSyntax ISyntaxWrapper<EnumMemberDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            throw new NotImplementedException();
        }
    }
}