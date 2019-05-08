using System;
using System.Collections.Generic;

namespace CSharpE.Syntax
{
    public static partial class SyntaxFactory
    {
        public static Attribute Attribute(NamedTypeReference type, params AttributeArgument[] arguments)
        {
            return new Attribute(type, arguments);
        }

        public static Attribute Attribute(NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
        {
            return new Attribute(type, arguments);
        }

        public static Attribute Attribute(AttributeTarget target, NamedTypeReference type, params AttributeArgument[] arguments)
        {
            return new Attribute(target, type, arguments);
        }

        public static Attribute Attribute(AttributeTarget target, NamedTypeReference type, IEnumerable<AttributeArgument> arguments)
        {
            return new Attribute(target, type, arguments);
        }

        public static AttributeArgument AttributeArgument(Expression expression)
        {
            return new AttributeArgument(expression);
        }

        public static AttributeArgument AttributeArgument(string name, Expression expression)
        {
            return new AttributeArgument(name, expression);
        }

        public static AttributeArgument AttributeArgument(string name, bool isConstructorArgument, Expression expression)
        {
            return new AttributeArgument(name, isConstructorArgument, expression);
        }

        public static AccessorDefinition AccessorDefinition()
        {
            return new AccessorDefinition();
        }

        public static ConstructorDefinition ConstructorDefinition(MemberModifiers modifiers, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            return new ConstructorDefinition(modifiers, parameters, body);
        }

        public static DelegateDefinition DelegateDefinition(TypeReference returnType, params Parameter[] parameters)
        {
            return new DelegateDefinition(returnType, parameters);
        }

        public static DelegateDefinition DelegateDefinition(TypeReference returnType, IEnumerable<Parameter> parameters)
        {
            return new DelegateDefinition(returnType, parameters);
        }

        public static EnumDefinition EnumDefinition(MemberModifiers modifiers, string name, IEnumerable<EnumMemberDefinition> members = null)
        {
            return new EnumDefinition(modifiers, name, members);
        }

        public static EnumMemberDefinition EnumMemberDefinition(string name, Expression initializer = null)
        {
            return new EnumMemberDefinition(name, initializer);
        }

        public static FieldDefinition FieldDefinition(MemberModifiers modifiers, TypeReference type, string name, Expression initializer = null)
        {
            return new FieldDefinition(modifiers, type, name, initializer);
        }

        public static FieldDefinition FieldDefinition(TypeReference type, string name, Expression initializer = null)
        {
            return new FieldDefinition(type, name, initializer);
        }

        public static FinalizerDefinition FinalizerDefinition(MemberModifiers modifiers, IEnumerable<Statement> body)
        {
            return new FinalizerDefinition(modifiers, body);
        }

        public static MethodDefinition MethodDefinition(MemberModifiers modifiers, TypeReference returnType, string name, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            return new MethodDefinition(modifiers, returnType, name, parameters, body);
        }

        public static NamespaceDefinition NamespaceDefinition(string name, params NamespaceOrTypeDefinition[] members)
        {
            return new NamespaceDefinition(name, members);
        }

        public static NamespaceDefinition NamespaceDefinition(string name, IEnumerable<NamespaceOrTypeDefinition> members)
        {
            return new NamespaceDefinition(name, members);
        }

        public static OperatorDefinition OperatorDefinition(MemberModifiers modifiers, TypeReference returnType, OperatorKind kind, IEnumerable<Parameter> parameters, IEnumerable<Statement> body)
        {
            return new OperatorDefinition(modifiers, returnType, kind, parameters, body);
        }

        public static PropertyDefinition PropertyDefinition(MemberModifiers modifiers, TypeReference type, string name)
        {
            return new PropertyDefinition(modifiers, type, name);
        }

        public static ClassDefinition ClassDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
        {
            return new ClassDefinition(modifiers, name, members);
        }

        public static ClassDefinition ClassDefinition(string name, IEnumerable<MemberDefinition> members = null)
        {
            return new ClassDefinition(name, members);
        }

        public static StructDefinition StructDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
        {
            return new StructDefinition(modifiers, name, members);
        }

        public static StructDefinition StructDefinition(string name, IEnumerable<MemberDefinition> members = null)
        {
            return new StructDefinition(name, members);
        }

        public static InterfaceDefinition InterfaceDefinition(MemberModifiers modifiers, string name, IEnumerable<MemberDefinition> members = null)
        {
            return new InterfaceDefinition(modifiers, name, members);
        }

        public static InterfaceDefinition InterfaceDefinition(string name, IEnumerable<MemberDefinition> members = null)
        {
            return new InterfaceDefinition(name, members);
        }

        public static AnonymousNewExpression AnonymousNew(params AnonymousObjectInitializer[] initializers)
        {
            return new AnonymousNewExpression(initializers);
        }

        public static AnonymousNewExpression AnonymousNew(IEnumerable<AnonymousObjectInitializer> initializers)
        {
            return new AnonymousNewExpression(initializers);
        }

        public static Argument Argument(Expression expression, string name = null)
        {
            return new Argument(expression, name);
        }

        public static AwaitExpression Await(Expression operand)
        {
            return new AwaitExpression(operand);
        }

        public static BaseExpression Base()
        {
            return new BaseExpression();
        }

        public static AddExpression Add(Expression left, Expression right)
        {
            return new AddExpression(left, right);
        }

        public static SubtractExpression Subtract(Expression left, Expression right)
        {
            return new SubtractExpression(left, right);
        }

        public static MultiplyExpression Multiply(Expression left, Expression right)
        {
            return new MultiplyExpression(left, right);
        }

        public static DivideExpression Divide(Expression left, Expression right)
        {
            return new DivideExpression(left, right);
        }

        public static ModuloExpression Modulo(Expression left, Expression right)
        {
            return new ModuloExpression(left, right);
        }

        public static LeftShiftExpression LeftShift(Expression left, Expression right)
        {
            return new LeftShiftExpression(left, right);
        }

        public static RightShiftExpression RightShift(Expression left, Expression right)
        {
            return new RightShiftExpression(left, right);
        }

        public static LogicalOrExpression LogicalOr(Expression left, Expression right)
        {
            return new LogicalOrExpression(left, right);
        }

        public static LogicalAndExpression LogicalAnd(Expression left, Expression right)
        {
            return new LogicalAndExpression(left, right);
        }

        public static BitwiseOrExpression BitwiseOr(Expression left, Expression right)
        {
            return new BitwiseOrExpression(left, right);
        }

        public static BitwiseAndExpression BitwiseAnd(Expression left, Expression right)
        {
            return new BitwiseAndExpression(left, right);
        }

        public static XorExpression Xor(Expression left, Expression right)
        {
            return new XorExpression(left, right);
        }

        public static EqualsExpression Equals(Expression left, Expression right)
        {
            return new EqualsExpression(left, right);
        }

        public static NotEqualsExpression NotEquals(Expression left, Expression right)
        {
            return new NotEqualsExpression(left, right);
        }

        public static LessThanExpression LessThan(Expression left, Expression right)
        {
            return new LessThanExpression(left, right);
        }

        public static LessThanOrEqualExpression LessThanOrEqual(Expression left, Expression right)
        {
            return new LessThanOrEqualExpression(left, right);
        }

        public static GreaterThanExpression GreaterThan(Expression left, Expression right)
        {
            return new GreaterThanExpression(left, right);
        }

        public static GreaterThanOrEqualExpression GreaterThanOrEqual(Expression left, Expression right)
        {
            return new GreaterThanOrEqualExpression(left, right);
        }

        public static IsExpression Is(Expression left, TypeReference right)
        {
            return new IsExpression(left, right);
        }

        public static AsExpression As(Expression left, TypeReference right)
        {
            return new AsExpression(left, right);
        }

        public static CoalesceExpression Coalesce(Expression left, Expression right)
        {
            return new CoalesceExpression(left, right);
        }

        public static AssignmentExpression Assignment(Expression left, Expression right)
        {
            return new AssignmentExpression(left, right);
        }

        public static AddAssignmentExpression AddAssignment(Expression left, Expression right)
        {
            return new AddAssignmentExpression(left, right);
        }

        public static SubtractAssignmentExpression SubtractAssignment(Expression left, Expression right)
        {
            return new SubtractAssignmentExpression(left, right);
        }

        public static MultiplyAssignmentExpression MultiplyAssignment(Expression left, Expression right)
        {
            return new MultiplyAssignmentExpression(left, right);
        }

        public static DivideAssignmentExpression DivideAssignment(Expression left, Expression right)
        {
            return new DivideAssignmentExpression(left, right);
        }

        public static ModuloAssignmentExpression ModuloAssignment(Expression left, Expression right)
        {
            return new ModuloAssignmentExpression(left, right);
        }

        public static AndAssignmentExpression AndAssignment(Expression left, Expression right)
        {
            return new AndAssignmentExpression(left, right);
        }

        public static XorAssignmentExpression XorAssignment(Expression left, Expression right)
        {
            return new XorAssignmentExpression(left, right);
        }

        public static OrAssignmentExpression OrAssignment(Expression left, Expression right)
        {
            return new OrAssignmentExpression(left, right);
        }

        public static LeftShiftAssignmentExpression LeftShiftAssignment(Expression left, Expression right)
        {
            return new LeftShiftAssignmentExpression(left, right);
        }

        public static RightShiftAssignmentExpression RightShiftAssignment(Expression left, Expression right)
        {
            return new RightShiftAssignmentExpression(left, right);
        }

        public static CastExpression Cast(TypeReference type, Expression expression)
        {
            return new CastExpression(type, expression);
        }

        public static CheckedExpression Checked(bool isChecked, Expression expression)
        {
            return new CheckedExpression(isChecked, expression);
        }

        public static ConditionalExpression Conditional(Expression condition, Expression whenTrue, Expression whenFalse)
        {
            return new ConditionalExpression(condition, whenTrue, whenFalse);
        }

        public static DeclarationExpression Declaration(TypeReference type, VariableDesignation designation)
        {
            return new DeclarationExpression(type, designation);
        }

        public static DefaultExpression Default(TypeReference type = null)
        {
            return new DefaultExpression(type);
        }

        public static ElementAccessExpression ElementAccess(this Expression expression, params Argument[] arguments)
        {
            return new ElementAccessExpression(expression, arguments);
        }

        public static ElementAccessExpression ElementAccess(this Expression expression, IEnumerable<Argument> arguments)
        {
            return new ElementAccessExpression(expression, arguments);
        }

        public static ConditionalElementAccessExpression ConditionalElementAccess(Expression expression, params Argument[] arguments)
        {
            return new ConditionalElementAccessExpression(expression, arguments);
        }

        public static ConditionalElementAccessExpression ConditionalElementAccess(Expression expression, IEnumerable<Argument> arguments)
        {
            return new ConditionalElementAccessExpression(expression, arguments);
        }

        public static IdentifierExpression Identifier(string identifier)
        {
            return new IdentifierExpression(identifier);
        }

        public static ImplicitNewArrayExpression ImplicitNewArray(ArrayInitializer initializer)
        {
            return new ImplicitNewArrayExpression(initializer);
        }

        public static ImplicitNewArrayExpression ImplicitNewArray(int rank, ArrayInitializer initializer)
        {
            return new ImplicitNewArrayExpression(rank, initializer);
        }

        public static ImplicitStackAllocExpression ImplicitStackAlloc(ArrayInitializer initializer)
        {
            return new ImplicitStackAllocExpression(initializer);
        }

        public static AnonymousObjectInitializer AnonymousObjectInitializer(Expression expression)
        {
            return new AnonymousObjectInitializer(expression);
        }

        public static AnonymousObjectInitializer AnonymousObjectInitializer(string name, Expression expression)
        {
            return new AnonymousObjectInitializer(name, expression);
        }

        public static ArrayInitializer ArrayInitializer(params VariableInitializer[] variableInitializers)
        {
            return new ArrayInitializer(variableInitializers);
        }

        public static ArrayInitializer ArrayInitializer(IEnumerable<VariableInitializer> variableInitializers)
        {
            return new ArrayInitializer(variableInitializers);
        }

        public static ExpressionVariableInitializer ExpressionVariableInitializer(Expression expression)
        {
            return new ExpressionVariableInitializer(expression);
        }

        public static CollectionInitializer CollectionInitializer(params ElementInitializer[] elementInitializers)
        {
            return new CollectionInitializer(elementInitializers);
        }

        public static CollectionInitializer CollectionInitializer(IEnumerable<ElementInitializer> elementInitializers)
        {
            return new CollectionInitializer(elementInitializers);
        }

        public static ElementInitializer ElementInitializer(params Expression[] expressions)
        {
            return new ElementInitializer(expressions);
        }

        public static ElementInitializer ElementInitializer(IEnumerable<Expression> expressions)
        {
            return new ElementInitializer(expressions);
        }

        public static MemberInitializer MemberInitializer(MemberInitializerTarget target, MemberInitializerValue value)
        {
            return new MemberInitializer(target, value);
        }

        public static NameMemberInitializerTarget NameMemberInitializerTarget(string name)
        {
            return new NameMemberInitializerTarget(name);
        }

        public static ElementAccessMemberInitializerTarget ElementAccessMemberInitializerTarget(IEnumerable<Argument> arguments)
        {
            return new ElementAccessMemberInitializerTarget(arguments);
        }

        public static ExpressionMemberInitializerValue ExpressionMemberInitializerValue(Expression expression)
        {
            return new ExpressionMemberInitializerValue(expression);
        }

        public static InitializerMemberInitializerValue InitializerMemberInitializerValue(Initializer initializer)
        {
            return new InitializerMemberInitializerValue(initializer);
        }

        public static ObjectInitializer ObjectInitializer(params MemberInitializer[] memberInitializers)
        {
            return new ObjectInitializer(memberInitializers);
        }

        public static ObjectInitializer ObjectInitializer(IEnumerable<MemberInitializer> memberInitializers)
        {
            return new ObjectInitializer(memberInitializers);
        }

        public static Interpolation Interpolation(Expression expression, Expression alignment = null, string format = null)
        {
            return new Interpolation(expression, alignment, format);
        }

        public static InterpolatedStringText InterpolatedStringText(string text)
        {
            return new InterpolatedStringText(text);
        }

        public static InterpolatedStringExpression InterpolatedString(params InterpolatedStringContent[] contents)
        {
            return new InterpolatedStringExpression(contents);
        }

        public static InterpolatedStringExpression InterpolatedString(IEnumerable<InterpolatedStringContent> contents)
        {
            return new InterpolatedStringExpression(contents);
        }

        public static InterpolatedStringExpression InterpolatedString(bool isVerbatim, params InterpolatedStringContent[] contents)
        {
            return new InterpolatedStringExpression(isVerbatim, contents);
        }

        public static InterpolatedStringExpression InterpolatedString(bool isVerbatim, IEnumerable<InterpolatedStringContent> contents)
        {
            return new InterpolatedStringExpression(isVerbatim, contents);
        }

        public static InvocationExpression Invocation(Expression expression, IEnumerable<Argument> arguments = null)
        {
            return new InvocationExpression(expression, arguments);
        }

        public static InvocationExpression Invocation(Expression expression, IEnumerable<Expression> arguments = null)
        {
            return new InvocationExpression(expression, arguments);
        }

        public static InvocationExpression Invocation(Expression expression, params Argument[] arguments)
        {
            return new InvocationExpression(expression, arguments);
        }

        public static LambdaExpression Lambda(IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
        {
            return new LambdaExpression(parameters, statements);
        }

        public static LambdaExpression Lambda(bool isAsync, IEnumerable<LambdaParameter> parameters, IEnumerable<Statement> statements)
        {
            return new LambdaExpression(isAsync, parameters, statements);
        }

        public static LambdaExpression Lambda(IEnumerable<LambdaParameter> parameters, Expression expression)
        {
            return new LambdaExpression(parameters, expression);
        }

        public static LambdaExpression Lambda(bool isAsync, IEnumerable<LambdaParameter> parameters, Expression expression)
        {
            return new LambdaExpression(isAsync, parameters, expression);
        }

        public static LambdaParameter LambdaParameter(string name)
        {
            return new LambdaParameter(name);
        }

        public static LambdaParameter LambdaParameter(TypeReference type, string name)
        {
            return new LambdaParameter(type, name);
        }

        public static LambdaParameter LambdaParameter(LambdaParameterModifier modifier, TypeReference type, string name)
        {
            return new LambdaParameter(modifier, type, name);
        }

        public static MemberAccessExpression MemberAccess(this Expression expression, string memberName)
        {
            return new MemberAccessExpression(expression, memberName);
        }

        public static MemberAccessExpression MemberAccess(this Expression expression, string memberName, params TypeReference[] typeArguments)
        {
            return new MemberAccessExpression(expression, memberName, typeArguments);
        }

        public static MemberAccessExpression MemberAccess(this Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
        {
            return new MemberAccessExpression(expression, memberName, typeArguments);
        }

        public static MemberAccessExpression MemberAccess(this Expression expression, FieldDefinition fieldDefinition)
        {
            return new MemberAccessExpression(expression, fieldDefinition);
        }

        public static PointerMemberAccessExpression PointerMemberAccess(this Expression expression, string memberName)
        {
            return new PointerMemberAccessExpression(expression, memberName);
        }

        public static PointerMemberAccessExpression PointerMemberAccess(this Expression expression, string memberName, params TypeReference[] typeArguments)
        {
            return new PointerMemberAccessExpression(expression, memberName, typeArguments);
        }

        public static PointerMemberAccessExpression PointerMemberAccess(this Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
        {
            return new PointerMemberAccessExpression(expression, memberName, typeArguments);
        }

        public static PointerMemberAccessExpression PointerMemberAccess(this Expression expression, FieldDefinition fieldDefinition)
        {
            return new PointerMemberAccessExpression(expression, fieldDefinition);
        }

        public static ConditionalMemberAccessExpression ConditionalMemberAccess(this Expression expression, string memberName)
        {
            return new ConditionalMemberAccessExpression(expression, memberName);
        }

        public static ConditionalMemberAccessExpression ConditionalMemberAccess(this Expression expression, string memberName, params TypeReference[] typeArguments)
        {
            return new ConditionalMemberAccessExpression(expression, memberName, typeArguments);
        }

        public static ConditionalMemberAccessExpression ConditionalMemberAccess(this Expression expression, string memberName, IEnumerable<TypeReference> typeArguments)
        {
            return new ConditionalMemberAccessExpression(expression, memberName, typeArguments);
        }

        public static ConditionalMemberAccessExpression ConditionalMemberAccess(this Expression expression, FieldDefinition fieldDefinition)
        {
            return new ConditionalMemberAccessExpression(expression, fieldDefinition);
        }

        public static NewArrayExpression NewArray(TypeReference elementType, ArrayInitializer initializer = null)
        {
            return new NewArrayExpression(elementType, initializer);
        }

        public static NewArrayExpression NewArray(TypeReference elementType, params Expression[] lengths)
        {
            return new NewArrayExpression(elementType, lengths);
        }

        public static NewArrayExpression NewArray(TypeReference elementType, IEnumerable<Expression> lengths, ArrayInitializer initializer = null)
        {
            return new NewArrayExpression(elementType, lengths, initializer);
        }

        public static NewExpression New(TypeReference type, IEnumerable<Argument> arguments, Initializer initializer = null)
        {
            return new NewExpression(type, arguments, initializer);
        }

        public static NewExpression New(TypeReference type, IEnumerable<Expression> arguments, Initializer initializer = null)
        {
            return new NewExpression(type, arguments, initializer);
        }

        public static NewExpression New(TypeReference type, params Argument[] arguments)
        {
            return new NewExpression(type, arguments);
        }

        public static NullExpression Null()
        {
            return new NullExpression();
        }

        public static ParenthesizedExpression Parenthesized(Expression expression)
        {
            return new ParenthesizedExpression(expression);
        }

        public static RefExpression Ref(Expression expression)
        {
            return new RefExpression(expression);
        }

        public static SizeOfExpression SizeOf(TypeReference type)
        {
            return new SizeOfExpression(type);
        }

        public static StackAllocExpression StackAlloc(TypeReference elementType, ArrayInitializer initializer = null)
        {
            return new StackAllocExpression(elementType, initializer);
        }

        public static StackAllocExpression StackAlloc(TypeReference elementType, Expression length, ArrayInitializer initializer = null)
        {
            return new StackAllocExpression(elementType, length, initializer);
        }

        public static ThisExpression This()
        {
            return new ThisExpression();
        }

        public static ThrowExpression Throw(Expression operand)
        {
            return new ThrowExpression(operand);
        }

        public static TupleExpression Tuple(params Expression[] expressions)
        {
            return new TupleExpression(expressions);
        }

        public static TupleExpression Tuple(IEnumerable<Expression> expressions)
        {
            return new TupleExpression(expressions);
        }

        public static TupleExpression Tuple(IEnumerable<Argument> arguments)
        {
            return new TupleExpression(arguments);
        }

        public static TypeOfExpression TypeOf(TypeReference type)
        {
            return new TypeOfExpression(type);
        }

        public static UnaryPlusExpression UnaryPlus(Expression operand)
        {
            return new UnaryPlusExpression(operand);
        }

        public static UnaryMinusExpression UnaryMinus(Expression operand)
        {
            return new UnaryMinusExpression(operand);
        }

        public static ComplementExpression Complement(Expression operand)
        {
            return new ComplementExpression(operand);
        }

        public static NegateExpression Negate(Expression operand)
        {
            return new NegateExpression(operand);
        }

        public static PreIncrementExpression PreIncrement(Expression operand)
        {
            return new PreIncrementExpression(operand);
        }

        public static PreDecrementExpression PreDecrement(Expression operand)
        {
            return new PreDecrementExpression(operand);
        }

        public static AddressOfExpression AddressOf(Expression operand)
        {
            return new AddressOfExpression(operand);
        }

        public static PointerIndirectionExpression PointerIndirection(Expression operand)
        {
            return new PointerIndirectionExpression(operand);
        }

        public static PostIncrementExpression PostIncrement(Expression operand)
        {
            return new PostIncrementExpression(operand);
        }

        public static PostDecrementExpression PostDecrement(Expression operand)
        {
            return new PostDecrementExpression(operand);
        }

        public static SingleVariableDesignation SingleVariableDesignation(string name)
        {
            return new SingleVariableDesignation(name);
        }

        public static MultiVariableDesignation MultiVariableDesignation(params VariableDesignation[] variables)
        {
            return new MultiVariableDesignation(variables);
        }

        public static MultiVariableDesignation MultiVariableDesignation(IEnumerable<VariableDesignation> variables)
        {
            return new MultiVariableDesignation(variables);
        }

        public static Parameter Parameter(TypeReference type, string name)
        {
            return new Parameter(type, name);
        }

        public static Parameter Parameter(ParameterModifiers modifiers, TypeReference type, string name, Expression defaultValue = null)
        {
            return new Parameter(modifiers, type, name, defaultValue);
        }

        public static SourceFile SourceFile(string path, string text)
        {
            return new SourceFile(path, text);
        }

        public static SourceFile SourceFile(string path)
        {
            return new SourceFile(path);
        }

        public static BlockStatement Block(params Statement[] statements)
        {
            return new BlockStatement(statements);
        }

        public static BlockStatement Block(IEnumerable<Statement> statements)
        {
            return new BlockStatement(statements);
        }

        public static BreakStatement Break()
        {
            return new BreakStatement();
        }

        public static CatchClause CatchClause(TypeReference exceptionType, string exceptionName, params Statement[] statements)
        {
            return new CatchClause(exceptionType, exceptionName, statements);
        }

        public static CatchClause CatchClause(TypeReference exceptionType, string exceptionName, IEnumerable<Statement> statements)
        {
            return new CatchClause(exceptionType, exceptionName, statements);
        }

        public static CatchClause CatchClause(TypeReference exceptionType, string exceptionName, Expression filter, IEnumerable<Statement> statements)
        {
            return new CatchClause(exceptionType, exceptionName, filter, statements);
        }

        public static CheckedStatement Checked(bool isChecked, params Statement[] statements)
        {
            return new CheckedStatement(isChecked, statements);
        }

        public static CheckedStatement Checked(bool isChecked, IEnumerable<Statement> statements)
        {
            return new CheckedStatement(isChecked, statements);
        }

        public static ContinueStatement Continue()
        {
            return new ContinueStatement();
        }

        public static DoWhileStatement DoWhile(IEnumerable<Statement> statements, Expression condition)
        {
            return new DoWhileStatement(statements, condition);
        }

        public static EmptyStatement Empty()
        {
            return new EmptyStatement();
        }

        public static ExpressionStatement Expression(Expression expression)
        {
            return new ExpressionStatement(expression);
        }

        public static FixedStatement Fixed(VariableDeclarationStatement variableDeclaration, params Statement[] statements)
        {
            return new FixedStatement(variableDeclaration, statements);
        }

        public static FixedStatement Fixed(VariableDeclarationStatement variableDeclaration, IEnumerable<Statement> statements)
        {
            return new FixedStatement(variableDeclaration, statements);
        }

        public static ForEachStatement ForEach(TypeReference elementType, string elementName, Expression expression, params Statement[] statements)
        {
            return new ForEachStatement(elementType, elementName, expression, statements);
        }

        public static ForEachStatement ForEach(TypeReference elementType, string elementName, Expression expression, IEnumerable<Statement> statements)
        {
            return new ForEachStatement(elementType, elementName, expression, statements);
        }

        public static PatternForEachStatement PatternForEach(Expression elementPattern, Expression expression, params Statement[] statements)
        {
            return new PatternForEachStatement(elementPattern, expression, statements);
        }

        public static PatternForEachStatement PatternForEach(Expression elementPattern, Expression expression, IEnumerable<Statement> statements)
        {
            return new PatternForEachStatement(elementPattern, expression, statements);
        }

        public static ForStatement For(VariableDeclarationStatement variableDeclaration, Expression condition, Expression incrementor, params Statement[] statements)
        {
            return new ForStatement(variableDeclaration, condition, incrementor, statements);
        }

        public static ForStatement For(VariableDeclarationStatement variableDeclaration, Expression condition, IEnumerable<Expression> incrementors, IEnumerable<Statement> statements)
        {
            return new ForStatement(variableDeclaration, condition, incrementors, statements);
        }

        public static IfStatement If(Expression condition, params Statement[] thenStatements)
        {
            return new IfStatement(condition, thenStatements);
        }

        public static IfStatement If(Expression condition, IEnumerable<Statement> thenStatements)
        {
            return new IfStatement(condition, thenStatements);
        }

        public static IfStatement If(Expression condition, IEnumerable<Statement> thenStatements, IEnumerable<Statement> elseStatements)
        {
            return new IfStatement(condition, thenStatements, elseStatements);
        }

        public static LockStatement Lock(Expression expression, params Statement[] statements)
        {
            return new LockStatement(expression, statements);
        }

        public static LockStatement Lock(Expression expression, IEnumerable<Statement> statements)
        {
            return new LockStatement(expression, statements);
        }

        public static ReturnStatement Return(Expression expression)
        {
            return new ReturnStatement(expression);
        }

        public static ReturnStatement Return()
        {
            return new ReturnStatement();
        }

        public static UnsafeStatement Unsafe(params Statement[] statements)
        {
            return new UnsafeStatement(statements);
        }

        public static UnsafeStatement Unsafe(IEnumerable<Statement> statements)
        {
            return new UnsafeStatement(statements);
        }

        public static UsingStatement Using(VariableDeclarationStatement variableDeclaration, params Statement[] statements)
        {
            return new UsingStatement(variableDeclaration, statements);
        }

        public static UsingStatement Using(VariableDeclarationStatement variableDeclaration, IEnumerable<Statement> statements)
        {
            return new UsingStatement(variableDeclaration, statements);
        }

        public static UsingStatement Using(Expression expression, params Statement[] statements)
        {
            return new UsingStatement(expression, statements);
        }

        public static UsingStatement Using(Expression expression, IEnumerable<Statement> statements)
        {
            return new UsingStatement(expression, statements);
        }

        public static VariableDeclarationStatement VariableDeclaration(TypeReference type, string name, Expression initializer = null)
        {
            return new VariableDeclarationStatement(type, name, initializer);
        }

        public static VariableDeclarationStatement VariableDeclaration(bool isConst, TypeReference type, string name, Expression initializer = null)
        {
            return new VariableDeclarationStatement(isConst, type, name, initializer);
        }

        public static WhileStatement While(Expression condition, params Statement[] statements)
        {
            return new WhileStatement(condition, statements);
        }

        public static WhileStatement While(Expression condition, IEnumerable<Statement> statements)
        {
            return new WhileStatement(condition, statements);
        }

        public static ArrayTypeReference ArrayType(TypeReference elementType)
        {
            return new ArrayTypeReference(elementType);
        }

        public static ArrayTypeReference ArrayType(TypeReference elementType, int rank)
        {
            return new ArrayTypeReference(elementType, rank);
        }

        public static NamedTypeReference NamedType(NamedTypeReference openGenericType, params TypeReference[] typeArguments)
        {
            return new NamedTypeReference(openGenericType, typeArguments);
        }

        public static NamedTypeReference NamedType(string ns, string name, params TypeReference[] typeArguments)
        {
            return new NamedTypeReference(ns, name, typeArguments);
        }

        public static NamedTypeReference NamedType(string ns, string name, IEnumerable<TypeReference> typeArguments = null)
        {
            return new NamedTypeReference(ns, name, typeArguments);
        }

        public static NamedTypeReference NamedType(string ns, NamedTypeReference container, string name, IEnumerable<TypeReference> typeArguments = null)
        {
            return new NamedTypeReference(ns, container, name, typeArguments);
        }

        public static NamedTypeReference NamedType(Type type)
        {
            return new NamedTypeReference(type);
        }

        public static NullableTypeReference NullableType(TypeReference elementType)
        {
            return new NullableTypeReference(elementType);
        }

        public static PointerTypeReference PointerType(TypeReference elementType)
        {
            return new PointerTypeReference(elementType);
        }

        public static RefTypeReference RefType(TypeReference elementType)
        {
            return new RefTypeReference(elementType);
        }

        public static TupleElement TupleElement(TypeReference type, string name = null)
        {
            return new TupleElement(type, name);
        }

        public static TupleTypeReference TupleType(params TypeReference[] elementTypes)
        {
            return new TupleTypeReference(elementTypes);
        }

        public static TupleTypeReference TupleType(IEnumerable<TypeReference> elementTypes)
        {
            return new TupleTypeReference(elementTypes);
        }

        public static TupleTypeReference TupleType(params TupleElement[] elements)
        {
            return new TupleTypeReference(elements);
        }

        public static TupleTypeReference TupleType(IEnumerable<TupleElement> elements)
        {
            return new TupleTypeReference(elements);
        }
    }
}