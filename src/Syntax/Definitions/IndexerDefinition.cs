using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class IndexerDefinition : BasePropertyDefinition, ISyntaxWrapper<IndexerDeclarationSyntax>
    {
        private IndexerDeclarationSyntax syntax;

        private protected override BasePropertyDeclarationSyntax BasePropertySyntax => syntax;

        public IndexerDefinition(IndexerDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(IndexerDeclarationSyntax syntax)
        {
            this.syntax = syntax;
            
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Virtual | Sealed | Override | Abstract | Extern | Unsafe;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(
                    $"The modifiers {invalidModifiers} are not valid for an indexer.", nameof(value));
        }

        private SeparatedSyntaxList<Parameter, ParameterSyntax> parameters;
        public IList<Parameter> Parameters
        {
            get
            {
                if (parameters == null)
                    parameters = new SeparatedSyntaxList<Parameter, ParameterSyntax>(
                        syntax.ParameterList.Parameters, this);

                return parameters;
            }
            set => SetList(ref parameters, new SeparatedSyntaxList<Parameter, ParameterSyntax>(value, this));
        }

        // TODO: expression body
        
        IndexerDeclarationSyntax ISyntaxWrapper<IndexerDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            throw new NotImplementedException();
        }

        private protected override BasePropertyDeclarationSyntax GetWrappedBaseProperty(ref bool? changed) =>
            this.GetWrapped<IndexerDeclarationSyntax>(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }

    }
}