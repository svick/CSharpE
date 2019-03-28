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
                case AssignmentExpressionSyntax assignment:
                    return new AssignmentExpression(assignment, parent);
                case AwaitExpressionSyntax await:
                    return new AwaitExpression(await, parent);
                case IdentifierNameSyntax identifierName:
                    return new IdentifierExpression(identifierName, parent);
                case InvocationExpressionSyntax invocation:
                    return new InvocationExpression(invocation, parent);
                case LiteralExpressionSyntax literal:
                    return LiteralExpression(literal, parent);
                case MemberAccessExpressionSyntax memberAccess:
                    return new MemberAccessExpression(memberAccess, parent);
                case ObjectCreationExpressionSyntax objectCreation:
                    return new NewExpression(objectCreation, parent);
                case ThisExpressionSyntax @this:
                    return new ThisExpression(@this, parent);
                case TupleExpressionSyntax tuple:
                    return new TupleExpression(tuple, parent);
                case null:
                    return null;
            }

            throw new NotImplementedException(syntax.GetType().Name);
        }

        public static LiteralExpression LiteralExpression(LiteralExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.NullLiteralExpression:
                    return new NullExpression(syntax, parent);
                case SyntaxKind.NumericLiteralExpression:
                    if (syntax.Token.Value is int)
                        return new IntLiteralExpression(syntax, parent);
                    break;
            }

            throw new NotImplementedException();
        }

        public static Statement Statement(StatementSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case null:
                    return null;
                case ExpressionStatementSyntax expressionStatement:
                    return new ExpressionStatement(expressionStatement, parent);
                case ReturnStatementSyntax returnStatement:
                    return new ReturnStatement(returnStatement, parent);
            }

            throw new NotImplementedException();
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
                case ArrayTypeSyntax array:
                    return new ArrayTypeReference(array, parent);
                case NameSyntax _:
                case PredefinedTypeSyntax _:
                    return new NamedTypeReference(typeSyntax, parent);
                default:
                    throw new NotImplementedException(typeSyntax.GetType().Name);
            }
        }
    }
}
