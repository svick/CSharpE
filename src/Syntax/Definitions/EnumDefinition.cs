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
    public sealed class EnumDefinition : BaseTypeDefinition, ISyntaxWrapper<EnumDeclarationSyntax>
    {
        private EnumDeclarationSyntax syntax;

        internal EnumDefinition(EnumDeclarationSyntax syntax, SyntaxNode parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        public EnumDefinition(string name, params EnumMemberDefinition[] members)
            : this(name, members.AsEnumerable()) { }

        public EnumDefinition(string name, IEnumerable<EnumMemberDefinition> members)
            : this(default, name, members) { }

        public EnumDefinition(MemberModifiers modifiers, string name, params EnumMemberDefinition[] members)
            : this(modifiers, name, members.AsEnumerable()) { }

        public EnumDefinition(MemberModifiers modifiers, string name, IEnumerable<EnumMemberDefinition> members)
            : this(modifiers, name, null, members) { }

        public EnumDefinition(
            MemberModifiers modifiers, string name, TypeReference underlyingType, IEnumerable<EnumMemberDefinition> members)
        {
            Modifiers = modifiers;
            Name = name;
            UnderlyingType = underlyingType;
            Members = members?.ToList();
        }

        private void Init(EnumDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            name = new Identifier(this.syntax.Identifier);
            Modifiers = FromRoslyn.MemberModifiers(this.syntax.Modifiers);
        }

        private bool underlyingTypeSet;
        private TypeReference underlyingType;
        public TypeReference UnderlyingType
        {
            get
            {
                if (!underlyingTypeSet)
                {
                    underlyingType = FromRoslyn.TypeReference(syntax.BaseList?.Types.Single().Type, this);
                    underlyingTypeSet = true;
                }

                return underlyingType;
            }
            set
            {
                Set(ref underlyingType, value);
                underlyingTypeSet = true;
            }
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
            Set(ref underlyingType, null);
            underlyingTypeSet = false;
            SetList(ref members, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new EnumDefinition(Modifiers, Name, UnderlyingType, Members) { Attributes = Attributes };

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
            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newUnderlyingType = underlyingTypeSet ? underlyingType?.GetWrapped(ref thisChanged) : syntax.BaseList?.Types.Single().Type;
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var baseList = newUnderlyingType == null
                    ? null
                    : RoslynSyntaxFactory.BaseList(
                        RoslynSyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(RoslynSyntaxFactory.SimpleBaseType(newUnderlyingType)));

                var newSyntax = RoslynSyntaxFactory.EnumDeclaration(newAttributes, newModifiers.GetWrapped(), newName, baseList, newMembers);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            this.GetWrapped<EnumDeclarationSyntax>(ref changed);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var member in Members)
            {
                member.Initializer = Expression.ReplaceExpressions(member.Initializer, filter, projection);
            }
        }
    }
}