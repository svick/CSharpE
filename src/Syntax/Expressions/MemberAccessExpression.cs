using static CSharpE.Syntax.SyntaxFactory;

namespace CSharpE.Syntax
{
    public class MemberAccessExpression : Expression
    {
        public Expression Receiver { get; set; }
        
        public string MemberName { get; set; }
        
        public MemberAccessExpression(FieldDefinition fieldDefinition)
        {
            if (fieldDefinition.Modifiers.Contains(MemberModifiers.Static))
                Receiver = fieldDefinition.ContainingType;
            else
                Receiver = This();

            MemberName = fieldDefinition.Name;
        }
    }
}