using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax
{
    public abstract class MemberDefinition : ISyntaxWrapper<MemberDeclarationSyntax>
    {
        private MemberModifiers modifiers;
        public MemberModifiers Modifiers
        {
            get => modifiers;
            set
            {
                ValidateModifiers(value);
                modifiers = value;
            }
        }

        protected abstract void ValidateModifiers(MemberModifiers modifiers);

        public TypeDefinition ContainingType { get; internal set; }

        MemberDeclarationSyntax ISyntaxWrapper<MemberDeclarationSyntax>.GetWrapped() => GetWrapped();

        internal MemberDeclarationSyntax GetWrapped() => GetWrappedImpl();

        protected abstract MemberDeclarationSyntax GetWrappedImpl();
    }
}