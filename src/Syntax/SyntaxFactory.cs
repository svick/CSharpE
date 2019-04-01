using System;
using System.Collections.Generic;

namespace CSharpE.Syntax
{
    public static class SyntaxFactory
    {
        #region General

        public static NamedTypeReference TypeReference(Type type) => type;

        public static NamedTypeReference TypeReference(
            NamedTypeReference openGenericType, params TypeReference[] typeParameters) =>
            new NamedTypeReference(
                openGenericType.Namespace, openGenericType.Container, openGenericType.Name, typeParameters);
        
        public static Parameter Parameter(TypeReference type, string name) => new Parameter(type, name);

        #endregion

        #region Expressions

        public static InvocationExpression Call(this Expression receiver, string methodName, params Expression[] arguments) =>
            new InvocationExpression(new MemberAccessExpression(receiver, methodName), arguments);

        public static InvocationExpression Call(this Expression receiver, string methodName, IEnumerable<Expression> arguments) =>
            new InvocationExpression(new MemberAccessExpression(receiver, methodName), arguments);
        
        public static MemberAccessExpression MemberAccess(this Expression expression, FieldDefinition field) =>
            new MemberAccessExpression(expression, field);

        public static MemberAccessExpression MemberAccess(this NamedTypeReference type, string memberName) =>
            new MemberAccessExpression(type, memberName);

        public static MemberAccessExpression MemberAccess(this Expression expression, string memberName) =>
            new MemberAccessExpression(expression, memberName);

        public static NewExpression New(TypeReference type, params Expression[] arguments) => new NewExpression(type, arguments);

        public static NewExpression New(TypeReference type, IEnumerable<Expression> arguments) => new NewExpression(type, arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);
        
        public static StringLiteralExpression Literal(string value) => new StringLiteralExpression(value);
        
        public static ThisExpression This() => new ThisExpression();
        
        public static AwaitExpression Await(Expression operand) => new AwaitExpression(operand);
        
        public static ThrowExpression Throw(Expression operand) => new ThrowExpression(operand);
        
        public static IdentifierExpression Identifier(string identifier) => new IdentifierExpression(identifier);

        public static NullExpression Null => new NullExpression();
        
        public static BoolLiteralExpression True => new BoolLiteralExpression(true);
        
        public static BoolLiteralExpression False => new BoolLiteralExpression(false);

        public static LogicalAndExpression LogicalAnd(Expression left, Expression right) => new LogicalAndExpression(left, right);

        public static AsExpression As(Expression left, TypeReference right) => new AsExpression(left, right);

        public static AssignmentExpression Assignment(Expression left, Expression right) => new AssignmentExpression(left, right);

        public static TupleExpression Tuple(params Expression[] expressions) => new TupleExpression(expressions);

        public static TupleExpression Tuple(IEnumerable<Expression> expressions) => new TupleExpression(expressions);

        #endregion

        #region Statements

        public static TryStatement TryCatchFinally(
            IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses,
            IEnumerable<Statement> finallyStatements) =>
            new TryStatement(tryStatements, catchClauses, finallyStatements);

        public static TryStatement TryCatch(
            IEnumerable<Statement> tryStatements, IEnumerable<CatchClause> catchClauses) =>
            new TryStatement(tryStatements, catchClauses);

        public static TryStatement TryCatch(
            IEnumerable<Statement> tryStatements, params CatchClause[] catchClauses) =>
            new TryStatement(tryStatements, catchClauses);

        public static TryStatement TryFinally(
            IEnumerable<Statement> tryStatements, IEnumerable<Statement> finallyStatements) =>
            new TryStatement(tryStatements, finallyStatements);

        public static IfStatement If(Expression condition, IEnumerable<Statement> thenStatements) =>
            new IfStatement(condition, thenStatements);

        public static IfStatement If(Expression condition, params Statement[] thenStatements) =>
            new IfStatement(condition, thenStatements);
        
        public static ReturnStatement Return() => new ReturnStatement();
        
        public static ReturnStatement Return(Expression expression) => new ReturnStatement(expression);
        
        public static BlockStatement Block(params Statement[] statements) => new BlockStatement(statements);
        
        public static BlockStatement Block(IEnumerable<Statement> statements) => new BlockStatement(statements);

        #endregion
    }
}
 