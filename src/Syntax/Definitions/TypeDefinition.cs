using System;
using System.Collections.Generic;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class TypeDefinition : BaseTypeDefinition, ISyntaxWrapper<TypeDeclarationSyntax>
    {
        private TypeDeclarationSyntax syntax;
        
        internal TypeDefinition(TypeDeclarationSyntax typeDeclarationSyntax, SyntaxNode parent)
        {
            Init(typeDeclarationSyntax);

            Parent = parent;
        }

        private void Init(TypeDeclarationSyntax typeDeclarationSyntax)
        {
            syntax = typeDeclarationSyntax;

            name = new Identifier(syntax.Identifier);
            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
        }

        private protected override MemberDeclarationSyntax Syntax => syntax;

        private MemberList members;
        private MemberList MembersList
        {
            get
            {
                if (members == null)
                    members = new MemberList(syntax.Members, this);

                return members;
            }
        }

        public IList<MemberDefinition> Members
        {
            get => MembersList;
            set => SetList(ref members, new MemberList(value, this));
        }

        public IList<FieldDefinition> Fields
        {
            get => FilteredList.Create<MemberDefinition, FieldDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public IList<MethodDefinition> Methods
        {
            get => FilteredList.Create<MemberDefinition, MethodDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public IList<MethodDefinition> PublicMethods
        {
            get => FilteredList.Create(MembersList, (MethodDefinition method) => method.IsPublic);
            set => FilteredList.Set(MembersList, method => method.IsPublic, value);
        }

        public IList<TypeDefinition> Types
        {
            get => FilteredList.Create<MemberDefinition, TypeDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
        }

        public FieldDefinition AddField(
            MemberModifiers modifiers, TypeReference type, string name, Expression initializer = null)
        {
            var field = new FieldDefinition(modifiers, type, name, initializer);
            this.Members.Add(field);

            // TODO: SyntaxList will probably have to notify its parent, so that it can set ContainingType of the child
            field.Parent = this;

            return field;
        }

        public FieldDefinition AddField(TypeReference type, string name, Expression initializer = null) =>
            AddField(MemberModifiers.None, type, name, initializer);

        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) =>
            new IdentifierExpression(typeDefinition.Name);

        // TODO: namespace
        public NamedTypeReference GetReference() => new NamedTypeReference(null, Name);

        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Abstract | Sealed | Static | Unsafe | Partial;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a type.", nameof(value));
        }

        private protected abstract SyntaxKind KeywordKind { get; }

        internal TypeDeclarationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || AttributesChanged() || thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = CSharpSyntaxFactory.TypeDeclaration(
                    SyntaxFacts.GetTypeDeclarationKind(KeywordKind), GetNewAttributes(), default,
                    CSharpSyntaxFactory.Token(KeywordKind), newName, default, default, default,
                    CSharpSyntaxFactory.Token(OpenBraceToken), newMembers, CSharpSyntaxFactory.Token(CloseBraceToken),
                    default);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedImpl(ref bool? changed) => GetWrapped(ref changed);

        TypeDeclarationSyntax ISyntaxWrapper<TypeDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((TypeDeclarationSyntax)newSyntax);
            ResetAttributes();

            members = null;
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ClassDefinition : TypeDefinition, ISyntaxWrapper<ClassDeclarationSyntax>
    {
        internal ClassDefinition(ClassDeclarationSyntax classDeclarationSyntax, SyntaxNode parent)
            : base(classDeclarationSyntax, parent) { }

        private protected override SyntaxKind KeywordKind => ClassKeyword;

        ClassDeclarationSyntax ISyntaxWrapper<ClassDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (ClassDeclarationSyntax)GetWrapped(ref changed);
    }

    public sealed class StructDefinition : TypeDefinition, ISyntaxWrapper<StructDeclarationSyntax>
    {
        internal StructDefinition(StructDeclarationSyntax structDeclarationSyntax, SyntaxNode parent)
            : base(structDeclarationSyntax, parent) { }

        private protected override SyntaxKind KeywordKind => StructKeyword;

        StructDeclarationSyntax ISyntaxWrapper<StructDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (StructDeclarationSyntax)GetWrapped(ref changed);
    }

    public sealed class InterfaceDefinition : TypeDefinition, ISyntaxWrapper<InterfaceDeclarationSyntax>
    {
        internal InterfaceDefinition(InterfaceDeclarationSyntax interfaceDeclarationSyntax, SyntaxNode parent)
            : base(interfaceDeclarationSyntax, parent) { }

        private protected override SyntaxKind KeywordKind => InterfaceKeyword;

        InterfaceDeclarationSyntax ISyntaxWrapper<InterfaceDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (InterfaceDeclarationSyntax)GetWrapped(ref changed);
    }
}