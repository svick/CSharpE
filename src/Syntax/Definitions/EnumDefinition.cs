using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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
            Init((EnumDeclarationSyntax)newSyntax);
            SetList(ref attributes, null);
            SetList(ref members, null);
        }

        internal override SyntaxNode Clone() =>
            new EnumDefinition(Modifiers, Name, Members) { Attributes = Attributes };

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        private const MemberModifiers ValidModifiers = AccessModifiersMask | New;
        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for an enum.", nameof(value));
        }

        EnumDeclarationSyntax ISyntaxWrapper<EnumDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = RoslynSyntaxFactory.EnumDeclaration(
                    newAttributes, newModifiers.GetWrapped(), newName, default, newMembers);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            this.GetWrapped<EnumDeclarationSyntax>(ref changed);

        public override IEnumerable<SyntaxNode> GetChildren() => Attributes.Concat<SyntaxNode>(Members);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var member in Members)
            {
                member.Initializer = Expression.ReplaceExpressions(member.Initializer, filter, projection);
            }
        }
    }
}