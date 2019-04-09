using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn = Microsoft.CodeAnalysis;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CSharpE.Syntax
{
    // TODO: attributes
    public sealed class EnumMemberDefinition : SyntaxNode, ISyntaxWrapper<EnumMemberDeclarationSyntax>
    {
        private EnumMemberDeclarationSyntax syntax;

        internal EnumMemberDefinition(EnumMemberDeclarationSyntax syntax, EnumDefinition parent)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(EnumMemberDeclarationSyntax enumMemberDeclarationSyntax)
        {
            syntax = enumMemberDeclarationSyntax;

            name = new Identifier(enumMemberDeclarationSyntax.Identifier);
        }

        public EnumMemberDefinition(string name, Expression initializer = null)
        {
            Name = name;
            Initializer = initializer;
        }

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        private bool initializerSet;
        private Expression initializer;
        public Expression Initializer
        {
            get
            {
                if (!initializerSet)
                {
                    var syntaxInitializer = syntax.EqualsValue;

                    if (syntaxInitializer != null)
                        initializer = FromRoslyn.Expression(syntaxInitializer.Value, this);

                    initializerSet = true;
                }

                return initializer;
            }
            set
            {
                Set(ref initializer, value);
                initializerSet = true;
            }
        }

        EnumMemberDeclarationSyntax ISyntaxWrapper<EnumMemberDeclarationSyntax>.GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);
            var newInitializer = initializerSet ? initializer?.GetWrapped(ref thisChanged) : syntax.EqualsValue?.Value;

            if (syntax == null || thisChanged == true)
            {
                var equalsValue = newInitializer == null ? null : RoslynSyntaxFactory.EqualsValueClause(newInitializer);

                syntax = RoslynSyntaxFactory.EnumMemberDeclaration(default, newName, equalsValue);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((EnumMemberDeclarationSyntax)newSyntax);
            Set(ref initializer, null);
            initializerSet = false;
        }

        internal override SyntaxNode Clone() => new EnumMemberDefinition(Name, Initializer);
    }
}