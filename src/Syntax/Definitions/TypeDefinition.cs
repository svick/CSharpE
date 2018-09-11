using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public interface ILimitedTypeDefinition
    {
        NamedTypeReference GetReference();
        
        FieldDefinition AddField(
            MemberModifiers modifiers, TypeReference type, string name, Expression initializer = null);

        FieldDefinition AddField(TypeReference type, string name, Expression initializer = null);

        PropertyDefinition AddAutoProperty(
            MemberModifiers modifiers, TypeReference type, string name, bool getOnly = false);

        MethodDefinition AddMethod(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            params Statement[] body);

        MethodDefinition AddMethod(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body);

        ConstructorDefinition AddConstructor(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, IEnumerable<Statement> body);
    }
    
    public abstract class TypeDefinition
        : BaseTypeDefinition, ISyntaxWrapper<TypeDeclarationSyntax>, ILimitedTypeDefinition
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

        public TypeDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
        {
            Modifiers = modifiers;
            Name = name;
            Members = members?.ToList();
            baseTypes = new SeparatedSyntaxList<BaseType, BaseTypeSyntax>(this);
        }

        private protected override MemberDeclarationSyntax MemberSyntax => syntax;

        public bool IsNew
        {
            get => Modifiers.Contains(New);
            set => Modifiers = Modifiers.With(New, value);
        }

        public bool IsUnsafe
        {
            get => Modifiers.Contains(Unsafe);
            set => Modifiers = Modifiers.With(Unsafe, value);
        }

        public bool IsPartial
        {
            get => Modifiers.Contains(Partial);
            set => Modifiers = Modifiers.With(Partial, value);
        }

        private SeparatedSyntaxList<BaseType, BaseTypeSyntax> baseTypes;
        public IList<TypeReference> BaseTypes
        {
            get
            {
                if (baseTypes == null)
                    baseTypes = new SeparatedSyntaxList<BaseType, BaseTypeSyntax>(
                        syntax.BaseList?.Types ?? default, this);

                return ProjectionList.Create(
                    baseTypes, baseType => baseType.Type, reference => new BaseType(reference, this));
            }
            set => ProjectionList.Set(baseTypes, reference => new BaseType(reference, this), value);
        }

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

        // PERF: allocations
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

        public PropertyDefinition AddAutoProperty(
            MemberModifiers modifiers, TypeReference type, string name, bool getOnly = false)
        {
            var property = new PropertyDefinition(modifiers, type, name);
            
            property.GetAccessor = new AccessorDefinition();
            
            if (!getOnly)
                property.SetAccessor = new AccessorDefinition();
            
            this.Members.Add(property);

            property.Parent = this;

            return property;
        }

        public MethodDefinition AddMethod(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            params Statement[] body) =>
            AddMethod(modifiers, returnType, name, parameters, body.AsEnumerable());

        public MethodDefinition AddMethod(
            MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body)
        {
            var method = new MethodDefinition(modifiers, returnType, name, parameters, body);
            
            this.Members.Add(method);

            method.Parent = this;

            return method;
        }

        public ConstructorDefinition AddConstructor(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            var contructor = new ConstructorDefinition(modifiers, parameters, body);
            
            this.Members.Add(contructor);

            contructor.Parent = this;

            return contructor;
        }

        public static implicit operator IdentifierExpression(TypeDefinition typeDefinition) =>
            new IdentifierExpression(typeDefinition.Name);

        // TODO: namespace
        public NamedTypeReference GetReference() => new NamedTypeReference(null, Name);

        private protected abstract SyntaxKind KeywordKind { get; }

        private protected TypeDeclarationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newBaseTypes = baseTypes?.GetWrapped(ref thisChanged) ?? syntax.BaseList?.Types ?? default;
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || AttributesChanged() || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                var newBaseList = newBaseTypes.Any() ? CSharpSyntaxFactory.BaseList(newBaseTypes) : default;
                
                var newSyntax = CSharpSyntaxFactory.TypeDeclaration(
                    SyntaxFacts.GetTypeDeclarationKind(KeywordKind), GetNewAttributes(), newModifiers.GetWrapped(),
                    CSharpSyntaxFactory.Token(KeywordKind), newName, default, newBaseList, default,
                    CSharpSyntaxFactory.Token(OpenBraceToken), newMembers, CSharpSyntaxFactory.Token(CloseBraceToken),
                    default);

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            GetWrapped(ref changed);

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

        public ClassDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
            : base(modifiers, name, members) { }

        public ClassDefinition(string name, IEnumerable<MemberDefinition> members = null)
            : this(MemberModifiers.None, name, members) { }

        private protected override SyntaxKind KeywordKind => ClassKeyword;

        private const MemberModifiers ValidModifiers =
            AccessModifiersMask | New | Abstract | Sealed | Static | Unsafe | Partial;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a class.", nameof(value));
        }

        public bool IsAbstract
        {
            get => Modifiers.Contains(Abstract);
            set => Modifiers = Modifiers.With(Abstract, value);
        }

        public bool IsSealed
        {
            get => Modifiers.Contains(Sealed);
            set => Modifiers = Modifiers.With(Sealed, value);
        }

        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }

        ClassDeclarationSyntax ISyntaxWrapper<ClassDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (ClassDeclarationSyntax)GetWrapped(ref changed);
    }

    public sealed class StructDefinition : TypeDefinition, ISyntaxWrapper<StructDeclarationSyntax>
    {
        internal StructDefinition(StructDeclarationSyntax structDeclarationSyntax, SyntaxNode parent)
            : base(structDeclarationSyntax, parent) { }

        public StructDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
            : base(modifiers, name, members) { }

        public StructDefinition(string name, IEnumerable<MemberDefinition> members = null)
            : this(MemberModifiers.None, name, members) { }

        private protected override SyntaxKind KeywordKind => StructKeyword;

        private const MemberModifiers ValidModifiers = AccessModifiersMask | New | Unsafe | Partial | ReadOnly;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for a struct.", nameof(value));
        }

        StructDeclarationSyntax ISyntaxWrapper<StructDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (StructDeclarationSyntax)GetWrapped(ref changed);
    }

    public sealed class InterfaceDefinition : TypeDefinition, ISyntaxWrapper<InterfaceDeclarationSyntax>
    {
        internal InterfaceDefinition(InterfaceDeclarationSyntax interfaceDeclarationSyntax, SyntaxNode parent)
            : base(interfaceDeclarationSyntax, parent) { }

        public InterfaceDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
            : base(modifiers, name, members) { }

        public InterfaceDefinition(string name, IEnumerable<MemberDefinition> members = null)
            : this(MemberModifiers.None, name, members) { }

        private protected override SyntaxKind KeywordKind => InterfaceKeyword;

        private const MemberModifiers ValidModifiers = AccessModifiersMask | New | Unsafe | Partial;

        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for an interface.", nameof(value));
        }

        InterfaceDeclarationSyntax ISyntaxWrapper<InterfaceDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            (InterfaceDeclarationSyntax)GetWrapped(ref changed);
    }
}