using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;

namespace CSharpE.Syntax
{
    public class MethodDefinition : MemberDefinition
    {
        #region Modifiers

        private const MemberModifiers ValidMethodModifiers =
            AccessModifiersMask | New | Static | Unsafe | Abstract | Sealed | Virtual | Override | Extern | Partial |
            Async;
        
        protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException(nameof(value), $"The modifiers {invalidModifiers} are not valid for a method.");
        }

        public MemberModifiers Accessibility
        {
            get => Modifiers.Accessibility();
            set => Modifiers = Modifiers.WithAccessibilityModifier(value);
        }
        
        public bool IsPublic => Accessibility == Public;
        public bool IsProtected => Accessibility == Protected;
        public bool IsInternal => Accessibility == Internal;
        public bool IsPrivate => Accessibility == Private;
        public bool IsProtectedInternal => Accessibility == ProtectedInternal;

        public bool IsNew
        {
            get => Modifiers.Contains(New);
            set => Modifiers = Modifiers.With(New, value);
        }
        
        public bool IsStatic
        {
            get => Modifiers.Contains(Static);
            set => Modifiers = Modifiers.With(Static, value);
        }
        
        public bool IsUnsafe
        {
            get => Modifiers.Contains(Unsafe);
            set => Modifiers = Modifiers.With(Unsafe, value);
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
        
        public bool IsVirtual
        {
            get => Modifiers.Contains(Virtual);
            set => Modifiers = Modifiers.With(Virtual, value);
        }
        
        public bool IsOverride
        {
            get => Modifiers.Contains(Override);
            set => Modifiers = Modifiers.With(Override, value);
        }
        
        public bool IsExtern
        {
            get => Modifiers.Contains(Extern);
            set => Modifiers = Modifiers.With(Extern, value);
        }
        
        public bool IsPartial
        {
            get => Modifiers.Contains(Partial);
            set => 
                Modifiers = Modifiers.With(Partial, value);
        }
        
        public bool IsAsync
        {
            get => Modifiers.Contains(Async);
            set => Modifiers = Modifiers.With(Async, value);
        }
        
        #endregion

        public TypeReference ReturnType { get; set; }

        private MethodDeclarationSyntax syntax;
        
        private List<Statement> body;

        public MethodDefinition(MethodDeclarationSyntax syntax, TypeDefinition containingType)
        {
            this.syntax = syntax;
            ContainingType = containingType;
        }

        public IList<Statement> Body
        {
            get => body;
            set => body = value.ToList();
        }
    }
}