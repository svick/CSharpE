using System.Collections.Generic;
using System.Linq;

namespace CSharpE.Syntax
{
    public class MethodDefinition : MemberDefinition
    {
        public MemberModifiers Modifiers { get; set; }
        
        public TypeReference ReturnType { get; set; }

        private List<Statement> body;
        public IList<Statement> Body
        {
            get => body;
            set => body = value.ToList();
        }
    }
}