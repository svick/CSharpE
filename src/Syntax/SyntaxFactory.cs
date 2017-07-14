using System;

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
        
        #endregion
    }
}