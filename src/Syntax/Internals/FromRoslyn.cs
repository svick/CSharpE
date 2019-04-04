using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpE.Syntax.Internals
{
    /// <summary>
    /// Converts values from Roslyn's object model to CSharpE's object model.
    /// </summary>
    /// <remarks>
    /// Many CSharpE types have constructors that take Roslyn types, those might not have methods in this type.
    /// </remarks>
    internal static class FromRoslyn
    {
        public static Expression Expression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreation:
                    return new AnonymousNewExpression(anonymousObjectCreation, parent);
                case AssignmentExpressionSyntax assignment:
                    return AssignmentExpression(assignment, parent);
                case AwaitExpressionSyntax await:
                    return new AwaitExpression(await, parent);
                case BaseExpressionSyntax @base:
                    return new BaseExpression(@base, parent);
                case BinaryExpressionSyntax binary:
                    return BinaryExpression(binary, parent);
                case CastExpressionSyntax cast:
                    return new CastExpression(cast, parent);
                case ConditionalAccessExpressionSyntax conditionalAccess:
                    return ConditionalAccessExpression(conditionalAccess, parent);
                case ConditionalExpressionSyntax conditional:
                    return new ConditionalExpression(conditional, parent);
                case CheckedExpressionSyntax @checked:
                    return new CheckedExpression(@checked, parent);
                case DeclarationExpressionSyntax declaration:
                    return new DeclarationExpression(declaration, parent);
                case DefaultExpressionSyntax @default:
                    return new DefaultExpression(@default, parent);
                case ElementAccessExpressionSyntax elementAccess:
                    return new ElementAccessExpression(elementAccess, parent);
                case IdentifierNameSyntax identifierName:
                    return new IdentifierExpression(identifierName, parent);
                case InvocationExpressionSyntax invocation:
                    return new InvocationExpression(invocation, parent);
                case LiteralExpressionSyntax literal:
                    return LiteralExpression(literal, parent);
                case MemberAccessExpressionSyntax memberAccess:
                    return MemberAccessExpression(memberAccess, parent);
                case ObjectCreationExpressionSyntax objectCreation:
                    return new NewExpression(objectCreation, parent);
                case ParenthesizedExpressionSyntax parenthesized:
                    return new ParenthesizedExpression(parenthesized, parent);
                case PostfixUnaryExpressionSyntax postfixUnary:
                    return PostfixUnaryExpression(postfixUnary, parent);
                case PredefinedTypeSyntax predefinedType:
                    return new NamedTypeReference(predefinedType, parent);
                case PrefixUnaryExpressionSyntax prefixUnary:
                    return PrefixUnaryExpression(prefixUnary, parent);
                case RefExpressionSyntax @ref:
                    return new RefExpression(@ref, parent);
                case SizeOfExpressionSyntax sizeOf:
                    return new SizeOfExpression(sizeOf, parent);
                case ThisExpressionSyntax @this:
                    return new ThisExpression(@this, parent);
                case TupleExpressionSyntax tuple:
                    return new TupleExpression(tuple, parent);
                case TypeOfExpressionSyntax typeOf:
                    return new TypeOfExpression(typeOf, parent);
                case null:
                    return null;
            }

            throw new NotImplementedException(syntax.GetType().Name);
        }

        private static BinaryExpression AssignmentExpression(AssignmentExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.SimpleAssignmentExpression:
                    return new AssignmentExpression(syntax, parent);
                case SyntaxKind.AddAssignmentExpression:
                    return new AddAssignmentExpression(syntax, parent);
                case SyntaxKind.SubtractAssignmentExpression:
                    return new SubtractAssignmentExpression(syntax, parent);
                case SyntaxKind.MultiplyAssignmentExpression:
                    return new MultiplyAssignmentExpression(syntax, parent);
                case SyntaxKind.DivideAssignmentExpression:
                    return new DivideAssignmentExpression(syntax, parent);
                case SyntaxKind.ModuloAssignmentExpression:
                    return new ModuloAssignmentExpression(syntax, parent);
                case SyntaxKind.AndAssignmentExpression:
                    return new AndAssignmentExpression(syntax, parent);
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                    return new XorAssignmentExpression(syntax, parent);
                case SyntaxKind.OrAssignmentExpression:
                    return new OrAssignmentExpression(syntax, parent);
                case SyntaxKind.LeftShiftAssignmentExpression:
                    return new LeftShiftAssignmentExpression(syntax, parent);
                case SyntaxKind.RightShiftAssignmentExpression:
                    return new RightShiftAssignmentExpression(syntax, parent);
            }
            throw new InvalidOperationException();
        }

        private static BinaryExpression BinaryExpression(BinaryExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.AddExpression:
                    return new AddExpression(syntax, parent);
                case SyntaxKind.SubtractExpression:
                    return new SubtractExpression(syntax, parent);
                case SyntaxKind.MultiplyExpression:
                    return new MultiplyExpression(syntax, parent);
                case SyntaxKind.DivideExpression:
                    return new DivideExpression(syntax, parent);
                case SyntaxKind.ModuloExpression:
                    return new ModuloExpression(syntax, parent);
                case SyntaxKind.LeftShiftExpression:
                    return new LeftShiftExpression(syntax, parent);
                case SyntaxKind.RightShiftExpression:
                    return new RightShiftExpression(syntax, parent);
                case SyntaxKind.LogicalOrExpression:
                    return new LogicalOrExpression(syntax, parent);
                case SyntaxKind.LogicalAndExpression:
                    return new LogicalAndExpression(syntax, parent);
                case SyntaxKind.BitwiseOrExpression:
                    return new BitwiseOrExpression(syntax, parent);
                case SyntaxKind.BitwiseAndExpression:
                    return new BitwiseAndExpression(syntax, parent);
                case SyntaxKind.ExclusiveOrExpression:
                    return new XorExpression(syntax, parent);
                case SyntaxKind.EqualsExpression:
                    return new EqualsExpression(syntax, parent);
                case SyntaxKind.NotEqualsExpression:
                    return new NotEqualsExpression(syntax, parent);
                case SyntaxKind.LessThanExpression:
                    return new LessThanExpression(syntax, parent);
                case SyntaxKind.LessThanOrEqualExpression:
                    return new LessThanOrEqualExpression(syntax, parent);
                case SyntaxKind.GreaterThanExpression:
                    return new GreaterThanExpression(syntax, parent);
                case SyntaxKind.GreaterThanOrEqualExpression:
                    return new GreaterThanOrEqualExpression(syntax, parent);
                case SyntaxKind.IsExpression:
                    return new IsExpression(syntax, parent);
                case SyntaxKind.AsExpression:
                    return new AsExpression(syntax, parent);
                case SyntaxKind.CoalesceExpression:
                    return new CoalesceExpression(syntax, parent);
            }

            throw new InvalidOperationException();
        }

        private static Expression ConditionalAccessExpression(
            ConditionalAccessExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.WhenNotNull)
            {
                case MemberBindingExpressionSyntax _:
                    return new ConditionalMemberAccessExpression(syntax, parent);
                case ElementBindingExpressionSyntax _:
                    return new ConditionalElementAccessExpression(syntax, parent);
            }

            throw new InvalidOperationException();
        }

        private static BaseMemberAccessExpression MemberAccessExpression(
            MemberAccessExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    return new MemberAccessExpression(syntax, parent);
                case SyntaxKind.PointerMemberAccessExpression:
                    return new PointerMemberAccessExpression(syntax, parent);
            }

            throw new InvalidOperationException();
        }

        private static UnaryExpression PrefixUnaryExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.UnaryPlusExpression:
                    return new UnaryPlusExpression(syntax, parent);
                case SyntaxKind.UnaryMinusExpression:
                    return new UnaryMinusExpression(syntax, parent);
                case SyntaxKind.BitwiseNotExpression:
                    return new ComplementExpression(syntax, parent);
                case SyntaxKind.LogicalNotExpression:
                    return new NegateExpression(syntax, parent);
                case SyntaxKind.PreIncrementExpression:
                    return new PreIncrementExpression(syntax, parent);
                case SyntaxKind.PreDecrementExpression:
                    return new PreDecrementExpression(syntax, parent);
                case SyntaxKind.AddressOfExpression:
                    return new AddressOfExpression(syntax, parent);
                case SyntaxKind.PointerIndirectionExpression:
                    return new PointerIndirectionExpression(syntax, parent);
            }
            throw new InvalidOperationException();
        }

        private static UnaryExpression PostfixUnaryExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.PostIncrementExpression:
                    return new PostIncrementExpression(syntax, parent);
                case SyntaxKind.PostDecrementExpression:
                    return new PostDecrementExpression(syntax, parent);
            }
            throw new InvalidOperationException();
        }

        public static Expression LiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                    return new NullExpression(syntax, parent);
                case SyntaxKind.DefaultLiteralExpression:
                    return new DefaultExpression(syntax, parent);
                case SyntaxKind.NumericLiteralExpression:
                    if (syntax.Token.Value is int)
                        return new IntLiteralExpression(syntax, parent);
                    break;
                case SyntaxKind.TrueLiteralExpression:
                    return new BoolLiteralExpression(syntax, parent);
                case SyntaxKind.FalseLiteralExpression:
                    return new BoolLiteralExpression(syntax, parent);
            }

            throw new NotImplementedException(syntax.Kind().ToString());
        }

        public static Initializer Initializer(InitializerExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax?.Kind())
            {
                case null:
                    return null;
                case SyntaxKind.ObjectInitializerExpression:
                    return new ObjectInitializer(syntax, parent);
                case SyntaxKind.CollectionInitializerExpression:
                    return new CollectionInitializer(syntax, parent);
            }

            throw new NotImplementedException(syntax.Kind().ToString());
        }

        public static VariableDesignation VariableDesignation(VariableDesignationSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case DiscardDesignationSyntax _:
                case SingleVariableDesignationSyntax _:
                    return new SingleVariableDesignation(syntax, parent);
                case ParenthesizedVariableDesignationSyntax parenthesized:
                    return new MultiVariableDesignation(parenthesized, parent);
            }

            throw new InvalidOperationException();
        }

        public static Statement Statement(StatementSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case null:
                    return null;
                case CheckedStatementSyntax @checked:
                    return new CheckedStatement(@checked, parent);
                case ExpressionStatementSyntax expression:
                    return new ExpressionStatement(expression, parent);
                case ReturnStatementSyntax @return:
                    return new ReturnStatement(@return, parent);
            }

            throw new NotImplementedException(syntax.Kind().ToString());
        }

        public static MemberModifiers MemberModifiers(SyntaxTokenList modifiers)
        {
            MemberModifiers result = 0;

            foreach (var modifier in modifiers)
            {
                result |= MemberModifiersExtensions.ModifiersMapping[modifier.Kind()];
            }

            return result;
        }

        public static MemberDefinition MemberDefinition(MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType)
        {
            switch (memberDeclarationSyntax)
            {
                case FieldDeclarationSyntax fieldDeclaration:
                    return new FieldDefinition(fieldDeclaration, containingType);
                case PropertyDeclarationSyntax propertyDeclaration:
                    return new PropertyDefinition(propertyDeclaration, containingType);
                case MethodDeclarationSyntax methodDeclaration:
                    return new MethodDefinition(methodDeclaration, containingType);
                case ConstructorDeclarationSyntax constructorDeclaration:
                    return new ConstructorDefinition(constructorDeclaration, containingType);
                case DestructorDeclarationSyntax destructorDeclaration:
                    return new FinalizerDefinition(destructorDeclaration, containingType);
                case OperatorDeclarationSyntax operatorDeclaration:
                    return new OperatorDefinition(operatorDeclaration, containingType);
                case ConversionOperatorDeclarationSyntax conversionOperatorDeclaration:
                    return new OperatorDefinition(conversionOperatorDeclaration, containingType);
                case BaseTypeDeclarationSyntax baseTypeDeclaration:
                    return TypeDefinition(baseTypeDeclaration, containingType);
                case DelegateDeclarationSyntax delegateDeclaration:
                    return new DelegateDefinition(delegateDeclaration, containingType);
                case IncompleteMemberSyntax incompleteMember:
                    return new IncompleteMemberDefinition(incompleteMember, containingType);
                default:
                    throw new NotImplementedException(memberDeclarationSyntax.GetType().Name);
            }
        }

        public static BaseTypeDefinition TypeDefinition(MemberDeclarationSyntax memberDeclarationSyntax, SyntaxNode parent)
        {
            switch (memberDeclarationSyntax)
            {
                case DelegateDeclarationSyntax delegateDeclaration:
                    return new DelegateDefinition(delegateDeclaration, parent);
                case BaseTypeDeclarationSyntax baseTypeDeclaration:
                    return TypeDefinition(baseTypeDeclaration, parent);
                case IncompleteMemberSyntax _:
                case FieldDeclarationSyntax _:
                case MethodDeclarationSyntax _:
                    return new InvalidTypeDefinition(memberDeclarationSyntax, parent);
            }
            throw new InvalidOperationException();
        }

        public static BaseTypeDefinition TypeDefinition(BaseTypeDeclarationSyntax typeDeclarationSyntax, SyntaxNode parent)
        {
            switch (typeDeclarationSyntax)
            {
                case ClassDeclarationSyntax classDeclaration:
                    return new ClassDefinition(classDeclaration, parent);
                case StructDeclarationSyntax structDeclaration:
                    return new StructDefinition(structDeclaration, parent);
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    return new InterfaceDefinition(interfaceDeclaration, parent);
                case EnumDeclarationSyntax enumDeclaration:
                    return new EnumDefinition(enumDeclaration, parent);
            }
            throw new InvalidOperationException();
        }

        public static TypeReference TypeReference(TypeSyntax typeSyntax, SyntaxNode parent)
        {
            switch (typeSyntax)
            {
                case NameSyntax _:
                case PredefinedTypeSyntax _:
                    return new NamedTypeReference(typeSyntax, parent);
                case ArrayTypeSyntax array:
                    return new ArrayTypeReference(array, parent);
                case NullableTypeSyntax nullable:
                    return new NullableTypeReference(nullable, parent);
                case PointerTypeSyntax pointer:
                    return new PointerTypeReference(pointer, parent);
                case RefTypeSyntax @ref:
                    return new RefTypeReference(@ref, parent);
                case TupleTypeSyntax tuple:
                    return new TupleTypeReference(tuple, parent);
                default:
                    throw new NotImplementedException(typeSyntax.GetType().Name);
            }
        }
    }
}
