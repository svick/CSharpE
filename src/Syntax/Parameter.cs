using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class Parameter : SyntaxNode, ISyntaxWrapper<ParameterSyntax>
    {
        private ParameterSyntax syntax;
        
        internal Parameter(ParameterSyntax syntax, SyntaxNode parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(ParameterSyntax syntax)
        {
            this.syntax = syntax;
            name = new Identifier(syntax.Identifier);
        }

        public Parameter(TypeReference type, string name)
        {
            Type = type;
            Name = name;
        }

        // TODO: modifiers, default, attributes
        
        private TypeReference type;
        public TypeReference Type
        {
            get
            {
                if (type == null)
                    type = FromRoslyn.TypeReference(syntax.Type, this);

                return type;
            }
            set => SetNotNull(ref type, value);
        }

        
        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }
        
        ParameterSyntax ISyntaxWrapper<ParameterSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);
            
            bool? thisChanged = false;
            
            var newType = type?.GetWrapped(ref thisChanged) ?? syntax.Type;
            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true || !IsAnnotated(syntax))
            {
                var newSyntax = RoslynSyntaxFactory.Parameter(default, default, newType, newName, default);

                syntax = Annotate(newSyntax);
                
                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((ParameterSyntax)newSyntax);
            Set(ref type, null);
        }

        internal override SyntaxNode Clone() => new Parameter(Type, Name);

        internal override SyntaxNode Parent { get; set; }
    }
}