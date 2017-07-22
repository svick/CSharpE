using System;
using System.Collections.Generic;

namespace CSharpE.Syntax
{
    public static class SyntaxFactory
    {
        #region General

        public static TypeReference TypeReference(Type type) => type;

        public static TypeReference TypeReference(TypeReference openGenericType, params TypeReference[] typeParameters) =>
            new TypeReference(openGenericType, typeParameters);

        #endregion

        #region Expressions

        public static CallExpression Call(Expression receiver, string methodName, params Expression[] arguments) =>
            new CallExpression(receiver, methodName, arguments);

        public static NewExpression New(TypeReference type, params Expression[] arguments) => new NewExpression(type, arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);
        
        public static ThisExpression This() => new ThisExpression();
        
        public static AwaitExpression Await(Expression operand) => new AwaitExpression(operand);

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
 