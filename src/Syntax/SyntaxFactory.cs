using System;

namespace CSharpE.Syntax
{
    public static class SyntaxFactory
    {
        public static FieldModifiers ReadOnly => FieldModifiers.ReadOnly;

        public static NewExpression New(Type type, params Expression[] arguments) => New(new TypeReference(type), arguments);

        public static NewExpression New(TypeReference type, params Expression[] arguments) => new NewExpression(type, arguments);

        public static IntLiteralExpression Literal(int value) => new IntLiteralExpression(value);
    }
}