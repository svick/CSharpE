namespace CSharpE.Syntax
{
    public class MethodDefinition : MemberDefinition
    {
        public Accessibility Accessibility { get; set; }
        public TypeReference ReturnType { get; set; }
    }
}