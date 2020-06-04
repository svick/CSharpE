﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
        private static bool HasUnresolvedErrors(Roslyn::SyntaxNode node, IEnumerable<Roslyn::SyntaxNode> children)
        {
            var errors = node.GetDiagnostics().Where(d => d.DefaultSeverity == DiagnosticSeverity.Error);

            return errors.Except(children.SelectMany(c => c?.GetDiagnostics() ?? Array.Empty<Diagnostic>())).Any();
        }

        private static IEnumerable<Roslyn::SyntaxNode> ChildExpressionsAndStatements(Roslyn::SyntaxNode syntax)
        {
            foreach (var childNode in syntax.ChildNodes())
            {
                if (childNode is ExpressionSyntax expression)
                {
                    yield return expression;
                }
                else if (childNode is StatementSyntax statement)
                {
                    yield return statement;
                }
                else
                {
                    foreach (var childExpression in ChildExpressionsAndStatements(childNode))
                    {
                        yield return childExpression;
                    }
                }
            }
        }

        private static IEnumerable<Roslyn::SyntaxNode> ExpressionChildren(ExpressionSyntax syntax) =>
            ChildExpressionsAndStatements(syntax);

        public static Expression Expression(ExpressionSyntax syntax, SyntaxNode parent)
        {
            if (syntax == null)
                return null;

            if (HasUnresolvedErrors(syntax, ExpressionChildren(syntax)))
                return new InvalidExpression(syntax, parent);

            switch (syntax)
            {
                case AnonymousMethodExpressionSyntax anonymousMethod:
                    return new DelegateExpression(anonymousMethod, parent);
                case AnonymousObjectCreationExpressionSyntax anonymousObjectCreation:
                    return new AnonymousNewExpression(anonymousObjectCreation, parent);
                case ArrayCreationExpressionSyntax arrayCreation:
                    return new NewArrayExpression(arrayCreation, parent);
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
                case ImplicitArrayCreationExpressionSyntax implicitArrayCreation:
                    return new ImplicitNewArrayExpression(implicitArrayCreation, parent);
                case ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreation:
                    return new ImplicitStackAllocExpression(implicitStackAllocArrayCreation, parent);
                case InterpolatedStringExpressionSyntax interpolatedStringExpression:
                    return new InterpolatedStringExpression(interpolatedStringExpression, parent);
                case InvocationExpressionSyntax invocation:
                    return new InvocationExpression(invocation, parent);
                case IsPatternExpressionSyntax isPattern:
                    return new IsPatternExpression(isPattern, parent);
                case LambdaExpressionSyntax lambda:
                    return new LambdaExpression(lambda, parent);
                case LiteralExpressionSyntax literal:
                    return LiteralExpression(literal, parent);
                case MemberAccessExpressionSyntax memberAccess:
                    return MemberAccessExpression(memberAccess, parent);
                case ObjectCreationExpressionSyntax objectCreation:
                    return new NewExpression(objectCreation, parent);
                case OmittedArraySizeExpressionSyntax _:
                    return null;
                case ParenthesizedExpressionSyntax parenthesized:
                    return new ParenthesizedExpression(parenthesized, parent);
                case PostfixUnaryExpressionSyntax postfixUnary:
                    return PostfixUnaryExpression(postfixUnary, parent);
                case PredefinedTypeSyntax predefinedType:
                    return new NamedTypeReference(predefinedType, parent);
                case PrefixUnaryExpressionSyntax prefixUnary:
                    return PrefixUnaryExpression(prefixUnary, parent);
                case QueryExpressionSyntax query:
                    return new LinqExpression(query, parent);
                case RangeExpressionSyntax range:
                    return new RangeExpression(range, parent);
                case RefExpressionSyntax @ref:
                    return new RefExpression(@ref, parent);
                case SizeOfExpressionSyntax sizeOf:
                    return new SizeOfExpression(sizeOf, parent);
                case StackAllocArrayCreationExpressionSyntax stackAllocArrayCreation:
                    return new StackAllocExpression(stackAllocArrayCreation, parent);
                case ThisExpressionSyntax @this:
                    return new ThisExpression(@this, parent);
                case ThrowExpressionSyntax @throw:
                    return new ThrowExpression(@throw, parent);
                case TupleExpressionSyntax tuple:
                    return new TupleExpression(tuple, parent);
                case TypeOfExpressionSyntax typeOf:
                    return new TypeOfExpression(typeOf, parent);
            }

            throw new NotImplementedException(syntax.GetType().Name);
        }

        private static BinaryExpression AssignmentExpression(AssignmentExpressionSyntax syntax, SyntaxNode parent) =>
            (syntax.Kind()) switch
            {
                SyntaxKind.SimpleAssignmentExpression => new AssignmentExpression(syntax, parent),
                SyntaxKind.AddAssignmentExpression => new AddAssignmentExpression(syntax, parent),
                SyntaxKind.SubtractAssignmentExpression => new SubtractAssignmentExpression(syntax, parent),
                SyntaxKind.MultiplyAssignmentExpression => new MultiplyAssignmentExpression(syntax, parent),
                SyntaxKind.DivideAssignmentExpression => new DivideAssignmentExpression(syntax, parent),
                SyntaxKind.ModuloAssignmentExpression => new ModuloAssignmentExpression(syntax, parent),
                SyntaxKind.AndAssignmentExpression => new AndAssignmentExpression(syntax, parent),
                SyntaxKind.ExclusiveOrAssignmentExpression => new XorAssignmentExpression(syntax, parent),
                SyntaxKind.OrAssignmentExpression => new OrAssignmentExpression(syntax, parent),
                SyntaxKind.LeftShiftAssignmentExpression => new LeftShiftAssignmentExpression(syntax, parent),
                SyntaxKind.RightShiftAssignmentExpression => new RightShiftAssignmentExpression(syntax, parent),
                SyntaxKind.CoalesceAssignmentExpression => new CoalesceAssignmentExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

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

        private static UnaryExpression PrefixUnaryExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent) =>
            (syntax.Kind()) switch
            {
                SyntaxKind.UnaryPlusExpression => new UnaryPlusExpression(syntax, parent),
                SyntaxKind.UnaryMinusExpression => new UnaryMinusExpression(syntax, parent),
                SyntaxKind.BitwiseNotExpression => new ComplementExpression(syntax, parent),
                SyntaxKind.LogicalNotExpression => new NegateExpression(syntax, parent),
                SyntaxKind.PreIncrementExpression => new PreIncrementExpression(syntax, parent),
                SyntaxKind.PreDecrementExpression => new PreDecrementExpression(syntax, parent),
                SyntaxKind.AddressOfExpression => new AddressOfExpression(syntax, parent),
                SyntaxKind.PointerIndirectionExpression => new PointerIndirectionExpression(syntax, parent),
                SyntaxKind.IndexExpression => new IndexExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

        private static UnaryExpression PostfixUnaryExpression(PostfixUnaryExpressionSyntax syntax, SyntaxNode parent) =>
            (syntax.Kind()) switch
            {
                SyntaxKind.PostIncrementExpression => new PostIncrementExpression(syntax, parent),
                SyntaxKind.PostDecrementExpression => new PostDecrementExpression(syntax, parent),
                SyntaxKind.SuppressNullableWarningExpression => new SuppressNullableWarningExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

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
                case SyntaxKind.CharacterLiteralExpression:
                    return new CharLiteralExpression(syntax, parent);
                case SyntaxKind.StringLiteralExpression:
                    return new StringLiteralExpression(syntax, parent);
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

            throw new InvalidOperationException();
        }

        public static VariableInitializer VariableInitializer(ExpressionSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case InitializerExpressionSyntax initializer:
                    return new ArrayInitializer(initializer, parent);
                default:
                    return new ExpressionVariableInitializer(syntax, parent);
            }
        }

        public static VariableDesignation VariableDesignation(VariableDesignationSyntax syntax, SyntaxNode parent)
        {
            switch (syntax)
            {
                case null:
                    return null;
                case DiscardDesignationSyntax _:
                case SingleVariableDesignationSyntax _:
                    return new SingleVariableDesignation(syntax, parent);
                case ParenthesizedVariableDesignationSyntax parenthesized:
                    return new MultiVariableDesignation(parenthesized, parent);
            }

            throw new InvalidOperationException(syntax.GetType().Name);
        }

        private static IEnumerable<Roslyn::SyntaxNode> StatementChildren(StatementSyntax syntax) =>
            ChildExpressionsAndStatements(syntax);

        public static Statement Statement(StatementSyntax syntax, SyntaxNode parent)
        {
            if (syntax == null)
                return null;

            if (HasUnresolvedErrors(syntax, StatementChildren(syntax)))
                return new InvalidStatement(syntax, parent);

            switch (syntax)
            {
                case BlockSyntax block:
                    return new BlockStatement(block, parent);
                case BreakStatementSyntax @break:
                    return new BreakStatement(@break, parent);
                case CheckedStatementSyntax @checked:
                    return new CheckedStatement(@checked, parent);
                case ContinueStatementSyntax @continue:
                    return new ContinueStatement(@continue, parent);
                case DoStatementSyntax @do:
                    return new DoWhileStatement(@do, parent);
                case EmptyStatementSyntax empty:
                    return new EmptyStatement(empty, parent);
                case ExpressionStatementSyntax expression:
                    return new ExpressionStatement(expression, parent);
                case FixedStatementSyntax @fixed:
                    return new FixedStatement(@fixed, parent);
                case ForEachStatementSyntax forEach:
                    return new ForEachStatement(forEach, parent);
                case ForEachVariableStatementSyntax forEachVariable:
                    return new ForEachPatternStatement(forEachVariable, parent);
                case ForStatementSyntax @for:
                    return new ForStatement(@for, parent);
                case GotoStatementSyntax @goto:
                    return GotoStatement(@goto, parent);
                case IfStatementSyntax @if:
                    return new IfStatement(@if, parent);
                case LabeledStatementSyntax labeled:
                    return new LabelStatement(labeled, parent);
                case LocalFunctionStatementSyntax localFunction:
                    return new LocalFunctionStatement(localFunction, parent);
                case LocalDeclarationStatementSyntax localDeclaration:
                    return new VariableDeclarationStatement(localDeclaration, parent);
                case LockStatementSyntax @lock:
                    return new LockStatement(@lock, parent);
                case ReturnStatementSyntax @return:
                    return new ReturnStatement(@return, parent);
                case SwitchStatementSyntax @switch:
                    return new SwitchStatement(@switch, parent);
                case ThrowStatementSyntax @throw:
                    return new ExpressionStatement(@throw, parent);
                case TryStatementSyntax @try:
                    return new TryStatement(@try, parent);
                case UnsafeStatementSyntax @unsafe:
                    return new UnsafeStatement(@unsafe, parent);
                case UsingStatementSyntax @using:
                    return new UsingStatement(@using, parent);
                case WhileStatementSyntax @while:
                    return new WhileStatement(@while, parent);
                case YieldStatementSyntax yield:
                    if (yield.Kind() == SyntaxKind.YieldBreakStatement)
                        return new YieldBreakStatement(yield, parent);
                    else
                        return new YieldReturnStatement(yield, parent);
            }

            throw new NotImplementedException(syntax.Kind().ToString());
        }

        private static Statement GotoStatement(GotoStatementSyntax syntax, SyntaxNode parent)
        {
            switch (syntax.Kind())
            {
                case SyntaxKind.GotoStatement:
                    return new GotoStatement(syntax, parent);
                case SyntaxKind.GotoCaseStatement:
                    return new GotoCaseStatement(syntax, parent);
                case SyntaxKind.GotoDefaultStatement:
                    return new GotoDefaultStatement(syntax, parent);
            }

            throw new InvalidOperationException();
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

        public static ParameterModifiers ParameterModifiers(SyntaxTokenList modifiers)
        {
            ParameterModifiers result = 0;

            foreach (var modifier in modifiers)
            {
                result |= ParameterModifiersExtensions.ModifiersMapping[modifier.Kind()];
            }

            return result;
        }

        public static bool IsCompacted(Roslyn::SyntaxNode syntaxNode) => syntaxNode switch
        {
            BaseFieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables.Count > 1,
            AttributeListSyntax attributeList => attributeList.Attributes.Count > 1,
            LocalDeclarationStatementSyntax localDeclaration => localDeclaration.Declaration.Variables.Count > 1,
            ForStatementSyntax forStatement => forStatement.Initializers != default || forStatement.Declaration?.Variables.Count > 1,
            _ => false,
        };

        public static IEnumerable<TRoslynSyntax> Expand<TRoslynSyntax>(TRoslynSyntax roslynSyntax)
            where TRoslynSyntax : Roslyn::SyntaxNode
        {
            TRoslynSyntax Cast(Roslyn::SyntaxNode syntax) => (TRoslynSyntax)syntax;

            IEnumerable<VariableDeclarationSyntax> ExpandVariable(VariableDeclarationSyntax declaration)
            {
                foreach (var variable in declaration.Variables)
                {
                    yield return declaration.WithVariables(RoslynSyntaxFactory.SingletonSeparatedList(variable));
                }
            }

            switch (roslynSyntax)
            {
                case BaseFieldDeclarationSyntax fieldDeclaration:
                    foreach (var variable in fieldDeclaration.Declaration.Variables)
                    {
                        var expandedSyntax = fieldDeclaration.WithDeclaration(
                            fieldDeclaration.Declaration.WithVariables(
                                RoslynSyntaxFactory.SingletonSeparatedList(variable)));

                        yield return Cast(expandedSyntax);
                    }
                    break;

                case AttributeListSyntax attributeList:
                    foreach (var attribute in attributeList.Attributes)
                    {
                        var expandedSyntax =
                            attributeList.WithAttributes(RoslynSyntaxFactory.SingletonSeparatedList(attribute));

                        yield return Cast(expandedSyntax);
                    }
                    break;

                case LocalDeclarationStatementSyntax localDeclaration:
                    foreach (var variable in ExpandVariable(localDeclaration.Declaration))
                    {
                        var expandedSyntax = localDeclaration.WithDeclaration(variable);

                        yield return Cast(expandedSyntax);
                    }
                    break;

                case ForStatementSyntax forStatement:
                    if (forStatement.Initializers != default)
                    {
                        foreach (var initializer in forStatement.Initializers)
                        {
                            yield return Cast(RoslynSyntaxFactory.ExpressionStatement(initializer));
                        }

                        yield return Cast(forStatement.WithInitializers(default));
                    }
                    else
                    {
                        foreach (var variable in ExpandVariable(forStatement.Declaration))
                        {
                            yield return Cast(RoslynSyntaxFactory.LocalDeclarationStatement(variable));
                        }

                        yield return Cast(forStatement.WithDeclaration(default));
                    }
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private static IEnumerable<ExpressionSyntax> MemberAttributeExpressions(
            MemberDeclarationSyntax memberDeclarationSyntax) =>
            Syntax.MemberDefinition.GetAttributeLists(memberDeclarationSyntax)
                .SelectMany(al => al.Attributes)
                .SelectMany(a => a.ArgumentList?.Arguments ?? default)
                .Select(a => a.Expression);

        private static IEnumerable<Roslyn::SyntaxNode> MemberDeclarationChildren(MemberDeclarationSyntax memberDeclarationSyntax)
        {
            return MemberAttributeExpressions(memberDeclarationSyntax).Concat(GetChildrenImpl());

            IEnumerable<Roslyn::SyntaxNode> GetChildrenImpl()
            {
                switch (memberDeclarationSyntax)
                {
                    case FieldDeclarationSyntax field:
                        return new[] { field.Declaration.Variables.Single().Initializer };
                    case BasePropertyDeclarationSyntax property:
                        return property.AccessorList.Accessors;
                    case EventFieldDeclarationSyntax _:
                        return Array.Empty<Roslyn::SyntaxNode>();
                    case BaseMethodDeclarationSyntax method:
                        return new[] { method.Body };
                    case BaseTypeDeclarationSyntax baseType:
                        return TypeDefinitionChildren(baseType);
                    case DelegateDeclarationSyntax _:
                    case IncompleteMemberSyntax _:
                        return Array.Empty<Roslyn::SyntaxNode>();
                    default:
                        throw new NotImplementedException(memberDeclarationSyntax.GetType().Name);
                }
            }
        }

        public static MemberDefinition MemberDefinition(
            MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType)
        {
            if (HasUnresolvedErrors(memberDeclarationSyntax, MemberDeclarationChildren(memberDeclarationSyntax)))
                return new InvalidMemberDefinition(memberDeclarationSyntax, containingType);

            switch (memberDeclarationSyntax)
            {
                case FieldDeclarationSyntax field:
                    return new FieldDefinition(field, containingType);
                case PropertyDeclarationSyntax property:
                    return new PropertyDefinition(property, containingType);
                case IndexerDeclarationSyntax indexer:
                    return new IndexerDefinition(indexer, containingType);
                case EventDeclarationSyntax @event:
                    return new EventDefinition(@event, containingType);
                case EventFieldDeclarationSyntax eventField:
                    return new EventDefinition(eventField, containingType);
                case MethodDeclarationSyntax method:
                    return new MethodDefinition(method, containingType);
                case ConstructorDeclarationSyntax constructor:
                    return new ConstructorDefinition(constructor, containingType);
                case DestructorDeclarationSyntax destructor:
                    return new FinalizerDefinition(destructor, containingType);
                case OperatorDeclarationSyntax @operator:
                    return new OperatorDefinition(@operator, containingType);
                case ConversionOperatorDeclarationSyntax conversionOperator:
                    return new OperatorDefinition(conversionOperator, containingType);
                case BaseTypeDeclarationSyntax baseType:
                    return TypeDefinition(baseType, containingType);
                case DelegateDeclarationSyntax @delegate:
                    return new DelegateDefinition(@delegate, containingType);
                default:
                    throw new NotImplementedException(memberDeclarationSyntax.GetType().Name);
            }
        }

        public static BaseTypeDefinition TypeDefinition(
            MemberDeclarationSyntax memberDeclarationSyntax, SyntaxNode parent)
        {
            if (HasUnresolvedErrors(memberDeclarationSyntax, MemberDeclarationChildren(memberDeclarationSyntax)))
                return new InvalidMemberDefinition(memberDeclarationSyntax, parent);

            switch (memberDeclarationSyntax)
            {
                case DelegateDeclarationSyntax delegateDeclaration:
                    return new DelegateDefinition(delegateDeclaration, parent);
                case BaseTypeDeclarationSyntax baseTypeDeclaration:
                    return TypeDefinition(baseTypeDeclaration, parent);
            }
            throw new InvalidOperationException();
        }

        private static IEnumerable<Roslyn::SyntaxNode> TypeDefinitionChildren(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            return MemberAttributeExpressions(baseTypeDeclarationSyntax).Concat(GetChildrenImpl());

            IEnumerable<Roslyn::SyntaxNode> GetChildrenImpl()
            {
                switch (baseTypeDeclarationSyntax)
                {
                    case EnumDeclarationSyntax enumDeclaration:
                        return enumDeclaration.Members.Select(m => m.EqualsValue?.Value);
                    case TypeDeclarationSyntax typeDeclaration:
                        return typeDeclaration.Members;
                }
                throw new InvalidOperationException();
            }
        }

        private static BaseTypeDefinition TypeDefinition(BaseTypeDeclarationSyntax typeDeclarationSyntax, SyntaxNode parent)
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
                case null:
                case OmittedTypeArgumentSyntax _:
                    return null;
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

        public static TypeReference TypeReference(ITypeSymbol typeSymbol)
        {
            switch (typeSymbol)
            {
                case null:
                case IErrorTypeSymbol _:
                    return null;
                case INamedTypeSymbol namedType:
                    return new NamedTypeReference(namedType);
                case IArrayTypeSymbol arrayType:
                    return new ArrayTypeReference(arrayType);
                case IPointerTypeSymbol pointerType:
                    return new PointerTypeReference(pointerType);
            }

            throw new NotImplementedException();
        }

        public static InterpolatedStringContent InterpolatedStringContent(
            InterpolatedStringContentSyntax contentSyntax, InterpolatedStringExpression parent)
        {
            switch (contentSyntax)
            {
                case InterpolationSyntax interpolation:
                    return new Interpolation(interpolation, parent);
                case InterpolatedStringTextSyntax interpolatedStringText:
                    return new InterpolatedStringText(interpolatedStringText, parent);
            }

            throw new InvalidOperationException();
        }

        public static SwitchLabel SwitchLabel(SwitchLabelSyntax labelSyntax, SwitchSection parent)
        {
            switch (labelSyntax)
            {
                case CaseSwitchLabelSyntax _:
                case CasePatternSwitchLabelSyntax _:
                    return new SwitchCase(labelSyntax, parent);
                case DefaultSwitchLabelSyntax @default:
                    return new SwitchDefault(@default, parent);
            }

            throw new InvalidOperationException();
        }

        public static Pattern Pattern(PatternSyntax patternSyntax, SyntaxNode parent) =>
            patternSyntax switch
            {
                ConstantPatternSyntax constant => new ConstantPattern(constant, parent),
                DeclarationPatternSyntax declaration => new TypePattern(declaration, parent),
                DiscardPatternSyntax discard => new DiscardPattern(discard, parent),
                RecursivePatternSyntax recursive => recursive.PositionalPatternClause == null
                    ? (Pattern)new PropertyPattern(recursive, parent)
                    : new PositionalPattern(recursive, parent),
                VarPatternSyntax var => new VarPattern(var, parent),
                _ => throw new InvalidOperationException(),
            };

        public static LinqClause LinqClause(Roslyn::SyntaxNode clauseSyntax, LinqExpression parent)
        {
            switch (clauseSyntax)
            {
                case FromClauseSyntax from:
                    return new FromClause(from, parent);
                case GroupClauseSyntax group:
                    return new GroupByClause(group, parent);
                case JoinClauseSyntax join:
                    return new JoinClause(join, parent);
                case LetClauseSyntax let:
                    return new LetClause(let, parent);
                case OrderByClauseSyntax orderBy:
                    return new OrderByClause(orderBy, parent);
                case SelectClauseSyntax select:
                    return new SelectClause(select, parent);
                case WhereClauseSyntax where:
                    return new WhereClause(where, parent);
            }

            throw new NotImplementedException(clauseSyntax.GetType().Name);
        }

        public static TypeParameterConstraint TypeParameterConstraint(
            TypeParameterConstraintSyntax constraintSyntax, TypeParameterConstraintClause parent)
        {
            switch (constraintSyntax)
            {
                case ClassOrStructConstraintSyntax classOrStructConstraint:
                    if (classOrStructConstraint.Kind() == SyntaxKind.ClassConstraint)
                        return new ClassConstraint(classOrStructConstraint, parent);
                    else
                        return new StructConstraint(classOrStructConstraint, parent);
                case ConstructorConstraintSyntax constructorConstraint:
                    return new ConstructorConstraint(constructorConstraint, parent);
                case TypeConstraintSyntax typeConstraint:
                    return new TypeConstraint(typeConstraint, parent);
            }

            throw new InvalidOperationException();
        }
    }
}
