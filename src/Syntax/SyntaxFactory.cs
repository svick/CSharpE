using System;

namespace CSharpE.Syntax
{
    public static class SyntaxFactory
    {
        public static FieldModifiers ReadOnly => FieldModifiers.ReadOnly;

        public static TypeReference TypeReference(Type type) => type;

        public static TypeReference TypeReference(TypeReference openGenericType, params TypeReference[] typeParameters) =>
            new TypeReference(openGenericType, typeParameters);

        public static NewExpression New(TypeReference type, params Expression[] arguments) => new NewExpression(type, arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);
    }
}