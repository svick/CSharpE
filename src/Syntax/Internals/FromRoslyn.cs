using System;
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
        private static bool HasUnresolvedErrors(Roslyn::SyntaxNode node)
        {
            var errors = node.GetDiagnostics().Where(d => d.DefaultSeverity == DiagnosticSeverity.Error);

            return errors.Except(GetPossiblyInvalidNodes(node).SelectMany(c => c?.GetDiagnostics() ?? Array.Empty<Diagnostic>())).Any();
        }

        /// <summary>
        /// Returns descendant nodes that, when converted to CSharpE's object model, can represent their own errors
        /// (using Invalid* types)
        /// </summary>
        private static IEnumerable<Roslyn::SyntaxNode> GetPossiblyInvalidNodes(Roslyn::SyntaxNode syntax)
        {
            bool IsPossiblyInvalid(Roslyn::SyntaxNode node) =>
                node is MemberDeclarationSyntax || node is ExpressionSyntax || node is StatementSyntax;

            // find the highest possibly invalid nodes
            // (further descending into their children would be pointles)
            return syntax.DescendantNodes(descendIntoChildren: n => !IsPossiblyInvalid(n)).Where(n => IsPossiblyInvalid(n));
        }

        public static Expression Expression(ExpressionSyntax syntax, SyntaxNode parent) =>
            syntax switch
            {
                null => null,
                _ when HasUnresolvedErrors(syntax) => new InvalidExpression(syntax, parent),
                AnonymousMethodExpressionSyntax anonymousMethod => new DelegateExpression(anonymousMethod, parent),
                AnonymousObjectCreationExpressionSyntax anonymousObjectCreation => new AnonymousNewExpression(anonymousObjectCreation, parent),
                ArrayCreationExpressionSyntax arrayCreation => new NewArrayExpression(arrayCreation, parent),
                AssignmentExpressionSyntax assignment => AssignmentExpression(assignment, parent),
                AwaitExpressionSyntax await => new AwaitExpression(await, parent),
                BaseExpressionSyntax @base => new BaseExpression(@base, parent),
                BinaryExpressionSyntax binary => BinaryExpression(binary, parent),
                CastExpressionSyntax cast => new CastExpression(cast, parent),
                ConditionalAccessExpressionSyntax conditionalAccess => ConditionalAccessExpression(conditionalAccess, parent),
                ConditionalExpressionSyntax conditional => new ConditionalExpression(conditional, parent),
                CheckedExpressionSyntax @checked => new CheckedExpression(@checked, parent),
                DeclarationExpressionSyntax declaration => new DeclarationExpression(declaration, parent),
                DefaultExpressionSyntax @default => new DefaultExpression(@default, parent),
                ElementAccessExpressionSyntax elementAccess => new ElementAccessExpression(elementAccess, parent),
                IdentifierNameSyntax identifierName => new IdentifierExpression(identifierName, parent),
                ImplicitArrayCreationExpressionSyntax implicitArrayCreation => new ImplicitNewArrayExpression(implicitArrayCreation, parent),
                ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreation =>
                    new ImplicitStackAllocExpression(implicitStackAllocArrayCreation, parent),
                InterpolatedStringExpressionSyntax interpolatedStringExpression =>
                    new InterpolatedStringExpression(interpolatedStringExpression, parent),
                InvocationExpressionSyntax invocation => new InvocationExpression(invocation, parent),
                IsPatternExpressionSyntax isPattern => new IsPatternExpression(isPattern, parent),
                LambdaExpressionSyntax lambda => new LambdaExpression(lambda, parent),
                LiteralExpressionSyntax literal => LiteralExpression(literal, parent),
                MemberAccessExpressionSyntax memberAccess => MemberAccessExpression(memberAccess, parent),
                ObjectCreationExpressionSyntax objectCreation => new NewExpression(objectCreation, parent),
                OmittedArraySizeExpressionSyntax _ => null,
                ParenthesizedExpressionSyntax parenthesized => new ParenthesizedExpression(parenthesized, parent),
                PostfixUnaryExpressionSyntax postfixUnary => PostfixUnaryExpression(postfixUnary, parent),
                PredefinedTypeSyntax predefinedType => new NamedTypeReference(predefinedType, parent),
                PrefixUnaryExpressionSyntax prefixUnary => PrefixUnaryExpression(prefixUnary, parent),
                QueryExpressionSyntax query => new LinqExpression(query, parent),
                RangeExpressionSyntax range => new RangeExpression(range, parent),
                RefExpressionSyntax @ref => new RefExpression(@ref, parent),
                SizeOfExpressionSyntax sizeOf => new SizeOfExpression(sizeOf, parent),
                StackAllocArrayCreationExpressionSyntax stackAllocArrayCreation => new StackAllocExpression(stackAllocArrayCreation, parent),
                SwitchExpressionSyntax @switch => new SwitchExpression(@switch, parent),
                ThisExpressionSyntax @this => new ThisExpression(@this, parent),
                ThrowExpressionSyntax @throw => new ThrowExpression(@throw, parent),
                TupleExpressionSyntax tuple => new TupleExpression(tuple, parent),
                TypeOfExpressionSyntax typeOf => new TypeOfExpression(typeOf, parent),
                _ => throw new NotImplementedException(syntax.GetType().Name),
            };

        private static BinaryExpression AssignmentExpression(AssignmentExpressionSyntax syntax, SyntaxNode parent) =>
            syntax.Kind() switch
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

        private static BinaryExpression BinaryExpression(BinaryExpressionSyntax syntax, SyntaxNode parent) =>
            syntax.Kind() switch
            {
                SyntaxKind.AddExpression => new AddExpression(syntax, parent),
                SyntaxKind.SubtractExpression => new SubtractExpression(syntax, parent),
                SyntaxKind.MultiplyExpression => new MultiplyExpression(syntax, parent),
                SyntaxKind.DivideExpression => new DivideExpression(syntax, parent),
                SyntaxKind.ModuloExpression => new ModuloExpression(syntax, parent),
                SyntaxKind.LeftShiftExpression => new LeftShiftExpression(syntax, parent),
                SyntaxKind.RightShiftExpression => new RightShiftExpression(syntax, parent),
                SyntaxKind.LogicalOrExpression => new LogicalOrExpression(syntax, parent),
                SyntaxKind.LogicalAndExpression => new LogicalAndExpression(syntax, parent),
                SyntaxKind.BitwiseOrExpression => new BitwiseOrExpression(syntax, parent),
                SyntaxKind.BitwiseAndExpression => new BitwiseAndExpression(syntax, parent),
                SyntaxKind.ExclusiveOrExpression => new XorExpression(syntax, parent),
                SyntaxKind.EqualsExpression => new EqualsExpression(syntax, parent),
                SyntaxKind.NotEqualsExpression => new NotEqualsExpression(syntax, parent),
                SyntaxKind.LessThanExpression => new LessThanExpression(syntax, parent),
                SyntaxKind.LessThanOrEqualExpression => new LessThanOrEqualExpression(syntax, parent),
                SyntaxKind.GreaterThanExpression => new GreaterThanExpression(syntax, parent),
                SyntaxKind.GreaterThanOrEqualExpression => new GreaterThanOrEqualExpression(syntax, parent),
                SyntaxKind.IsExpression => new IsExpression(syntax, parent),
                SyntaxKind.AsExpression => new AsExpression(syntax, parent),
                SyntaxKind.CoalesceExpression => new CoalesceExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

        private static Expression ConditionalAccessExpression(ConditionalAccessExpressionSyntax syntax, SyntaxNode parent) =>
            syntax.WhenNotNull switch
            {
                MemberBindingExpressionSyntax _ => new ConditionalMemberAccessExpression(syntax, parent),
                ElementBindingExpressionSyntax _ => new ConditionalElementAccessExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

        private static BaseMemberAccessExpression MemberAccessExpression(MemberAccessExpressionSyntax syntax, SyntaxNode parent) =>
            syntax.Kind() switch
            {
                SyntaxKind.SimpleMemberAccessExpression => new MemberAccessExpression(syntax, parent),
                SyntaxKind.PointerMemberAccessExpression => new PointerMemberAccessExpression(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

        private static UnaryExpression PrefixUnaryExpression(PrefixUnaryExpressionSyntax syntax, SyntaxNode parent) =>
            syntax.Kind() switch
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
            syntax.Kind() switch
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

        public static Initializer Initializer(InitializerExpressionSyntax syntax, SyntaxNode parent) =>
            syntax?.Kind() switch
            {
                null => null,
                SyntaxKind.ObjectInitializerExpression => new ObjectInitializer(syntax, parent),
                SyntaxKind.CollectionInitializerExpression => new CollectionInitializer(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

        public static VariableInitializer VariableInitializer(ExpressionSyntax syntax, SyntaxNode parent) =>
            syntax switch
            {
                InitializerExpressionSyntax initializer => new ArrayInitializer(initializer, parent),
                _ => new ExpressionVariableInitializer(syntax, parent),
            };

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

        public static Statement Statement(StatementSyntax syntax, SyntaxNode parent) =>
            syntax switch
            {
                null => null,
                _ when HasUnresolvedErrors(syntax) => new InvalidStatement(syntax, parent),
                BlockSyntax block => new BlockStatement(block, parent),
                BreakStatementSyntax @break => new BreakStatement(@break, parent),
                CheckedStatementSyntax @checked => new CheckedStatement(@checked, parent),
                ContinueStatementSyntax @continue => new ContinueStatement(@continue, parent),
                DoStatementSyntax @do => new DoWhileStatement(@do, parent),
                EmptyStatementSyntax empty => new EmptyStatement(empty, parent),
                ExpressionStatementSyntax expression => new ExpressionStatement(expression, parent),
                FixedStatementSyntax @fixed => new FixedStatement(@fixed, parent),
                ForEachStatementSyntax forEach => new ForEachStatement(forEach, parent),
                ForEachVariableStatementSyntax forEachVariable => new ForEachPatternStatement(forEachVariable, parent),
                ForStatementSyntax @for => new ForStatement(@for, parent),
                GotoStatementSyntax @goto => GotoStatement(@goto, parent),
                IfStatementSyntax @if => new IfStatement(@if, parent),
                LabeledStatementSyntax labeled => new LabelStatement(labeled, parent),
                LocalFunctionStatementSyntax localFunction => new LocalFunctionStatement(localFunction, parent),
                LocalDeclarationStatementSyntax localDeclaration => new VariableDeclarationStatement(localDeclaration, parent),
                LockStatementSyntax @lock => new LockStatement(@lock, parent),
                ReturnStatementSyntax @return => new ReturnStatement(@return, parent),
                SwitchStatementSyntax @switch => new SwitchStatement(@switch, parent),
                ThrowStatementSyntax @throw => new ExpressionStatement(@throw, parent),
                TryStatementSyntax @try => new TryStatement(@try, parent),
                UnsafeStatementSyntax @unsafe => new UnsafeStatement(@unsafe, parent),
                UsingStatementSyntax @using => new UsingStatement(@using, parent),
                WhileStatementSyntax @while => new WhileStatement(@while, parent),
                YieldStatementSyntax yield => yield.Kind() == SyntaxKind.YieldBreakStatement
                    ? (Statement)new YieldBreakStatement(yield, parent)
                    : new YieldReturnStatement(yield, parent),
                _ => throw new NotImplementedException(syntax.Kind().ToString()),
            };

        private static Statement GotoStatement(GotoStatementSyntax syntax, SyntaxNode parent) =>
            syntax.Kind() switch
            {
                SyntaxKind.GotoStatement => new GotoStatement(syntax, parent),
                SyntaxKind.GotoCaseStatement => new GotoCaseStatement(syntax, parent),
                SyntaxKind.GotoDefaultStatement => new GotoDefaultStatement(syntax, parent),
                _ => throw new InvalidOperationException(),
            };

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

        public static bool IsCompacted(Roslyn::SyntaxNode syntaxNode) =>
            syntaxNode switch
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

        public static MemberDefinition MemberDefinition(MemberDeclarationSyntax memberDeclarationSyntax, TypeDefinition containingType) =>
            memberDeclarationSyntax switch
            {
                _ when HasUnresolvedErrors(memberDeclarationSyntax) => new InvalidMemberDefinition(memberDeclarationSyntax, containingType),
                FieldDeclarationSyntax field => new FieldDefinition(field, containingType),
                PropertyDeclarationSyntax property => new PropertyDefinition(property, containingType),
                IndexerDeclarationSyntax indexer => new IndexerDefinition(indexer, containingType),
                EventDeclarationSyntax @event => new EventDefinition(@event, containingType),
                EventFieldDeclarationSyntax eventField => new EventDefinition(eventField, containingType),
                MethodDeclarationSyntax method => new MethodDefinition(method, containingType),
                ConstructorDeclarationSyntax constructor => new ConstructorDefinition(constructor, containingType),
                DestructorDeclarationSyntax destructor => new FinalizerDefinition(destructor, containingType),
                OperatorDeclarationSyntax @operator => new OperatorDefinition(@operator, containingType),
                ConversionOperatorDeclarationSyntax conversionOperator => new OperatorDefinition(conversionOperator, containingType),
                BaseTypeDeclarationSyntax baseType => TypeDefinition(baseType, containingType),
                DelegateDeclarationSyntax @delegate => new DelegateDefinition(@delegate, containingType),
                _ => throw new NotImplementedException(memberDeclarationSyntax.GetType().Name),
            };

        public static BaseTypeDefinition TypeDefinition(MemberDeclarationSyntax memberDeclarationSyntax, SyntaxNode parent) =>
            memberDeclarationSyntax switch
            {
                _ when HasUnresolvedErrors(memberDeclarationSyntax) => new InvalidMemberDefinition(memberDeclarationSyntax, parent),
                DelegateDeclarationSyntax delegateDeclaration => new DelegateDefinition(delegateDeclaration, parent),
                BaseTypeDeclarationSyntax baseTypeDeclaration => TypeDefinition(baseTypeDeclaration, parent),
                _ => throw new InvalidOperationException(),
            };

        private static BaseTypeDefinition TypeDefinition(BaseTypeDeclarationSyntax typeDeclarationSyntax, SyntaxNode parent) =>
            typeDeclarationSyntax switch
            {
                ClassDeclarationSyntax classDeclaration => new ClassDefinition(classDeclaration, parent),
                StructDeclarationSyntax structDeclaration => new StructDefinition(structDeclaration, parent),
                InterfaceDeclarationSyntax interfaceDeclaration => new InterfaceDefinition(interfaceDeclaration, parent),
                EnumDeclarationSyntax enumDeclaration => new EnumDefinition(enumDeclaration, parent),
                _ => throw new InvalidOperationException(),
            };

        public static TypeReference TypeReference(TypeSyntax typeSyntax, SyntaxNode parent) =>
            typeSyntax switch
            {
                null => null,
                OmittedTypeArgumentSyntax _ => null,
                NameSyntax _ => new NamedTypeReference(typeSyntax, parent),
                PredefinedTypeSyntax _ => new NamedTypeReference(typeSyntax, parent),
                ArrayTypeSyntax array => new ArrayTypeReference(array, parent),
                NullableTypeSyntax nullable => new NullableTypeReference(nullable, parent),
                PointerTypeSyntax pointer => new PointerTypeReference(pointer, parent),
                RefTypeSyntax @ref => new RefTypeReference(@ref, parent),
                TupleTypeSyntax tuple => new TupleTypeReference(tuple, parent),
                _ => throw new NotImplementedException(typeSyntax.GetType().Name),
            };

        public static TypeReference TypeReference(ITypeSymbol typeSymbol) =>
            typeSymbol switch
            {
                null => null,
                IErrorTypeSymbol _ => null,
                INamedTypeSymbol namedType => new NamedTypeReference(namedType),
                IArrayTypeSymbol arrayType => new ArrayTypeReference(arrayType),
                IPointerTypeSymbol pointerType => new PointerTypeReference(pointerType),
                _ => throw new NotImplementedException(),
            };

        public static InterpolatedStringContent InterpolatedStringContent(
            InterpolatedStringContentSyntax contentSyntax, InterpolatedStringExpression parent) =>
            contentSyntax switch
            {
                InterpolationSyntax interpolation => new Interpolation(interpolation, parent),
                InterpolatedStringTextSyntax interpolatedStringText => new InterpolatedStringText(interpolatedStringText, parent),
                _ => throw new InvalidOperationException(),
            };

        public static SwitchLabel SwitchLabel(SwitchLabelSyntax labelSyntax, SwitchSection parent) =>
            labelSyntax switch
            {
                CaseSwitchLabelSyntax _ => new SwitchCase(labelSyntax, parent),
                CasePatternSwitchLabelSyntax _ => new SwitchCase(labelSyntax, parent),
                DefaultSwitchLabelSyntax @default => new SwitchDefault(@default, parent),
                _ => throw new InvalidOperationException(),
            };

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

        public static LinqClause LinqClause(Roslyn::SyntaxNode clauseSyntax, LinqExpression parent) =>
            clauseSyntax switch
            {
                FromClauseSyntax from => new FromClause(from, parent),
                GroupClauseSyntax group => new GroupByClause(group, parent),
                JoinClauseSyntax join => new JoinClause(join, parent),
                LetClauseSyntax let => new LetClause(let, parent),
                OrderByClauseSyntax orderBy => new OrderByClause(orderBy, parent),
                SelectClauseSyntax select => new SelectClause(select, parent),
                WhereClauseSyntax where => new WhereClause(where, parent),
                _ => throw new NotImplementedException(clauseSyntax.GetType().Name),
            };

        public static TypeParameterConstraint TypeParameterConstraint(
            TypeParameterConstraintSyntax constraintSyntax, TypeParameterConstraintClause parent) =>
            constraintSyntax switch
            {
                ClassOrStructConstraintSyntax classOrStructConstraint => classOrStructConstraint.Kind() == SyntaxKind.ClassConstraint
                    ? new ClassConstraint(classOrStructConstraint, parent)
                    : (TypeParameterConstraint)new StructConstraint(classOrStructConstraint, parent),
                ConstructorConstraintSyntax constructorConstraint => new ConstructorConstraint(constructorConstraint, parent),
                TypeConstraintSyntax typeConstraint => new TypeConstraint(typeConstraint, parent),
                _ => throw new InvalidOperationException(),
            };
    }
}
