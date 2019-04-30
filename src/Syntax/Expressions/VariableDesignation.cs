using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public abstract class VariableDesignation : SyntaxNode, ISyntaxWrapper<VariableDesignationSyntax>
    {
        VariableDesignationSyntax ISyntaxWrapper<VariableDesignationSyntax>.GetWrapped(ref bool? changed) =>
            GetWrapped(ref changed);

        protected abstract VariableDesignationSyntax GetWrapped(ref bool? changed);
    }

    public sealed class SingleVariableDesignation : VariableDesignation
    {
        private VariableDesignationSyntax syntax;

        internal SingleVariableDesignation(VariableDesignationSyntax syntax, SyntaxNode parent)
        {
            Debug.Assert(syntax is DiscardDesignationSyntax || syntax is SingleVariableDesignationSyntax);

            Init(syntax);
            Parent = parent;
        }

        private void Init(VariableDesignationSyntax syntax)
        {
            this.syntax = syntax;

            switch (syntax)
            {
                case DiscardDesignationSyntax _:
                    name = new Identifier("_");
                    break;
                case SingleVariableDesignationSyntax singleVariable:
                    name = new Identifier(singleVariable.Identifier);
                    break;
            }
        }

        public SingleVariableDesignation(string name) => Name = name;

        private Identifier name;
        public string Name
        {
            get => name.Text;
            set => name.Text = value;
        }

        protected override VariableDesignationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newName = name.GetWrapped(ref thisChanged);

            if (syntax == null || thisChanged == true)
            {
                if (newName.ValueText == "_")
                    syntax = RoslynSyntaxFactory.DiscardDesignation();
                else
                    syntax = RoslynSyntaxFactory.SingleVariableDesignation(newName);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax) =>
            Init((VariableDesignationSyntax)newSyntax);

        private protected override SyntaxNode CloneImpl() => new SingleVariableDesignation(Name);
    }

    public sealed class MultiVariableDesignation : VariableDesignation
    {
        private ParenthesizedVariableDesignationSyntax syntax;

        internal MultiVariableDesignation(ParenthesizedVariableDesignationSyntax syntax, SyntaxNode parent)
        {
            this.syntax = syntax;
            Parent = parent;
        }

        public MultiVariableDesignation(params VariableDesignation[] variables) : this(variables.AsEnumerable()) { }

        public MultiVariableDesignation(IEnumerable<VariableDesignation> variables) =>
            this.variables = new VariableDesignationList(variables, this);

        private VariableDesignationList variables;
        public IList<VariableDesignation> Variables
        {
            get
            {
                if (variables == null)
                    variables =
                        new VariableDesignationList(syntax.Variables, this);

                return variables;
            }
            set => SetList(
                ref variables,
                new VariableDesignationList(value, this));
        }

        protected override VariableDesignationSyntax GetWrapped(ref bool? changed)
        {
            GetAndResetChanged(ref changed);

            bool? thisChanged = false;

            var newVariables = variables?.GetWrapped(ref thisChanged) ?? syntax.Variables;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.ParenthesizedVariableDesignation(newVariables);

                SetChanged(ref changed);
            }

            return syntax;
        }

        internal override SyntaxNode Parent { get; set; }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            syntax = (ParenthesizedVariableDesignationSyntax)newSyntax;
            SetList(ref variables, null);
        }

        private protected override SyntaxNode CloneImpl() => new MultiVariableDesignation(Variables);

        public override IEnumerable<SyntaxNode> GetChildren() => Variables;
    }
}