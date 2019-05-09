using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
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
            : base(typeDeclarationSyntax)
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
                    baseTypes, baseType => baseType.Type, reference => new BaseType(reference));
            }
            set => SetList(
                ref baseTypes,
                new SeparatedSyntaxList<BaseType, BaseTypeSyntax>(
                    value.Select(reference => new BaseType(reference)), this));
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

        public IList<ConstructorDefinition> Constructors
        {
            get => FilteredList.Create<MemberDefinition, ConstructorDefinition>(MembersList);
            set => FilteredList.Set(MembersList, value);
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

            Members.Add(field);

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
            
            Members.Add(property);

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
            
            Members.Add(method);

            return method;
        }

        public ConstructorDefinition AddConstructor(
            MemberModifiers modifiers, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            var constructor = new ConstructorDefinition(modifiers, parameters, body);
            
            Members.Add(constructor);

            return constructor;
        }

        private protected abstract SyntaxKind KeywordKind { get; }

        private protected TypeDeclarationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newName = name.GetWrapped(ref thisChanged);
            var newBaseTypes = baseTypes?.GetWrapped(ref thisChanged) ?? syntax.BaseList?.Types ?? default;
            var newMembers = members?.GetWrapped(ref thisChanged) ?? syntax.Members;

            if (syntax == null || FromRoslyn.MemberModifiers(syntax.Modifiers) != newModifiers ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var newBaseList = newBaseTypes.Any() ? RoslynSyntaxFactory.BaseList(newBaseTypes) : default;
                
                var newSyntax = RoslynSyntaxFactory.TypeDeclaration(
                    SyntaxFacts.GetTypeDeclarationKind(KeywordKind), newAttributes, newModifiers.GetWrapped(),
                    RoslynSyntaxFactory.Token(KeywordKind), newName, default, newBaseList, default,
                    RoslynSyntaxFactory.Token(OpenBraceToken), newMembers, RoslynSyntaxFactory.Token(CloseBraceToken),
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

            SetList(ref attributes, null);
            SetList(ref members, null);
        }

        public override IEnumerable<SyntaxNode> GetChildren() =>
            Attributes.Concat<SyntaxNode>(BaseTypes).Concat(Members);

        protected override void ReplaceExpressionsImpl<T>(Func<T, bool> filter, Func<T, Expression> projection)
        {
            foreach (var member in Members)
            {
                member.ReplaceExpressions(filter, projection);
            }
        }
    }

    public sealed class ClassDefinition : TypeDefinition
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

        private protected override SyntaxNode CloneImpl() => new ClassDefinition(Modifiers, Name, Members) { Attributes = Attributes };
    }

    public sealed class StructDefinition : TypeDefinition
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

        private protected override SyntaxNode CloneImpl() => new StructDefinition(Modifiers, Name, Members) { Attributes = Attributes };
    }

    public sealed class InterfaceDefinition : TypeDefinition
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

        private protected override SyntaxNode CloneImpl() => new InterfaceDefinition(Modifiers, Name, Members) { Attributes = Attributes };
    }
}