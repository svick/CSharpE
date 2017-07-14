namespace CSharpE.Syntax
{
    public class IdentifierExpression : Expression
    {
        public string Identifier { get; set; }

        public IdentifierExpression(string identifier) => Identifier = identifier;
    }
}