using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax
{
    public static partial class SyntaxFactory
    {
        #region Expressions

        public static InvocationExpression Call(
            this Expression receiver, string methodName, params Expression[] arguments) =>
            Call(receiver, methodName, arguments.AsEnumerable());

        public static InvocationExpression Call(
            this Expression receiver, string methodName, IEnumerable<Expression> arguments) =>
            new InvocationExpression(new MemberAccessExpression(receiver, methodName), arguments);

        public static InvocationExpression Call(
            this Expression receiver, string methodName, IEnumerable<TypeReference> typeArguments,
            IEnumerable<Expression> arguments) =>
            new InvocationExpression(new MemberAccessExpression(receiver, methodName, typeArguments), arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);

        public static CharLiteralExpression Literal(char value) => new CharLiteralExpression(value);
        
        public static StringLiteralExpression Literal(string value) => new StringLiteralExpression(value);
        
        public static BoolLiteralExpression True() => new BoolLiteralExpression(true);
        
        public static BoolLiteralExpression False() => new BoolLiteralExpression(false);

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

        public static ConstructorInitializer ThisInitializer(params Argument[] arguments) =>
            ConstructorInitializer(ConstructorInitializerKind.This, arguments);

        public static ConstructorInitializer ThisInitializer(IEnumerable<Argument> arguments) =>
            ConstructorInitializer(ConstructorInitializerKind.This, arguments);

        public static ConstructorInitializer BaseInitializer(params Argument[] arguments) =>
            ConstructorInitializer(ConstructorInitializerKind.Base, arguments);

        public static ConstructorInitializer BaseInitializer(IEnumerable<Argument> arguments) =>
            ConstructorInitializer(ConstructorInitializerKind.Base, arguments);
    }
}
 