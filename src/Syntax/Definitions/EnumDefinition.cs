using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    // TODO: underlying type
    public sealed class EnumDefinition : BaseTypeDefinition, ISyntaxWrapper<EnumDeclarationSyntax>
    {
        private EnumDeclarationSyntax syntax;

        internal EnumDefinition(EnumDeclarationSyntax enumDeclarationSyntax, SyntaxNode parent)
        {
            Init(enumDeclarationSyntax);

            Parent = parent;
        }

        public EnumDefinition(MemberModifiers modifiers, string name, IEnumerable<EnumMemberDefinition> members = null)
        {
            Modifiers = modifiers;
            Name = name;

            this.members = new SeparatedSyntaxList<EnumMemberDefinition, EnumMemberDeclarationSyntax>(members, this);
        }

        private void Init(EnumDeclarationSyntax enumDeclarationSyntax)
        {
            syntax = enumDeclarationSyntax;

            name = new Identifier(syntax.Identifier);
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        private SeparatedSyntaxList<EnumMemberDefinition, EnumMemberDeclarationSyntax> members;
        public IList<EnumMemberDefinition> Members
        {
            get
            {
                if (members == null)
                    members = new SeparatedSyntaxList<EnumMemberDefinition, EnumMemberDeclarationSyntax>(
                        syntax.Members, this);

                return members;
            }
            set => SetList(
                ref members, new SeparatedSyntaxList<EnumMemberDefinition, EnumMemberDeclarationSyntax>(value, this));
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            throw new System.NotImplementedException();
        }

        internal override SyntaxNode Clone()
        {
            throw new System.NotImplementedException();
        }

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private protected override void ValidateModifiers(MemberModifiers modifiers)
        {
            throw new System.NotImplementedException();
        }

        EnumDeclarationSyntax ISyntaxWrapper<EnumDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            throw new System.NotImplementedException();
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            this.GetWrapped<EnumDeclarationSyntax>(ref changed);
    }
}