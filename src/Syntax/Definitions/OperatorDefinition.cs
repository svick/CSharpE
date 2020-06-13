using System;
using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.MemberModifiers;
using static CSharpE.Syntax.OperatorKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public enum OperatorKind
    {
        Plus,
        Add = Plus,
        Minus,
        Subtract = Minus,
        Negate,
        Complement,
        Increment,
        Decrement,
        True,
        False,
        Multiply,
        Divide,
        Modulo,
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
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private static readonly BiDirectionalDictionary<OperatorKind, SyntaxKind> OperatorKindMapping =
            new BiDirectionalDictionary<OperatorKind, SyntaxKind>
            {
                { Plus, PlusToken },
                { Minus, MinusToken },
                { Negate, ExclamationToken },
                { Complement, TildeToken },
                { Increment, PlusPlusToken },
                { Decrement, MinusMinusToken },
                { True, TrueKeyword },
                { False, FalseKeyword },
                { Multiply, AsteriskToken },
                { Divide, SlashToken },
                { Modulo, PercentToken },
                { And, AmpersandToken },
                { Or, BarToken },
                { Xor, CaretToken },
                { LeftShift, LessThanLessThanToken },
                { RightShift, GreaterThanGreaterThanToken },
                { OperatorKind.Equals, EqualsEqualsToken },
                { NotEquals, ExclamationEqualsToken },
                { GreaterThan, GreaterThanToken },
                { LessThan, LessThanToken },
                { GreaterThanOrEquals, GreaterThanEqualsToken },
                { LessThanOrEquals, LessThanEqualsToken },
                { Implicit, ImplicitKeyword },
                { Explicit, ExplicitKeyword }
            };

        private static OperatorKind GetKind(BaseMethodDeclarationSyntax syntax)
        {
            switch (syntax)
            {
                case OperatorDeclarationSyntax operatorDeclaration:
                    return OperatorKindMapping[operatorDeclaration.OperatorToken.Kind()];
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    return OperatorKindMapping[conversionOperatorDeclaration.ImplicitOrExplicitKeyword.Kind()];
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
            BlockStatement statementBody)
            : this(modifiers, returnType, kind, parameters, statementBody, null) { }

        public OperatorDefinition(
            MemberModifiers modifiers, TypeReference returnType, OperatorKind kind, IEnumerable<Parameter> parameters,
            Expression expressionBody)
            : this(modifiers, returnType, kind, parameters, null, expressionBody) { }

        private OperatorDefinition(
            MemberModifiers modifiers, TypeReference returnType, OperatorKind kind, IEnumerable<Parameter> parameters,
            BlockStatement statementBody, Expression expressionBody)
        {
            Modifiers = modifiers;
            ReturnType = returnType;
            Kind = kind;
            Parameters = parameters?.ToList();
            StatementBody = statementBody;
            ExpressionBody = expressionBody;
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

        private static SyntaxKind GetTokenKind(OperatorKind kind) => OperatorKindMapping[kind];

        private protected override BaseMethodDeclarationSyntax GetWrappedBaseMethod(ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newAttributes = attributes?.GetWrapped(ref thisChanged) ?? syntax?.AttributeLists ?? default;
            var newModifiers = Modifiers;
            var newReturnType = returnType?.GetWrapped(ref thisChanged) ?? GetReturnType(syntax);
            var newParameters = parameters?.GetWrapped(ref thisChanged) ?? syntax.ParameterList.Parameters;
            var newStatementBody = statementBodySet ? statementBody?.GetWrapped(ref thisChanged) : syntax.Body;
            var newExpressionBody = expressionBodySet ? expressionBody?.GetWrapped(ref thisChanged) : syntax.ExpressionBody?.Expression;

            if (syntax == null || newModifiers != FromRoslyn.MemberModifiers(syntax.Modifiers) ||
                thisChanged == true || ShouldAnnotate(syntax, changed))
            {
                var arrowClause = newExpressionBody == null ? null : RoslynSyntaxFactory.ArrowExpressionClause(newExpressionBody);
                var semicolonToken = newStatementBody == null ? RoslynSyntaxFactory.Token(SyntaxKind.SemicolonToken) : default;

                BaseMethodDeclarationSyntax newSyntax;

                var token = RoslynSyntaxFactory.Token(GetTokenKind(Kind));

                if (Kind == Implicit || Kind == Explicit)
                {
                    newSyntax = RoslynSyntaxFactory.ConversionOperatorDeclaration(
                        newAttributes, newModifiers.GetWrapped(), token, newReturnType,
                        RoslynSyntaxFactory.ParameterList(newParameters), newStatementBody, arrowClause)
                        .WithSemicolonToken(semicolonToken);
                }
                else
                {
                    newSyntax = RoslynSyntaxFactory.OperatorDeclaration(
                        newAttributes, newModifiers.GetWrapped(), newReturnType, token,
                        RoslynSyntaxFactory.ParameterList(newParameters), newStatementBody, arrowClause)
                        .WithSemicolonToken(semicolonToken);
                }

                syntax = Annotate(newSyntax);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((BaseMethodDeclarationSyntax)newSyntax);

            SetList(ref attributes, null);
            Set(ref returnType, null);
            SetList(ref parameters, null);
            Set(ref statementBody, null);
            statementBodySet = false;
            Set(ref expressionBody, null);
            expressionBodySet = false;
        }

        private protected override SyntaxNode CloneImpl() =>
            new OperatorDefinition(Modifiers, ReturnType, Kind, Parameters, StatementBody, ExpressionBody)
            {
                Attributes = Attributes
            };
    }
}