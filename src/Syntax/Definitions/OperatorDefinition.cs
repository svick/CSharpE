using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.OperatorKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using CSharpSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public enum OperatorKind
    {
        Plus,
        Addition = Plus,
        Minus,
        Subtraction = Minus,
        Not,
        Complement,
        Increment,
        Decrement,
        True,
        False,
        Multiplication,
        Division,
        Modulus,
        And,
        Or,
        Xor,
        LeftShift,
        RightShift,
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEquals,
        LessThanOrEquals,
        
        Implicit,
        Explicit
    }
    
    public sealed class OperatorDefinition : BaseMethodDefinition
    {
        private BaseMethodDeclarationSyntax syntax;

        private protected override BaseMethodDeclarationSyntax BaseMethodSyntax => syntax;

        internal OperatorDefinition(BaseMethodDeclarationSyntax syntax, TypeDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private static readonly BiDirectionalDictionary<OperatorKind, SyntaxKind> operatorKindMapping =
            new BiDirectionalDictionary<OperatorKind, SyntaxKind>
            {
                {OperatorKind.Equals, EqualsEqualsToken},
                {NotEquals, ExclamationEqualsToken},

                {Implicit, ImplicitKeyword},
                {Explicit, ExplicitKeyword}
            };

        private static OperatorKind GetKind(BaseMethodDeclarationSyntax syntax)
        {
            switch (syntax)
            {
                case OperatorDeclarationSyntax operatorDeclaration:
                    return operatorKindMapping[operatorDeclaration.OperatorToken.Kind()];
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    return operatorKindMapping[conversionOperatorDeclaration.ImplicitOrExplicitKeyword.Kind()];
                default:
                    throw new InvalidOperationException(syntax.GetType().Name);
            }
        }

        private void Init(BaseMethodDeclarationSyntax syntax)
        {
            this.syntax = syntax;

            Modifiers = FromRoslyn.MemberModifiers(syntax.Modifiers);
            Kind = GetKind(syntax);
        }

        public OperatorDefinition(
            MemberModifiers modifiers, TypeReference returnType, OperatorKind kind, IEnumerable<Parameter> parameters,
            IEnumerable<Statement> body)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            Kind = kind;
            Parameters = parameters?.ToList();
            Body = new BlockStatement(body?.ToList());
        }


        #region Modifiers

        private const MemberModifiers ValidMethodModifiers = Public | Static | Extern | Unsafe;
        private protected override void ValidateModifiers(MemberModifiers value)
        {
            var invalidModifiers = value & ~ValidMethodModifiers;
            if (invalidModifiers != 0)
                throw new ArgumentException($"The modifiers {invalidModifiers} are not valid for an operator.", nameof(value));
        }

        #endregion

        private static TypeSyntax GetReturnType(BaseMethodDeclarationSyntax syntax)
        {
            switch (syntax)
            {
                case OperatorDeclarationSyntax operatorDeclaration:
                    return operatorDeclaration.ReturnType;
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    return conversionOperatorDeclaration.Type;
                default:
                    throw new InvalidOperationException();
            }
        }

        private TypeReference returnType;
        public TypeReference ReturnType
        {
            get
            {
                if (returnType == null)
                    returnType = FromRoslyn.TypeReference(GetReturnType(syntax), this);

                return returnType;
            }
            set => SetNotNull(ref returnType, value);
        }

        public OperatorKind Kind { get; set; }

        private static SyntaxKind GetTokenKind(OperatorKind kind) => operatorKindMapping[kind];

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newModifiers = Modifiers;
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? GetReturnType(syntax);
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newBody = bodySet ? body?.GetWrapped(ref thisChanged) : syntax.Body;

            if (syntax == null || AttributesChanged() || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || !IsAnnotated(syntax))
            {
                BaseMethodDeclarationSyntax newSyntax;

                var token = CSharpSyntaxFactory.Token(GetTokenKind(Kind));

                if (Kind == Implicit || Kind == Explicit)
                {
                    newSyntax = CSharpSyntaxFactory.ConversionOperatorDeclaration(
                        GetNewAttributes(), newModifiers.GetWrapped(), token, newReturnType,
                        CSharpSyntaxFactory.ParameterList(newParameters), newBody, null);
                }
                else
                {
                    newSyntax = CSharpSyntaxFactory.OperatorDeclaration(
                        GetNewAttributes(), newModifiers.GetWrapped(), newReturnType, token,
                        CSharpSyntaxFactory.ParameterList(newParameters), newBody, null);
                }

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((BaseMethodDeclarationSyntax)newSyntax);
            ResetAttributes();

            Set(ref returnType, null);
            parameters = null;
            body = null;
        }

        internal override SyntaxNode Clone()
        {
            throw new NotImplementedException();
        }
    }
}