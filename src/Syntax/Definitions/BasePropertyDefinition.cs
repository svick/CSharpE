using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class BasePropertyDefinition : MemberDefinition, ISyntaxWrapper<BasePropertyDeclarationSyntax>
    {
        private protected abstract BasePropertyDeclarationSyntax BasePropertySyntax { get; }
        
        private protected sealed override MemberDeclarationSyntax MemberSyntax => BasePropertySyntax;
        
        public bool IsNew
        {
            get => Modifiers.Contains(MemberModifiers.New);
            set => Modifiers = Modifiers.With(MemberModifiers.New, value);
        }

        public bool IsVirtual
        {
            get => Modifiers.Contains(MemberModifiers.Virtual);
            set => Modifiers = Modifiers.With(MemberModifiers.Virtual, value);
        }

        public bool IsSealed
        {
            get => Modifiers.Contains(MemberModifiers.Sealed);
            set => Modifiers = Modifiers.With(MemberModifiers.Sealed, value);
        }

        public bool IsOverride
        {
            get => Modifiers.Contains(MemberModifiers.Override);
            set => Modifiers = Modifiers.With(MemberModifiers.Override, value);
        }
        
        public bool IsAbstract
        {
            get => Modifiers.Contains(MemberModifiers.Abstract);
            set => Modifiers = Modifiers.With(MemberModifiers.Abstract, value);
        }

        public bool IsExtern
        {
            get => Modifiers.Contains(MemberModifiers.Extern);
            set => Modifiers = Modifiers.With(MemberModifiers.Extern, value);
        }

        public bool IsUnsafe
        {
            get => Modifiers.Contains(MemberModifiers.Unsafe);
            set => Modifiers = Modifiers.With(MemberModifiers.Unsafe, value);
        }

        private protected AccessorDeclarationSyntax FindAccessor(SyntaxKind accessorKind) =>
            BasePropertySyntax.AccessorList.Accessors.FirstOrDefault(a => a.IsKind(accessorKind));

        private protected bool getAccessorSet;
        private protected AccessorDefinition getAccessor;
        public AccessorDefinition GetAccessor
        {
            get
            {
                if (!getAccessorSet)
                {
                    // TODO: properties with more than one of each kind of accessor should probably be error nodes
                    var declaration = FindAccessor(SyntaxKind.GetAccessorDeclaration);
                    
                    if (declaration != null)
                        getAccessor = new AccessorDefinition(declaration, this);
                    
                    getAccessorSet = true;
                }
                return getAccessor;
            }
            set
            {
                Set(ref getAccessor, value);
                getAccessor.Kind = SyntaxKind.GetAccessorDeclaration;
                getAccessorSet = true;
            }
        }
        
        private protected bool setAccessorSet;
        private protected AccessorDefinition setAccessor;
        public AccessorDefinition SetAccessor
        {
            get
            {
                if (!setAccessorSet)
                {
                    // TODO: properties with more than one of each kind of accessor should probably be error nodes
                    var declaration = BasePropertySyntax.AccessorList.Accessors
                        .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
                    
                    if (declaration != null)
                        setAccessor = new AccessorDefinition(declaration, this);
                    
                    setAccessorSet = true;
                }
                return setAccessor;
            }
            set
            {
                Set(ref setAccessor, value);
                setAccessor.Kind = SyntaxKind.SetAccessorDeclaration;
                setAccessorSet = true;
            }
        }
        
        private protected TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(BasePropertySyntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        BasePropertyDeclarationSyntax ISyntaxWrapper<BasePropertyDeclarationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrappedBaseProperty(ref changed);

        private protected sealed override MemberDeclarationSyntax GetWrappedMember(ref bool? changed) =>
            GetWrappedBaseProperty(ref changed);

        private protected abstract BasePropertyDeclarationSyntax GetWrappedBaseProperty(ref bool? changed);
    }
}