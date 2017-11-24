using Microsoft.CodeAnalysis.CSharp.Syntax;
using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class MemberAccessExpression : Expression
    {
        public Expression Receiver { get; set; }
        
        public string MemberName { get; set; }

        public MemberAccessExpression(Expression receiver, string memberName)
        {
            Receiver = receiver;
            MemberName = memberName;
        }

        internal MemberAccessExpression(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.Modifiers.Contains(MemberModifiers.Static))
                Receiver = fieldDefinition.ContainingType;
            else
                Receiver = This();

            MemberName = fieldDefinition.Name;
        }

        internal override ExpressionSyntax GetWrapped()
        {
            throw new System.NotImplementedException();
        }
    }
}