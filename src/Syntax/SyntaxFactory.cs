using System;
using System.Collections.Generic;

namespace CSharpE.Syntax
{
    public static class SyntaxFactory
    {
        #region General

        public static TypeReference TypeReference(Type type) => type;

        public static NamedTypeReference TypeReference(
            NamedTypeReference openGenericType, params TypeReference[] typeParameters) =>
            new NamedTypeReference(
                openGenericType.Namespace, openGenericType.Container, openGenericType.Name, typeParameters);
        
        public static Parameter Parameter(TypeReference type, string name) => new Parameter(type, name);

        #endregion

        #region Expressions

        public static InvocationExpression Call(Expression receiver, string methodName, params Expression[] arguments) =>
            new InvocationExpression(new MemberAccessExpression(receiver, methodName), arguments);

        public static MemberAccessExpression MemberAccess(Expression expression, FieldDefinition field) =>
            new MemberAccessExpression(expression, field);

        public static NewExpression New(TypeReference type, params Expression[] arguments) => new NewExpression(type, arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);
        
        public static ThisExpression This() => new ThisExpression();
        
        public static AwaitExpression Await(Expression operand) => new AwaitExpression(operand);
        
        public static ThrowExpression Throw(Expression operand) => new ThrowExpression(operand);

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
        
        #endregion
    }
}
 