using System.Collections.Generic;
using System.Linq;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynSyntaxFactory = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Roslyn = Microsoft.CodeAnalysis;

namespace CSharpE.Syntax
{
    public sealed class TypeParameterConstraintClause : SyntaxNode, ISyntaxWrapper<TypeParameterConstraintClauseSyntax>
    {
        private TypeParameterConstraintClauseSyntax syntax;

        internal TypeParameterConstraintClause(TypeParameterConstraintClauseSyntax syntax, TypeDefinition parent)
            : base(syntax)
        {
            Init(syntax);
            Parent = parent;
        }

        private void Init(TypeParameterConstraintClauseSyntax syntax)
        {
            this.syntax = syntax;
            typeParameterName = new Identifier(syntax.Name.Identifier);
        }

        public TypeParameterConstraintClause(string typeParameterName, params TypeParameterConstraint[] constraints)
            : this(typeParameterName, constraints.AsEnumerable()) { }

        public TypeParameterConstraintClause(string typeParameterName, IEnumerable<TypeParameterConstraint> constraints)
        {
            TypeParameterName = typeParameterName;
            this.constraints = new TypeParameterConstraintList(constraints, this);
        }

        private Identifier typeParameterName;
        public string TypeParameterName
        {
            get => typeParameterName.Text;
            set => typeParameterName.Text = value;
        }

        private TypeParameterConstraintList constraints;
        public IList<TypeParameterConstraint> Constraints
        {
            get
            {
                if (constraints == null)
                    constraints = new TypeParameterConstraintList(syntax.Constraints, this);

                return constraints;
            }
            set => SetList(ref constraints, new TypeParameterConstraintList(value, this));
        }

        TypeParameterConstraintClauseSyntax ISyntaxWrapper<TypeParameterConstraintClauseSyntax>.GetWrapped(
            ref bool? changed)
        {
            GetAndResetChanged(ref changed, out var thisChanged);

            var newName = typeParameterName.GetWrapped(ref thisChanged);
            var newConstraints = constraints?.GetWrapped(ref thisChanged) ?? syntax.Constraints;

            if (syntax == null || thisChanged == true)
            {
                syntax = RoslynSyntaxFactory.TypeParameterConstraintClause(
                    RoslynSyntaxFactory.IdentifierName(newName), newConstraints);

                SetChanged(ref changed);
            }

            return syntax;
        }

        private protected override void SetSyntaxImpl(Roslyn::SyntaxNode newSyntax)
        {
            Init((TypeParameterConstraintClauseSyntax)newSyntax);
            SetList(ref constraints, null);
        }

        private protected override SyntaxNode CloneImpl() =>
            new TypeParameterConstraintClause(TypeParameterName, Constraints);

        public override IEnumerable<SyntaxNode> GetChildren() => Constraints;
    }
}
